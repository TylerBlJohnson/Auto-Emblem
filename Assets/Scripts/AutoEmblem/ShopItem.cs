using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ShopItem : MonoBehaviour
{
    [SerializeField] private Statsheet.Stat statToBoost;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int price;
    private readonly int minPrice = 1;
    private readonly int maxPrice = 4;

    [SerializeField] private TextMesh itemDescriptionText;
    [SerializeField] private TextMesh itemPriceText;
    [SerializeField] private Sprite[] itemSprites;

    private readonly int[] boostStats =
    {
        1,      //Level
        100,    //Exp
        3,      //HP
        1,      //Atk
        1,      //Spd
        1,      //Def
        1       //Res
    };

    private readonly int[] priceMods =
    {
        2,      //Level
        1,      //Exp
        1,      //HP
        0,      //Atk
        0,      //Spd
        0,      //Def
        0       //Res
    };

    private void Awake()
    {
        Messenger.AddListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
        Messenger.AddListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.COMBAT_STATE_CHANGED, OnCombatStateChanged);
        Messenger.RemoveListener(GameEvent.NEXT_LEVEL, OnNextLevel);
    }

    private void Start()
    {
        RandomlyGenerateItem();
    }

    private void Update()
    {
        if (spriteRenderer.sprite != itemSprites[(int)statToBoost])
        {
            spriteRenderer.sprite = itemSprites[(int)statToBoost];
        }

        if (itemPriceText.text != price.ToString())
        {
            itemPriceText.text = price.ToString();
        }

        itemDescriptionText.text = GetStatName() + " +" + GetBoost();
        
    }

    public void SetStatToBoost(Statsheet.Stat statToBoost)
    {
        this.statToBoost = statToBoost;
    }

    public Statsheet.Stat GetStatToBoost()
    {
        return statToBoost;
    }

    public int GetBoost()
    {
        return boostStats[(int)statToBoost];
    }

    public void SetPrice(int price)
    {
        this.price = price;
    }

    public int GetPrice()
    {
        return price;
    }

    public string GetStatName()
    {
        switch (statToBoost)
        {
            case (Statsheet.Stat.Level):
                return "Lvl";
            case (Statsheet.Stat.Exp):
                return "Exp";
            case (Statsheet.Stat.HP):
                return "HP";
            case (Statsheet.Stat.Atk):
                return "Atk";
            case (Statsheet.Stat.Spd):
                return "Spd";
            case (Statsheet.Stat.Def):
                return "Def";
            case (Statsheet.Stat.Res):
                return "Res";
            default:
                return "ERROR";
        }
    }

    public void RandomlyGenerateItem()
    {
        statToBoost = (Statsheet.Stat)Random.Range(0, itemSprites.Length);
        price = Random.Range(minPrice, maxPrice + 1);
        price += priceMods[(int)statToBoost];
    }

    private void OnMouseDown()
    {
        //Debug.Log("Click");
        Messenger<ShopItem>.Broadcast(GameEvent.SHOP_ITEM_CLICKED, this);
    }

    private void OnCombatStateChanged()
    {
        this.gameObject.SetActive(false);
    }

    private void OnNextLevel()
    {
        this.gameObject.SetActive(true);
        RandomlyGenerateItem();
    }
}
