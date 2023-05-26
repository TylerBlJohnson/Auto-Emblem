using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Statsheet : MonoBehaviour
{
    public enum Stat
    {
        Level,
        Exp,
        HP,
        Atk,
        Spd,
        Def,
        Res
    }
    
    public enum Faction
    {
        Ally,
        Enemy
    }

    public enum EnemyType
    {
        Normal,
        Crawfather
    }

    [SerializeField] private bool isEmptyUnit = false;
    [SerializeField] private Image healthBar;
    [SerializeField] private Color[] healthColorArray;
    [SerializeField] private Animator anim;

    [SerializeField] private Faction faction = Faction.Ally;
    [SerializeField] private string characterName = "Character";
    [SerializeField] private int baseHP = 20;
    [SerializeField] private int baseAtk = 20;
    [SerializeField] private int baseSpd = 15;
    [SerializeField] private int baseDef = 10;
    [SerializeField] private int baseRes = 10;
    [SerializeField] private int HPGrowth = 70;
    [SerializeField] private int atkGrowth = 50;
    [SerializeField] private int spdGrowth = 50;
    [SerializeField] private int defGrowth = 50;
    [SerializeField] private int resGrowth = 40;
    [SerializeField] private int setLevel = 1;
    public EnemyType enemyType = EnemyType.Normal;
    
    private int level = 1;
    private int defaultHP = 20;
    private int defaultAtk = 20;
    private int defaultSpd = 15;
    private int defaultDef = 10;
    private int defaultRes = 10;
    private int defaultHPGrowth = 70;
    private int defaultAtkGrowth = 50;
    private int defaultSpdGrowth = 50;
    private int defaultDefGrowth = 50;
    private int defaultResGrowth = 40;
    private int exp = 0;

    private int atkMod = 0;
    private int spdMod = 0;
    private int defMod = 0;
    private int resMod = 0;

    private int damage = 0;

    private bool isMagic = false;

    private void OnMouseDown()
    {
        Messenger<Statsheet>.Broadcast(GameEvent.STATSHEET_CLICKED, this);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (baseHP == 0) //Prefab was instantiated and thus doesn't have default values???
        {
            baseHP = defaultHP;
            baseAtk = defaultAtk;
            baseSpd = defaultSpd;
            baseDef = defaultDef;
            baseRes = defaultRes;
            HPGrowth = defaultHPGrowth;
            atkGrowth = defaultAtkGrowth;
            spdGrowth = defaultSpdGrowth;
            defGrowth = defaultDefGrowth;
            resGrowth = defaultResGrowth;
        }
        else
        {
            defaultHP = baseHP;
            defaultAtk = baseAtk;
            defaultSpd = baseSpd;
            defaultDef = baseDef;
            defaultRes = baseRes;
            defaultHPGrowth = HPGrowth;
            defaultAtkGrowth = atkGrowth;
            defaultSpdGrowth = spdGrowth;
            defaultDefGrowth = defGrowth;
            defaultResGrowth = resGrowth;
        }

        SetFaction(faction);

        for (int i = 1; i < setLevel; i++)
        {
            LevelUp(true);
        }

        UpdateHealthBar();
        UpdateIsMagic();
    }

    private void Update()
    {
        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (faction == Faction.Ally)
            {
                sr.flipX = false;
            }
            else
            {
                sr.flipX = true;
            }
        }
    }

    private void Reset()
    {
        baseHP = defaultHP;
        baseAtk = defaultAtk;
        baseSpd = defaultSpd;
        baseDef = defaultDef;
        baseRes = defaultRes;
        HPGrowth = defaultHPGrowth;
        atkGrowth = defaultAtkGrowth;
        spdGrowth = defaultSpdGrowth;
        defGrowth = defaultDefGrowth;
        resGrowth = defaultResGrowth;

        level = 1;
        exp = 0;

        atkMod = 0;
        spdMod = 0;
        defMod = 0;
        resMod = 0;

        damage = 0;

        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        SetFaction(faction);

        UpdateIsMagic();
    }

    public Faction GetFaction()
    {
        return faction;
    }

    public void SetFaction(Faction faction)
    {
        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (faction == Faction.Ally)
            {
                sr.flipX = false;
            }
            else if (faction == Faction.Enemy)
            {
                sr.flipX = true;
            }
        }
        this.faction = faction;
    }

    public bool GetIsEmptyUnit()
    {
        return isEmptyUnit;
    }

    public string GetName()
    {
        return characterName;
    }

    public void SetName(string characterName)
    {
        this.characterName = characterName;
    }

    public int GetStat(Stat stat)
    {
        switch (stat)
        {
            case (Stat.Level):
                return level;
            case (Stat.Exp):
                return exp;
            case (Stat.HP):
                return baseHP;
            case (Stat.Atk):
                return baseAtk;
            case (Stat.Spd):
                return baseSpd;
            case (Stat.Def):
                return baseDef;
            case (Stat.Res):
                return baseRes;
            default:
                Debug.LogError("Illegal stat request in Statsheet.getStat()");
                return -1;
        }
    }

    public int GetCurStat(Stat stat)
    {
        switch (stat)
        {
            case (Stat.Level):
                Debug.LogError("There is no mod variable for level, use getStat(level) instead.");
                return level;
            case (Stat.Exp):
                Debug.LogError("There is no mod variable for exp, use getStat(exp) instead.");
                return exp;
            case (Stat.HP):
                return baseHP - damage;
            case (Stat.Atk):
                return baseAtk + atkMod;
            case (Stat.Spd):
                return baseSpd + spdMod;
            case (Stat.Def):
                return baseDef + defMod;
            case (Stat.Res):
                return baseRes + resMod;
            default:
                Debug.LogError("Illegal stat request in Statsheet.getCurStat()");
                return -1;
        }
    }

    public string GetStatName(Stat stat)
    {
        switch (stat)
        {
            case (Stat.Level):
                return "Lvl";
            case (Stat.Exp):
                return "Exp";
            case (Stat.HP):
                return "HP";
            case (Stat.Atk):
                return "Atk";
            case (Stat.Spd):
                return "Spd";
            case (Stat.Def):
                return "Def";
            case (Stat.Res):
                return "Res";
            default:
                return "ERROR";
        }
    }

    public void SetAllStats (int HP, int atk, int spd, int def, int res)
    {
        SetStat(Stat.HP, HP);
        SetStat(Stat.Atk, atk);
        SetStat(Stat.Spd, spd);
        SetStat(Stat.Def, def);
        SetStat(Stat.Res, res);
    }

    public void SetStat(Stat stat, int newStat)
    {
        switch (stat)
        {
            case (Stat.Level):
                level = newStat;
                break;
            case (Stat.Exp):
                exp = newStat;
                break;
            case (Stat.HP):
                baseHP = newStat;
                break;
            case (Stat.Atk):
                baseAtk = newStat;
                break;
            case (Stat.Spd):
                baseSpd = newStat;
                break;
            case (Stat.Def):
                baseDef = newStat;
                break;
            case (Stat.Res):
                baseRes = newStat;
                break;
            default:
                Debug.LogError("Illegal stat request in Statsheet.setStat(), cannot use stat: " + GetStatName(stat));
                break;
        }
        UpdateIsMagic();
    }

    public void SetStatMod(Stat stat, int newMod)
    {
        switch (stat)
        {
            case (Stat.Atk):
                atkMod = newMod;
                break;
            case (Stat.Spd):
                spdMod = newMod;
                break;
            case (Stat.Def):
                defMod = newMod;
                break;
            case (Stat.Res):
                resMod = newMod;
                break;
            default:
                Debug.LogError("Illegal stat request in Statsheet.getStat()");
                break;
        }
    }

    public void SetAllStatGrowths(int HP, int atk, int spd, int def, int res)
    {
        SetStatGrowth(Stat.HP, HP);
        SetStatGrowth(Stat.Atk, atk);
        SetStatGrowth(Stat.Spd, spd);
        SetStatGrowth(Stat.Def, def);
        SetStatGrowth(Stat.Res, res);
    }

    public void SetStatGrowth(Stat stat, int newStat)
    {
        switch (stat)
        {
            case (Stat.HP):
                HPGrowth = newStat;
                break;
            case (Stat.Atk):
                atkGrowth = newStat;
                break;
            case (Stat.Spd):
                spdGrowth = newStat;
                break;
            case (Stat.Def):
                defGrowth = newStat;
                break;
            case (Stat.Res):
                resGrowth = newStat;
                break;
            default:
                Debug.LogError("Illegal stat request in Statsheet.setStatGrowth()");
                break;
        }
    }

    public int GetStatGrowth(Stat stat)
    {
        switch (stat)
        {
            case (Stat.HP):
                return HPGrowth;
            case (Stat.Atk):
                return atkGrowth;
            case (Stat.Spd):
                return spdGrowth;
            case (Stat.Def):
                return defGrowth;
            case (Stat.Res):
                return resGrowth;
            default:
                Debug.LogError("Illegal stat request in Statsheet.setStatGrowth()");
                return -1;
        }
    }

    public int GetTotalGrowth()
    {
        return HPGrowth + atkGrowth + spdGrowth + defGrowth + resGrowth;
    }

    //Returns true if unit dies
    public bool TakeDamage(int damageDealt)
    {
        if (damageDealt < 0)
        {
            Debug.LogError("ERROR IN Statsheet.takeDamage(): damageDealt was less than 0, to heal damage use healDamage() instead");
            return false;
        }
        else
        {
            damage += damageDealt;

            // if current HP <= 0 then the unit is dead
            if (GetCurStat(Stat.HP) <= 0)
            {
                return true;
            }
            else
            {
                UpdateHealthBar();
                return false;
            }
        }
    }

    public void HealDamage(int damageHealed)
    {
        if (damageHealed < 0)
        {
            Debug.LogError("ERROR IN Statsheet.healDamage(): damageHealed was less than 0, to deal damage use takeDamage() instead");
        }
        else
        {
            damage -= damageHealed;
            if (damage < 0)
            {
                damage = 0;
            }
        }
        UpdateHealthBar();
    }

    public void IncrementStat(Stat stat, int boost)
    {
        switch (stat)
        {
            case (Stat.Level):
                LevelUp(true);
                break;
            case (Stat.Exp):
                IncrementExp(boost);
                break;
            case (Stat.HP):
                baseHP += boost;
                break;
            case (Stat.Atk):
                baseAtk += boost;
                break;
            case (Stat.Spd):
                baseSpd += boost;
                break;
            case (Stat.Def):
                baseDef += boost;
                break;
            case (Stat.Res):
                baseRes += boost;
                break;
            default:
                Debug.LogError("Non-existant stat request in Statsheet.incrementStat()");
                break;
        }

        UpdateIsMagic();
    }

    public void IncrementExp(int boost)
    {
        int expRequiredToLevelUp = DetermineEXPRequiredToLevelUp();
        if (boost > expRequiredToLevelUp)
        {
            boost = expRequiredToLevelUp;
        }

        exp += boost;
        LevelUp();
    }

    //if forceLevelUp is false, performs an exp check before levelling up
    public void LevelUp(bool forceLevelUp = false)
    {
        int expRequired = DetermineEXPRequiredToLevelUp();
	    if (exp < expRequired && !forceLevelUp) { //If unit has less than enough exp to level up
            return;
        }

        else
        {
            if (!forceLevelUp)
            {
                exp -= expRequired;
            }

            if (exp < 0)
            {
                Debug.LogError("ERROR IN Statsheet.levelUp(), exp has gone below 0.");
            }

            LevelUpStat(ref baseHP, HPGrowth);
            LevelUpStat(ref baseAtk, atkGrowth);
            LevelUpStat(ref baseSpd, spdGrowth);
            LevelUpStat(ref baseDef, defGrowth);
            LevelUpStat(ref baseRes, resGrowth);
            level++;
        }
    }

    private void LevelUpStat(ref int baseStat, int statGrowth)
    {
        //If a units growth rate is larger than 100% they get a free stat point as well
        //IE, an HP growth of 110% is one guaranteed point of HP with a 10% chance of a second point
        int statIncrement = 0;
        while (statGrowth > 100)
        {
            statIncrement++;
            statGrowth -= 100;
        }

        if (statGrowth > Random.Range(1, 100))
        {
            statIncrement++;
        }

        baseStat += statIncrement;
    }

    public int DetermineEXPRequiredToLevelUp()
    {
        //EXP formula ripped from Fire Emblem Heroes
        //Each level takes 10% more exp than the previous level
        //Level 1 to Level 2 takes 100
        //to 3 takes 110
        //to 4 takes 121
        //etc
        return (int)(System.Math.Pow(1.1, level - 1) * 100);
    }

    //Pass in base level for unit to determine what level the generated unit will be
    //totalGrowthPercentage will be split randomly throughout the 5 base stats (it won't be always be exact but it'll be pretty close)
    //faction is which side the generated unit will be on
    public void GenerateRandomStatsheet(int level, int totalGrowthPercentage, int growthMaxDeviation, Statsheet.Faction faction)
    {
        int baseHPMaxDeviation = 5;
        int baseAtkMaxDeviation = 3;
        int baseSpdMaxDeviation = 4;
        int baseDefMaxDeviation = 4;
        int baseResMaxDeviation = 3;

        //Initialize Statsheet
        Reset();
        SetFaction(faction);

        //if (faction == Statsheet.Faction.Ally)
        //{
        //    //SetName("Ally");
        //}
        //else if (faction == Statsheet.Faction.Enemy)
        //{
        //    //SetName("Foe");
        //}
        //else
        //{
        //    Debug.LogError("Unsupported faction passed to generateRandomStatsheet");
        //}

        if (level <= 0)
        {
            Debug.LogError("generateRandomStatsheet() was called but was passed an illegal level value.");
            return;
        }

        //Determine base stats
        SetAllStats(
            Random.Range(baseHP - baseHPMaxDeviation, baseHP + baseHPMaxDeviation + 1),
            Random.Range(baseAtk - baseAtkMaxDeviation, baseAtk + baseAtkMaxDeviation + 1),
            Random.Range(baseSpd - baseSpdMaxDeviation, baseSpd + baseSpdMaxDeviation + 1),
            Random.Range(baseDef - baseDefMaxDeviation, baseDef + baseDefMaxDeviation + 1),
            Random.Range(baseRes - baseResMaxDeviation, baseRes + baseResMaxDeviation + 1)
        );


        //Determine base growths
        for (int i = (int)Stat.HP; i <= (int)Stat.Res; i++)
        {
            totalGrowthPercentage -= GetStatGrowth((Stat)i);
        }

        for (int i = (int)Stat.HP; i <= (int)Stat.Res; i++)
        {
            SetStatGrowth((Stat)i
            , GetStatGrowth((Stat)i) + Random.Range(-growthMaxDeviation, growthMaxDeviation + 1) + (totalGrowthPercentage / 5));
        }

        //Level the character up to match the passed level.
        for (int i = 1; i < level; i++)
        {
            LevelUp(true);
        }

        UpdateIsMagic();
    }

    public void InitializeEmptyUnit(Faction faction)
    {
        isEmptyUnit = true;
        this.faction = faction;
        SetName("");
    }

    private void UpdateHealthBar()
    {
        float healthPercent = (float)GetCurStat(Stat.HP) / GetStat(Stat.HP);

        healthBar.fillAmount = healthPercent;

        //Set new healthbar color
        if (healthColorArray.Length == 1)
        {
            healthBar.color = healthColorArray[0];
        }
        else if (healthColorArray.Length != 0)
        {
            Color lastColor = healthColorArray[0];
            Color nextColor = healthColorArray[1];
            float lastColorPercent = 0.0f;
            float nextColorPercent = 1.0f / (healthColorArray.Length - 1);
            float betweener = healthPercent;

            if (betweener == lastColorPercent)
            {
                healthBar.color = lastColor;
            }
            else if (betweener == nextColorPercent)
            {
                healthBar.color = nextColor;
            }
            else //find how far between whichever two colors the healthBar is
            {
                for (int i = 1; i < healthColorArray.Length - 1; i++)
                {
                    if (betweener > lastColorPercent
                        && betweener < nextColorPercent)
                    {
                        betweener = (betweener - lastColorPercent) / (nextColorPercent - lastColorPercent);
                        //Debug.Log("Found color at array position: " + i);
                        break;
                    }
                    else if (betweener == nextColorPercent) //Avoids division by 0 error
                    {
                        betweener = 1.0f;
                        break;
                    }
                    else
                    {
                        lastColor = healthColorArray[i];
                        nextColor = healthColorArray[i + 1];
                        lastColorPercent = (float)i / (healthColorArray.Length - 1);
                        nextColorPercent = (float)(i + 1) / (healthColorArray.Length - 1);
                    }
                }
                healthBar.color = Color.Lerp(lastColor, nextColor, betweener);
            }
        }
        else
        {
            healthBar.color = Color.blue;
        }
        //TODO: tween the healthBar so it doesn't just change instantaneously
    }

    public bool GetIsMagic()
    {
        return isMagic;
    }

    public void UpdateIsMagic()
    {
        isMagic = GetStat(Stat.Res) > GetStat(Stat.Def);
        if (anim != null)
        {
            anim.SetBool("isMagic", isMagic);
        }
    }
}