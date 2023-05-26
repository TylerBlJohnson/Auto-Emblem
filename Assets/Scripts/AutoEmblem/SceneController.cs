using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    //Inventory
    [SerializeField] private int defaultFunds = 3;
    [SerializeField] private int funds = 0;

    // Other Controllers and Buttons
    [SerializeField] private SoundController soundController;
    [SerializeField] private NameGenerator nameGenerator;
    [SerializeField] private Button startButton;
    [SerializeField] private Button nextLevelButton;

    // Selected units
    private Statsheet unit1;
    private Statsheet unit2;
    private ShopItem selectedShopItem;

    // Army / Shop Arrays
    private GameObject[,] allyUnits;
    private GameObject[,] enemyUnits;

    // Constants
    private readonly int numCols = 2;
    private readonly int numRows = 4;
    private readonly int defaultLevel = 1;
    private readonly int numUnitsEachSide = 5;

    // Unit Spawn Points
    [SerializeField] private Transform allySpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject emptyUnitPrefab;
    [SerializeField] private GameObject bossPrefab;

    // Non-UI Display Values
    [SerializeField] private GameObject damageText;
    [SerializeField] private GameObject levelUpSymbol;
    [SerializeField] private GameObject unitCursor;
    [SerializeField] private GameObject shopCursor;
    [SerializeField] private TextMesh currentLevelMesh;
    [SerializeField] private TextMesh currentScoreMesh;
    [SerializeField] private TextMesh victoryTextMesh;
    [SerializeField] private TextMesh fundsTextMesh;
    private string victory = "You win!";
    private string defeat = "You lose...";
    [SerializeField] private int currentLevel = 0;
    private Vector3 hideObject = new Vector3(0, -10, 0); // Where to hide the above GameObjects when not in use

    // UI Display values
    [SerializeField] private Image unitSprite;
    [SerializeField] private Text nameValue;
    [SerializeField] private Text levelValue;
    [SerializeField] private Text expValue;
    [SerializeField] private Text HPValue;
    [SerializeField] private Text atkValue;
    [SerializeField] private Text spdValue;
    [SerializeField] private Text defValue;
    [SerializeField] private Text resValue;

    // Combat values
    private bool isDoCombat = false;
    private bool[,] allyUnitCanAttack;
    private bool[,] enemyUnitCanAttack;

    private void Awake()
    {
        Messenger<ShopItem>.AddListener(GameEvent.SHOP_ITEM_CLICKED, OnShopItemClicked);
        Messenger<Statsheet>.AddListener(GameEvent.STATSHEET_CLICKED, OnUnitClicked);
    }

    private void OnDestroy()
    {
        Messenger<ShopItem>.RemoveListener(GameEvent.SHOP_ITEM_CLICKED, OnShopItemClicked);
        Messenger<Statsheet>.RemoveListener(GameEvent.STATSHEET_CLICKED, OnUnitClicked);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (!isDoCombat)
        {
            if (Input.GetMouseButtonDown(1))
            {
                DeselectUnits();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        fundsTextMesh.text = funds.ToString();
    }

    public void Reset()
    {
        startButton.interactable = true;
        nextLevelButton.interactable = false;
        victoryTextMesh.text = "";
        funds = defaultFunds;

        if (allyUnits != null)
        {
            for (int x = 0; x < numCols; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    Destroy(allyUnits[x, y]);
                    Destroy(enemyUnits[x, y]);
                }
            }
        }
        
        allyUnits = new GameObject[numCols, numRows];
        enemyUnits = new GameObject[numCols, numRows];
        SpawnNewUnitsBothSides(numUnitsEachSide, numUnitsEachSide);
        StopAllCoroutines();
        DeselectUnits();

        if (isDoCombat)
        {
            iTween.Stop(damageText);
            iTween.Stop(levelUpSymbol);
            Messenger.Broadcast(GameEvent.COMBAT_STATE_CHANGED);
            isDoCombat = false;
        }
        levelUpSymbol.transform.position = hideObject;
        damageText.transform.position = hideObject;

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                ResetUnit(allyUnits[x, y], Statsheet.Faction.Ally);
            }
        }

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                ResetUnit(enemyUnits[x, y], Statsheet.Faction.Enemy);
            }
        }

        StartCoroutine(MoveUnitsInFromOffScreen());
        currentLevel = 0;
        currentLevelMesh.text = "Level: " + ((int)currentLevel + 1);
        Messenger.Broadcast(GameEvent.NEXT_LEVEL);
        UpdateScore();
    }

    public void NextLevel()
    {
        if (!isDoCombat)
        {
            currentLevel++;
            currentLevelMesh.text = "Level: " + ((int)currentLevel + 1);
            funds += 2;
            victoryTextMesh.text = "";
            int totalLevel = 0;
            int numAllies = 0;
            for (int x = 0; x < numCols; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    Destroy(enemyUnits[x, y]);

                    allyUnits[x, y].SetActive(true);
                    Statsheet ss = allyUnits[x, y].GetComponent<Statsheet>();
                    if (!ss.GetIsEmptyUnit())
                    {
                        ss.HealDamage(999);
                        totalLevel += ss.GetStat(Statsheet.Stat.Level);
                        numAllies++;
                        ss.UpdateIsMagic();
                    }
                }
            }

            enemyUnits = new GameObject[numCols, numRows];
            DeselectUnits();

            int bossLevel = 10;
            if (currentLevel != bossLevel - 1 || bossPrefab == null)
            {
                SpawnNewUnits(numUnitsEachSide, Statsheet.Faction.Enemy);
                int avgLvl = totalLevel / numAllies;
                for (int y = 0; y < numRows; y++)
                {
                    for (int x = 0; x < numCols; x++)
                    {
                        ResetUnit(enemyUnits[x, y], Statsheet.Faction.Enemy, avgLvl);
                    }
                }
            }
            else
            {
                SpawnNewUnits(0, Statsheet.Faction.Enemy);
                Destroy(enemyUnits[1, 1]);
                GameObject boss = Instantiate(bossPrefab);
                boss.transform.position += new Vector3(0, 0, -1);
                enemyUnits[1, 1] = boss;
            }

            nextLevelButton.interactable = false;
            startButton.interactable = true;
            StartCoroutine(MoveUnitsInFromOffScreen());
            Messenger.Broadcast(GameEvent.NEXT_LEVEL);
            UpdateScore();
        }
    }

    private void ResetUnit(GameObject unit, Statsheet.Faction faction)
    {
        ResetUnit(unit, faction, defaultLevel);
    }

    private void ResetUnit(GameObject unit, Statsheet.Faction faction, int level)
    {
        if (unit != null)
        {
            Statsheet ss = unit.GetComponent<Statsheet>();
            if (ss != null)
            {
                if (!ss.GetIsEmptyUnit())
                {
                    unit.SetActive(true);
                    int growths = 280;
                    if (faction == Statsheet.Faction.Enemy)
                    {
                        growths += (currentLevel + 1) * 20;
                    }
                    ss.GenerateRandomStatsheet(level, growths, 20, faction);
                }
            }
        }
        else
        {
            //Debug.LogError("ERROR IN resetUnit - passed null unit.");
        }
    }

    private void OnMouseDown() // SceneController covers the entire background and is used to detect when the player clicks nothing in particular
    {
        DeselectUnits();
    }

    private void DeselectUnits()
    {
        unit1 = null;
        unit2 = null;
        selectedShopItem = null;
        DisplayUnitStats(null);
        MoveCursorTo(null);
    }

    private void DisplayUnitStats(Statsheet unit, Sprite unitSprite = null)
    {
        if (unit != null && !unit.GetIsEmptyUnit())
        {
            this.unitSprite.sprite = unitSprite;
            //Debug.Log("Displaying " + attacker.getName());
            nameValue.text = unit.GetName();
            levelValue.text = unit.GetStat(Statsheet.Stat.Level).ToString();
            HPValue.text = unit.GetCurStat(Statsheet.Stat.HP).ToString() + " / " + unit.GetStat(Statsheet.Stat.HP).ToString();
            atkValue.text = unit.GetCurStat(Statsheet.Stat.Atk).ToString();
            spdValue.text = unit.GetCurStat(Statsheet.Stat.Spd).ToString();
            defValue.text = unit.GetCurStat(Statsheet.Stat.Def).ToString();
            resValue.text = unit.GetCurStat(Statsheet.Stat.Res).ToString();

            if (unit.GetFaction() == Statsheet.Faction.Ally) // The enemy army does not earn EXP and as such doesn't need to display that value
            {
                expValue.text = unit.GetStat(Statsheet.Stat.Exp).ToString() + " / " + unit.DetermineEXPRequiredToLevelUp().ToString();
            }
            else
            {
                expValue.text = "";
            }
        }
        else
        {
            this.unitSprite.sprite = null;
            nameValue.text = "";
            levelValue.text = "";
            expValue.text = "";
            HPValue.text = "";
            atkValue.text = "";
            spdValue.text = "";
            defValue.text = "";
            resValue.text = "";
        }
    }

    private void MoveCursorTo(GameObject gameObject, bool isUnitCursor = true)
    {
        if (gameObject == null)
        {
            unitCursor.transform.position = hideObject;
            shopCursor.transform.position = hideObject;
        }
        else
        {
            if (isUnitCursor)
            {
                if (unitCursor != null)
                {
                    unitCursor.transform.position = gameObject.transform.position;
                }
            }
            else
            {
                if (shopCursor != null)
                {
                    shopCursor.transform.position = gameObject.transform.position;
                }
            }
        }
    }

    private void OnUnitClicked(Statsheet unit)
    {
        UnitClicked(unit.gameObject);
    }

    public void UnitClicked(GameObject unit)
    {
        if (!isDoCombat)
        {
            Statsheet unitStatsheet = unit.GetComponent<Statsheet>();
            SpriteRenderer unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
            
            if (selectedShopItem != null && unitStatsheet.GetFaction() == Statsheet.Faction.Enemy)
            {
                DeselectUnits();
            }

            if (selectedShopItem == null)
            {
                if (!(unitStatsheet.GetIsEmptyUnit() && unitStatsheet.GetFaction() == Statsheet.Faction.Enemy))
                {
                    MoveCursorTo(unit);
                }
                else
                {
                    DeselectUnits();
                }

                if (unitStatsheet == null || unitSpriteRenderer == null)
                {
                    Debug.LogError("UNITCLICKED ERROR: GameObject either doesn't have a Statsheet or a SpriteRenderer attached");
                }
                else
                {
                    DisplayUnitStats(unitStatsheet, unitSpriteRenderer.sprite);
                    if (unitStatsheet.GetFaction() == Statsheet.Faction.Enemy)
                    {
                        unit1 = null;
                        unit2 = null; //Pretty sure unit2 would already be null at this point but better safe than sorry
                    }
                    else if (unit1 == null || (unit1.GetIsEmptyUnit() && unitStatsheet.GetIsEmptyUnit()))
                    {
                        unit1 = unitStatsheet;
                    }
                    else if (unit2 == null && unit1.gameObject != unit)
                    {
                        unit2 = unitStatsheet;

                        if (unit1.GetFaction() == unit2.GetFaction())
                        {
                            StartCoroutine(SwapPlaces(unit1.gameObject, unit2.gameObject));
                        }

                        DeselectUnits();
                    }
                }
            }
            else
            {
                if (!unitStatsheet.GetIsEmptyUnit() && unitStatsheet.GetFaction() == Statsheet.Faction.Ally)
                {
                    MoveCursorTo(unit);
                }
                else
                {
                    DeselectUnits();
                }

                if (unit1 == null || unit1 != unitStatsheet)
                {
                    unit1 = unitStatsheet;
                    DisplayUnitStats(unit1, unitSpriteRenderer.sprite);
                }
                else if (unit1 == unitStatsheet)
                {
                    // Using item on ally
                    funds -= selectedShopItem.GetPrice();
                    soundController.PlaySfx(GameAssets.Instance.stat_up);
                    StartCoroutine(DisplayLevelUp(unit, false));
                    unitStatsheet.IncrementStat(selectedShopItem.GetStatToBoost(), selectedShopItem.GetBoost());
                    selectedShopItem.gameObject.SetActive(false);
                    DeselectUnits();
                    DisplayUnitStats(unitStatsheet, unitSpriteRenderer.sprite);
                    UpdateScore();
                }
            }
        }
    }

    private void OnShopItemClicked(ShopItem shopItem)
    {
        DeselectUnits();
        if (funds >= shopItem.GetPrice())
        {
            selectedShopItem = shopItem;
            MoveCursorTo(shopItem.gameObject, false);
        }
        else
        {
            soundController.PlaySfx(GameAssets.Instance.not_allowed);
        }
    }

    public void StartCombat()
    {
        if (!isDoCombat)
        {
            StartCoroutine(GroupCombat());
        }
        //isDoCombat = false;
    }

    private IEnumerator GroupCombat()
    {
        //Combat started
        isDoCombat = true;
        startButton.interactable = false;
        DisplayUnitStats(null);
        Messenger.Broadcast(GameEvent.COMBAT_STATE_CHANGED);
        allyUnitCanAttack = new bool[numCols, numRows];
        enemyUnitCanAttack = new bool[numCols, numRows];

        // Initialize allyUnitCanAttack and enemyUnitCanAttack
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                Statsheet ss = allyUnits[x, y].GetComponent<Statsheet>();
                if (ss != null)
                {
                    if (!allyUnits[x, y].activeSelf || ss.GetIsEmptyUnit())
                    {
                        allyUnitCanAttack[x, y] = false;
                    }
                    else
                    {
                        allyUnitCanAttack[x, y] = true;
                    }
                }
                else
                {
                    Debug.LogError("ERROR IN GroupCombat() - Statsheet does not exist.");
                }

                ss = enemyUnits[x, y].GetComponent<Statsheet>();
                if (ss != null)
                {
                    if (!enemyUnits[x, y].activeSelf || ss.GetIsEmptyUnit())
                    {
                        enemyUnitCanAttack[x, y] = false;
                    }
                    else
                    {
                        enemyUnitCanAttack[x, y] = true;
                    }
                }
                else
                {
                    Debug.LogError("ERROR IN GroupCombat() - Statsheet does not exist.");
                }
            }
        }


        //Combat System
        int xIndex = 0;
        int yIndex = 1;
        int factionIndex = 2;

        while ((IsAnyTrue(allyUnitCanAttack)
            && IsAnyTrue(enemyUnitCanAttack))
            || IsAnyActive(allyUnits)
            || IsAnyActive(enemyUnits))
        {
            int[] combatant = DetermineNextCombatant();

            if (combatant == null)
            {
                break;
            }

            int[] opposingCombatant = DetermineOpposingCombatant(combatant);
            Statsheet combatantSS;
            Statsheet opposingCombatantSS;

            if (combatant == null || opposingCombatant == null)
            {
                //Debug.LogError("ERROR IN GroupCombat() - combatant or opposingCombatant do not exist");
                //isDoCombat = false;
                //Messenger.Broadcast(GameEvent.COMBAT_STATE_CHANGED);
                break;
            }

            //error check
            if (combatant[factionIndex] == opposingCombatant[factionIndex])
            {
                Debug.LogError("ERROR IN GroupCombat() - Combatants should not be part of the same faction");
            }

            if (combatant[factionIndex] == (int)Statsheet.Faction.Ally)
            {
                combatantSS = allyUnits[combatant[xIndex], combatant[yIndex]].GetComponent<Statsheet>();
                opposingCombatantSS = enemyUnits[opposingCombatant[xIndex], opposingCombatant[yIndex]].GetComponent<Statsheet>();
                allyUnitCanAttack[combatant[xIndex], combatant[yIndex]] = false;
            }
            else
            {
                combatantSS = enemyUnits[combatant[xIndex], combatant[yIndex]].GetComponent<Statsheet>();
                opposingCombatantSS = allyUnits[opposingCombatant[xIndex], opposingCombatant[yIndex]].GetComponent<Statsheet>();
                enemyUnitCanAttack[combatant[xIndex], combatant[yIndex]] = false;
            }
            yield return StartCoroutine(Combat(combatantSS, opposingCombatantSS));
        }


        //Determine the victor
        int allyHP = TeamTotalHP(allyUnits);
        int enemyHP = TeamTotalHP(enemyUnits);
        //Debug.Log("Score: " + allyHP + " - " + enemyHP);
        if (allyHP >= enemyHP)
        {
            victoryTextMesh.text = victory;
            nextLevelButton.interactable = true;
            startButton.interactable = false;
        }
        else
        {
            victoryTextMesh.text = defeat;
            nextLevelButton.interactable = false;
            startButton.interactable = false;
        }

        //Combat ended
        isDoCombat = false;
        Messenger.Broadcast(GameEvent.COMBAT_STATE_CHANGED);
    }

    private int TeamTotalHP(GameObject[,] team)
    {
        int totalHP = 0;
        for (int x = 0; x < numCols; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                //Debug.Log(x + " " + y);
                Statsheet ss = team[x, y].GetComponent<Statsheet>();
                if (ss != null)
                {
                    if (team[x, y].activeSelf && !ss.GetIsEmptyUnit())
                    {
                        totalHP += ss.GetCurStat(Statsheet.Stat.HP);
                    }
                }
                
            }
        }
        return totalHP;
    }

    private bool IsAnyTrue(bool[,] boolArr)
    {
        for (int x = 0; x < numCols; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                if (boolArr[x, y])
                {
                    //Debug.Log("There is a true at " + x + " " + y);
                    return true;
                }
            }
        }
        //Debug.Log("There was no true");
        return false;
    }

    private bool IsAnyActive(GameObject[,] GOArr)
    {
        for (int x = 0; x < numCols; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                Statsheet ss = GOArr[x, y].GetComponent<Statsheet>();
                if (GOArr[x, y].activeSelf && !ss.GetIsEmptyUnit())
                {
                    //Debug.Log("There is an active at " + x + " " + y);
                    return true;
                }
            }
        }
        //Debug.Log("There was no active");
        return false;
    }

    private int[] DetermineNextCombatant()
    {
        int[] unit = new int[3]; //[x, y, Statsheet.Faction]
        int xIndex = 0;
        int yIndex = 1;
        int factionIndex = 2;
        unit[factionIndex] = -1; //This will be used to determine whether an initial combatant has been found or not

        int x;
        //Determine first unit
        for (x = 0; x < numCols && unit[factionIndex] == -1; x++)
        {
            //Debug.Log(x);
            //First, check x row of allies
            for (int y = 0; y < numRows; y++)
            {
                //Debug.Log(x + " " + y);
                if (allyUnitCanAttack[x, y])
                {
                    unit[xIndex] = x;
                    unit[yIndex] = y;
                    unit[factionIndex] = (int)Statsheet.Faction.Ally;
                    break;
                }
            }
            //Next, check x row of enemies
            if (unit[factionIndex] == -1)
            {
                for (int y = 0; y < numRows; y++)
                {
                    if (enemyUnitCanAttack[x, y])
                    {
                        unit[xIndex] = x;
                        unit[yIndex] = y;
                        unit[factionIndex] = (int)Statsheet.Faction.Enemy;
                    }
                }
            }
            else
            {
                break;
            }
        }

        if (unit[factionIndex] == -1)
        {
            //Debug.Log("Failed to find a combatant");
            return null;
        }

        //Determine if there's a better suited combatant
        x = unit[xIndex]; //Only need to check whichever row first unit was on
        if (unit[factionIndex] == (int)Statsheet.Faction.Ally)
        {
            for (int y = unit[yIndex]; y < numRows; y++)
            {
                if (allyUnitCanAttack[x, y])
                {
                    Statsheet curUnit = allyUnits[unit[xIndex], unit[yIndex]].GetComponent<Statsheet>();
                    Statsheet nextUnit = allyUnits[x, y].GetComponent<Statsheet>();
                    int curUnitLevel = curUnit.GetStat(Statsheet.Stat.Level);
                    int nextUnitLevel = nextUnit.GetStat(Statsheet.Stat.Level);

                    if (curUnitLevel < nextUnitLevel)
                    {
                        unit[xIndex] = x;
                        unit[yIndex] = y;
                        //No need to change faction, faction is already ally
                    }
                    else if (curUnitLevel == nextUnitLevel)
                    {
                        int curUnitSpeed = curUnit.GetCurStat(Statsheet.Stat.Spd);
                        int nextUnitSpeed = nextUnit.GetCurStat(Statsheet.Stat.Spd);

                        if (curUnitSpeed < nextUnitSpeed)
                        {
                            unit[xIndex] = x;
                            unit[yIndex] = y;
                        }
                    }
                }
            }

            //Now we check the enemies
            for (int y = 0; y < numRows; y++)
            {
                if (enemyUnitCanAttack[x, y])
                {
                    Statsheet curUnit;
                    if (unit[factionIndex] == (int)Statsheet.Faction.Ally)
                    {
                        curUnit = allyUnits[unit[xIndex], unit[yIndex]].GetComponent<Statsheet>();
                    }
                    else
                    {
                        curUnit = enemyUnits[unit[xIndex], unit[yIndex]].GetComponent<Statsheet>();
                    }
                    Statsheet nextUnit = enemyUnits[x, y].GetComponent<Statsheet>();
                    int curUnitLevel = curUnit.GetStat(Statsheet.Stat.Level);
                    int nextUnitLevel = nextUnit.GetStat(Statsheet.Stat.Level);
                    if (curUnitLevel < nextUnitLevel)
                    {
                        unit[xIndex] = x;
                        unit[yIndex] = y;
                        unit[factionIndex] = (int)Statsheet.Faction.Enemy;
                    }
                    else if (curUnitLevel == nextUnitLevel)
                    {
                        int curUnitSpeed = curUnit.GetCurStat(Statsheet.Stat.Spd);
                        int nextUnitSpeed = nextUnit.GetCurStat(Statsheet.Stat.Spd);

                        if (curUnitSpeed < nextUnitSpeed)
                        {
                            unit[xIndex] = x;
                            unit[yIndex] = y;
                            unit[factionIndex] = (int)Statsheet.Faction.Enemy;
                        }
                    }
                }
            }
        }
        else
        {
            for (int y = unit[yIndex]; y < numRows; y++)
            {
                if (enemyUnitCanAttack[x, y])
                {
                    Statsheet curUnit = enemyUnits[unit[xIndex], unit[yIndex]].GetComponent<Statsheet>();
                    Statsheet nextUnit = enemyUnits[x, y].GetComponent<Statsheet>();
                    int curUnitLevel = curUnit.GetStat(Statsheet.Stat.Level);
                    int nextUnitLevel = nextUnit.GetStat(Statsheet.Stat.Level);

                    if (curUnitLevel < nextUnitLevel)
                    {
                        unit[xIndex] = x;
                        unit[yIndex] = y;
                        //No need to change faction, faction is already enemy
                    }
                    else if (curUnitLevel == nextUnitLevel)
                    {
                        int curUnitSpeed = curUnit.GetCurStat(Statsheet.Stat.Spd);
                        int nextUnitSpeed = nextUnit.GetCurStat(Statsheet.Stat.Spd);

                        if (curUnitSpeed < nextUnitSpeed)
                        {
                            unit[xIndex] = x;
                            unit[yIndex] = y;
                        }
                    }
                }
            }
        }
        return unit;
    }

    private int[] DetermineOpposingCombatant(int[] unit)
    {
        if (unit.Length != 3)
        {
            Debug.LogError("Unit array length must equal 3!");
            Debug.Break();
        }
        int xIndex = 0;
        int yIndex = 1;
        int factionIndex = 2;

        bool unitIsAlly = unit[factionIndex] == (int)Statsheet.Faction.Ally;
        int frontCol = 0;
        int backCol = 1;
        int topRow = 0;
        int bottomRow = numRows - 1;
        Statsheet.Faction faction = (Statsheet.Faction)unit[factionIndex];

        int[] opposingCombatant = { frontCol, unit[yIndex], (int)Statsheet.Faction.Ally };
        Statsheet ss;
        if (unit[factionIndex] == (int)Statsheet.Faction.Ally)
        {
            opposingCombatant[factionIndex] = (int)Statsheet.Faction.Enemy;
        }

        GameObject compareUnit;

        //If unit is on the top row
        if (unit[yIndex] == topRow)
        {
            for (int y = 0; y < numRows; y++)
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[frontCol, y];
                }
                else
                {
                    compareUnit = allyUnits[frontCol, y];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                //Debug.Log(unit + "\n"
                //    + frontCol + ":" + y + "\n"
                //    + "isEmptyUnit: " + ss.GetIsEmptyUnit() + "\n"
                //    + "HP: " + ss.GetStat(Statsheet.Stat.HP));
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = frontCol;
                    opposingCombatant[yIndex] = y;
                    //Debug.Log("Opponent is empty: " + ss.GetIsEmptyUnit());
                    return opposingCombatant;
                }
                else
                {
                    if (unitIsAlly)
                    {
                        compareUnit = enemyUnits[backCol, y];
                    }
                    else
                    {
                        compareUnit = allyUnits[backCol, y];
                    }
                    ss = compareUnit.GetComponent<Statsheet>();
                    if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                    {
                        opposingCombatant[xIndex] = backCol;
                        opposingCombatant[yIndex] = y;
                        return opposingCombatant;
                    }
                }
            }
            //Debug.LogError("DetermineOpposingCombatant() called without a valid enemy target.");
            return null;
        }

        //If unit is on the bottom row
        else if (unit[yIndex] == numRows - 1)
        {
            for (int y = numRows - 1; y >= 0; y--)
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[frontCol, y];
                }
                else
                {
                    compareUnit = allyUnits[frontCol, y];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = frontCol;
                    opposingCombatant[yIndex] = y;
                    return opposingCombatant;
                }
                else
                {
                    if (unitIsAlly)
                    {
                        compareUnit = enemyUnits[backCol, y];
                    }
                    else
                    {
                        compareUnit = allyUnits[backCol, y];
                    }
                    ss = compareUnit.GetComponent<Statsheet>();
                    if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                    {
                        opposingCombatant[xIndex] = backCol;
                        opposingCombatant[yIndex] = y;
                        return opposingCombatant;
                    }
                }
            }
            //Debug.LogError("DetermineOpposingCombatant() called without a valid enemy target.");
            //Debug.Break();
            return null;
        }

        //If unit is in one of the middle rows
        else
        {
            if (numRows != 4)
            {
                Debug.LogError("ERROR IN DetermineOpposingCombatant(): function is meant to be used in a 2x4 grid");
                Debug.Break();
            }

            //Check same row
            for (int x = 0; x < numCols; x++)
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[x, unit[yIndex]];
                }
                else
                {
                    compareUnit = allyUnits[x, unit[yIndex]];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = x;
                    opposingCombatant[yIndex] = unit[yIndex];
                    return opposingCombatant;
                }
            }

            //Check front of adjacent rows
            if (unitIsAlly)
            {
                compareUnit = enemyUnits[frontCol, unit[yIndex] - 1];
            }
            else
            {
                compareUnit = allyUnits[frontCol, unit[yIndex] - 1];
            }
            ss = compareUnit.GetComponent<Statsheet>();
            if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
            {
                opposingCombatant[xIndex] = frontCol;
                opposingCombatant[yIndex] = unit[yIndex] - 1;
                return opposingCombatant;
            }
            else
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[frontCol, unit[yIndex] + 1];
                }
                else
                {
                    compareUnit = allyUnits[frontCol, unit[yIndex] + 1];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = frontCol;
                    opposingCombatant[yIndex] = unit[yIndex] + 1;
                    return opposingCombatant;
                }
            }

            //Check back of adjacent rows
            if (unitIsAlly)
            {
                compareUnit = enemyUnits[backCol, unit[yIndex] - 1];
            }
            else
            {
                compareUnit = allyUnits[backCol, unit[yIndex] - 1];
            }
            ss = compareUnit.GetComponent<Statsheet>();
            if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
            {
                opposingCombatant[xIndex] = backCol;
                opposingCombatant[yIndex] = unit[yIndex] - 1;
                return opposingCombatant;
            }
            else
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[backCol, unit[yIndex] + 1];
                }
                else
                {
                    compareUnit = allyUnits[backCol, unit[yIndex] + 1];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = backCol;
                    opposingCombatant[yIndex] = unit[yIndex] + 1;
                    return opposingCombatant;
                }
            }

            //Check remaining row
            int checkRow;
            if (unit[yIndex] == 1)
            {
                checkRow = numRows - 1;
            }
            else if (unit[yIndex] == 2)
            {
                checkRow = topRow;
            }
            else
            {
                Debug.LogError("ERROR IN determineOpposingCombatant() - unit[yIndex] is an illegal value.");
                Debug.Break();
                checkRow = -1;
            }

            if (unitIsAlly)
            {
                compareUnit = enemyUnits[frontCol, checkRow];
            }
            else
            {
                compareUnit = allyUnits[frontCol, checkRow];
            }

            ss = compareUnit.GetComponent<Statsheet>();
            if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
            {
                opposingCombatant[xIndex] = frontCol;
                opposingCombatant[yIndex] = checkRow;
                return opposingCombatant;
            }
            else
            {
                if (unitIsAlly)
                {
                    compareUnit = enemyUnits[backCol, checkRow];
                }
                else
                {
                    compareUnit = allyUnits[backCol, checkRow];
                }
                ss = compareUnit.GetComponent<Statsheet>();
                if (compareUnit.activeSelf && !ss.GetIsEmptyUnit())
                {
                    opposingCombatant[xIndex] = backCol;
                    opposingCombatant[yIndex] = checkRow;
                    return opposingCombatant;
                }
            }

            //Debug.LogError("DetermineOpposingCombatant() called without a valid enemy target.");
            return null;
        }
    }

    private IEnumerator Combat(Statsheet unit1, Statsheet unit2)
    {
        yield return StartCoroutine(AnimateUnitAttacking(unit1, unit2, false));
        
        //unit2 will be inactive if they're now dead
        if (unit2.gameObject.activeSelf)
        {
            yield return StartCoroutine(AnimateUnitAttacking(unit2, unit1, false));

            if (unit1.gameObject.activeSelf)
            {
                int unit1Spd = unit1.GetCurStat(Statsheet.Stat.Spd);
                int unit2Spd = unit2.GetCurStat(Statsheet.Stat.Spd);

                if (unit1Spd > unit2Spd)
                {
                    yield return StartCoroutine(AnimateUnitAttacking(unit1, unit2, true));
                }
                else if (unit1Spd < unit2Spd)
                {
                    yield return StartCoroutine(AnimateUnitAttacking(unit2, unit1, true));
                }
            }
        }

        if (unit1.GetFaction() == Statsheet.Faction.Ally)
        {
            int level = unit1.GetStat(Statsheet.Stat.Level);
            GiveAllyExperience(ref unit1, unit2);
            if (unit1.GetStat(Statsheet.Stat.Level) != level)
            {
                StartCoroutine(DisplayLevelUp(unit1.gameObject));
            }

        }
        else
        {
            int level = unit2.GetStat(Statsheet.Stat.Level);
            GiveAllyExperience(ref unit2, unit1);
            if (unit2.GetStat(Statsheet.Stat.Level) != level)
            {
                StartCoroutine(DisplayLevelUp(unit2.gameObject));
            }
        }
    }

    private IEnumerator DisplayLevelUp(GameObject unit, bool playSound = true)
    {
        if (playSound)
        {
            soundController.PlaySfx(GameAssets.Instance.level_up);
        }
        float timeToTake = 0.5f;
        levelUpSymbol.transform.position = unit.transform.position + new Vector3(0.5f, 0, 0);
        levelUpSymbol.transform.localScale = new Vector3(0.5f, 0.5f, 1.0f);
        iTween.ScaleTo(levelUpSymbol, new Vector3(1, 1, 1), timeToTake);
        yield return new WaitForSeconds(timeToTake);
        levelUpSymbol.transform.position = hideObject;
    }

    private void GiveAllyExperience(ref Statsheet ally, Statsheet enemy)
    {
        if (!ally.gameObject.activeSelf)
        {
            return;
        }

        int levelDifference = enemy.GetStat(Statsheet.Stat.Level) - ally.GetStat(Statsheet.Stat.Level);
        int expEarned;
        int minimumLevelDifference = -11;

        levelDifference -= minimumLevelDifference;

        if (levelDifference < 0)
        {
            expEarned = 1;
            ally.IncrementExp(expEarned);
            return;
        }
        else if (levelDifference > 12)
        {
            expEarned = 60;
        }
        else
        {
            if (levelDifference > 3)
            {
                levelDifference -= 3;
                if (levelDifference < 3)
                {
                    levelDifference = 3;
                }
            }
            expEarned = levelDifference * 6;
        }
        
        if (!enemy.gameObject.activeSelf)
        {
            expEarned *= 10;
        }
        ally.IncrementExp(expEarned);
    }
    
    private IEnumerator AnimateUnitAttacking(Statsheet attacker, Statsheet defender, bool isFollowUp)
    {
        float attackMoveTime = 0.2f;
        float returnMoveTime = 0.6f;

        if (attacker != null && defender != null)
        {
            if (attacker.gameObject.activeSelf && defender.gameObject.activeSelf)
            {
                Vector3 atkDirection;
                if (attacker.GetFaction() == Statsheet.Faction.Ally)
                {
                    atkDirection = transform.right;
                }
                else
                {
                    atkDirection = -transform.right;
                }


                iTween.MoveTo(attacker.gameObject, attacker.gameObject.transform.position + atkDirection, attackMoveTime);
                yield return new WaitForSeconds(attackMoveTime);
                
                int damageDealt = DetermineDamage(attacker, defender, isFollowUp);
                if (damageDealt == 0)
                {
                    soundController.PlaySfx(GameAssets.Instance.no_damage);
                }
                else if (attacker.enemyType == Statsheet.EnemyType.Crawfather)
                {
                    soundController.PlaySfx(GameAssets.Instance.craw_snip);
                }
                else if (attacker.GetIsMagic())
                {
                    soundController.PlaySfx(GameAssets.Instance.magical_damage);
                }
                else
                {
                    soundController.PlaySfx(GameAssets.Instance.physical_damage);
                }

                TextMesh damageTextMesh = damageText.GetComponent<TextMesh>();
                if (damageTextMesh != null)
                {
                    damageTextMesh.text = "-" + damageDealt.ToString();
                }

                yield return null;
                damageText.transform.position = defender.transform.position + transform.up;

                if (defender.TakeDamage(damageDealt)) //takeDamage() returns true if the attacker is now dead
                {
                    defender.gameObject.SetActive(false);
                }

                UpdateScore();

                iTween.MoveTo(damageText, damageText.transform.position + new Vector3(0, 0.5f, 0), returnMoveTime - 0.1f);
                iTween.MoveTo(attacker.gameObject, attacker.gameObject.transform.position - atkDirection, returnMoveTime);
                yield return new WaitForSeconds(returnMoveTime);

                //Move damageText offscreen
                damageText.transform.position = hideObject;
            }
        }
    }

    private void UpdateScore()
    {
        if (currentScoreMesh != null)
        {
            int allyScore = TeamTotalHP(allyUnits);
            int enemyScore = TeamTotalHP(enemyUnits);
            currentScoreMesh.text = allyScore + " - " + enemyScore;
        }
        else
        {
            Debug.LogError("Error: Current score mesh does not exist");
        }
    }

    private int DetermineDamage(Statsheet attacker, Statsheet defender, bool isFollowUpAttack)
    {
        int damage;
        int atk = attacker.GetCurStat(Statsheet.Stat.Atk);
        int def;
        Statsheet.Stat defensiveStat;

        //Determine Defensive Stat
        //Attacker targets the defensive stat of the defender corresponding to their higher defensive stat, or defense if it's a tie
        if (attacker.GetIsMagic())
        {
            defensiveStat = Statsheet.Stat.Res;
        }
        else
        {
            defensiveStat = Statsheet.Stat.Def;
        }
        
        def = defender.GetCurStat(defensiveStat);
        damage = atk - def;

        if (damage <= 0)
        {
            return 0;
        }

        if (!isFollowUpAttack)
        {
            return damage;
        }
        else
        {
            int atkSpd = attacker.GetCurStat(Statsheet.Stat.Spd);
            int defSpd = defender.GetCurStat(Statsheet.Stat.Spd);

            if (atkSpd <= defSpd)
            {
                Debug.LogError("ERROR IN determineDamage(): attacker is following up despite having less or equal speed");
                return 0;
            }
            else
            {
                //Attack is a follow-up attack.
                //Follow-up attack damage is determined by multiplying damage by 20% of the difference in speed
                damage = (int)(damage * (atkSpd - defSpd) * 0.2f); 
                if (damage <= 0)
                {
                    //Damage was already determined to be at least 1
                    //A followup attack should never deal 0 damage if the initial strike dealt damage
                    return 1;
                }
                return damage;
            }
        }
    }

    private void SpawnNewUnitsBothSides(int numAlliesToSpawn, int numEnemiesToSpawn)
    {
        SpawnNewUnits(numAlliesToSpawn, Statsheet.Faction.Ally);
        SpawnNewUnits(numEnemiesToSpawn, Statsheet.Faction.Enemy);
    }

    private void SpawnNewUnits(int numUnitsToSpawn, Statsheet.Faction faction)
    {
        int maxUnitsPerSide = numRows * numCols;

        // Error checking
        if (numUnitsToSpawn > maxUnitsPerSide)
        {
            Debug.LogError("ERROR IN SpawnNewUnitsBothSides(): Too many units requested to be built. Max is " + maxUnitsPerSide + ".");
            return;
        }
        
        if (unitPrefab == null)
        {
            Debug.LogError("ERROR IN SpawnNewUnits(): No unit prefab exists");
            return;
        }

        if (emptyUnitPrefab == null)
        {
            Debug.LogError("ERROR IN SpawnNewUnits(): No empty unit prefab exists");
            return;
        }

        if (unitPrefab.GetComponent<Statsheet>() == null)
        {
            Debug.LogError("ERROR IN SpawnNewUnits(): Unit prefab does not have an attached Statsheet script");
        }

        if (emptyUnitPrefab.GetComponent<Statsheet>() == null)
        {
            Debug.LogError("ERROR IN SpawnNewUnits(): Empty unit prefab does not have an attached Statsheet script");
        }

        float offset = 1.5f;

        int numUnits = 0;

        string generateName = "";
        if (faction == Statsheet.Faction.Enemy)
        {
            generateName = nameGenerator.GetEnemyName();
        }

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                //Create a unit
                if (numUnits < numUnitsToSpawn)
                {
                    GameObject unit = Instantiate(unitPrefab);
                    if (faction == Statsheet.Faction.Ally)
                    {
                        unit.transform.position = allySpawnPoint.position + new Vector3(x * -offset, y * -offset, 0);
                    }
                    else
                    {
                        unit.transform.position = enemySpawnPoint.position + new Vector3(x * offset, y * -offset, 0);
                    }

                    Statsheet ss = unit.GetComponent<Statsheet>();
                    if (ss != null)
                    {
                        ss.GenerateRandomStatsheet(1, 280, 20, faction);
                        if (faction == Statsheet.Faction.Ally)
                        {
                            generateName = nameGenerator.GetAllyName();
                        }
                        ss.SetName(generateName);
                    }
                    else
                    {
                        Debug.LogError("Unit Prefab does not have a statsheet attached.");
                    }

                    if (faction == Statsheet.Faction.Ally)
                    {
                        allyUnits[x, y] = unit;
                    }
                    else
                    {
                        enemyUnits[x, y] = unit;
                    }
                    
                    numUnits++;
                }
                else
                {
                    GameObject emptyUnit = Instantiate(emptyUnitPrefab);
                    if (faction == Statsheet.Faction.Ally)
                    {
                        emptyUnit.transform.position = allySpawnPoint.position + new Vector3(x * -offset, y * -offset, 0);
                    }
                    else
                    {
                        emptyUnit.transform.position = enemySpawnPoint.position + new Vector3(x * offset, y * -offset, 0);
                    }
                    
                    Statsheet ss = emptyUnit.GetComponent<Statsheet>();
                    if (ss != null)
                    {
                        if (faction == Statsheet.Faction.Ally)
                        {
                            ss.InitializeEmptyUnit(Statsheet.Faction.Ally);
                        }
                        else
                        {
                            ss.InitializeEmptyUnit(Statsheet.Faction.Enemy);
                        }
                    }
                    else
                    {
                        Debug.LogError("Empty Unit Prefab does not have a statsheet attached.");
                    }

                    if (faction == Statsheet.Faction.Ally)
                    {
                        allyUnits[x, y] = emptyUnit;
                    }
                    else
                    {
                        enemyUnits[x, y] = emptyUnit;
                    }
                }
            }
        }
        if (faction == Statsheet.Faction.Ally)
        {
            Shuffle(allyUnits);
        }
        else
        {
            Shuffle(enemyUnits);
        }
    }

    private void Shuffle(GameObject[,] unitArray)
    {
        for (int x = 0; x < numCols; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                GameObject tempObj = unitArray[x, y];
                int randomX = Random.Range(0, numCols);
                int randomY = Random.Range(0, numRows);
                unitArray[x, y] = unitArray[randomX, randomY];
                unitArray[randomX, randomY] = tempObj;

                if (unitArray[x, y] != unitArray[randomX, randomY])
                {
                    Vector3 tempPos = unitArray[x, y].transform.position;
                    unitArray[x, y].transform.position = unitArray[randomX, randomY].transform.position;
                    unitArray[randomX, randomY].transform.position = tempPos;
                }
            }
        }
    }

    private void Shuffle<T>(ref T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randNum = Random.Range(i, array.Length);
            T temp = array[i];
            array[i] = array[randNum];
            array[randNum] = temp;
        }
    }

    private IEnumerator SwapPlaces(GameObject unit1, GameObject unit2)
    {
        if (unit1 != unit2)
        {
            float moveTime = 0.3f;

            Vector3 temp = unit1.transform.position;
            iTween.MoveTo(unit1, unit2.transform.position, moveTime);
            iTween.MoveTo(unit2, temp, moveTime);

            Statsheet ss = unit1.GetComponent<Statsheet>();
            if (ss != null)
            {
                if (ss.GetFaction() == Statsheet.Faction.Ally)
                {
                    SwapArrayPosition(unit1, unit2, allyUnits);
                }
                else
                {
                    SwapArrayPosition(unit1, unit2, enemyUnits);
                }
            }

            yield return new WaitForSeconds(moveTime);
        }
    }

    private void SwapArrayPosition(GameObject unit1, GameObject unit2, GameObject[,] unitArray)
    {
        bool isDone = false;
        int[] unit1Pos = new int[2];
        int[] unit2Pos = new int[2];
        bool unit1Found = false;
        bool unit2Found = false;
        for (int x = 0; x < numCols && !isDone; x++) {
            for (int y = 0; y < numRows; y++)
            {
                if (unitArray[x, y] == unit1)
                {
                    unit1Pos[0] = x;
                    unit1Pos[1] = y;
                    unit1Found = true;
                }
                else if (unitArray[x, y] == unit2)
                {
                    unit2Pos[0] = x;
                    unit2Pos[1] = y;
                    unit2Found = true;
                }

                if (unit1Found && unit2Found)
                {
                    isDone = true;
                    break;
                }
            }
        }

        if (unit1Found && unit2Found)
        {
            GameObject temp = unitArray[unit1Pos[0], unit1Pos[1]];
            unitArray[unit1Pos[0], unit1Pos[1]] = unitArray[unit2Pos[0], unit2Pos[1]];
            unitArray[unit2Pos[0], unit2Pos[1]] = temp;
        }
        else
        {
            Debug.LogError("ERROR IN SwapArrayPosition() - unit1 or unit2 not found in passed unitArray");
        }
    }

    private IEnumerator MoveUnitsInFromOffScreen()
    {
        startButton.interactable = false;

        Vector3 distanceToMove = new Vector3 (10.0f, 0);
        float minMoveTime = 0.1f;
        float maxMoveTime = 0.7f;
        float timeBetweenMoves = 0.05f;

        //Teleport all units off screen
        for (int x = 0; x < numCols; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                allyUnits[x, y].transform.position -= distanceToMove;
                enemyUnits[x, y].transform.position += distanceToMove;
            }
        }

        //Initialize unit move order
        int numUnits = numRows * numCols;
        int[] allyMoveOrder = new int[numUnits];
        int[] enemyMoveOrder = new int[numUnits];

        for (int i = 0; i < numRows * numCols; i++)
        {
            allyMoveOrder[i] = i;
            enemyMoveOrder[i] = i;
        }

        //Shuffle move order
        Shuffle(ref allyMoveOrder);
        Shuffle(ref enemyMoveOrder);

        //Move units in from off-screen
        for (int i = 0; i < numRows * numCols; i++)
        {
            int x = allyMoveOrder[i] % numCols;
            int y = allyMoveOrder[i] / numCols;

            //Debug.Log(allyMoveOrder[i] + " " + x + " " + y);
            GameObject allyUnit = allyUnits[x, y];
            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            //Debug.Log(moveTime);
            iTween.MoveTo(allyUnit, allyUnit.transform.position + distanceToMove, moveTime);

            x = enemyMoveOrder[i] % numCols;
            y = enemyMoveOrder[i] / numCols;
            GameObject enemyUnit = enemyUnits[x, y];
            moveTime = Random.Range(minMoveTime, maxMoveTime);
            iTween.MoveTo(enemyUnit, enemyUnit.transform.position - distanceToMove, moveTime);

            yield return new WaitForSeconds(timeBetweenMoves);
        }

        yield return new WaitForSeconds(maxMoveTime - timeBetweenMoves);

        startButton.interactable = true;
    }
}