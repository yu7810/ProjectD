using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ValueData : MonoBehaviour
{
    public GameObject Player;
    private static ValueData instance;
    public bool canBehurt;//可被攻擊，用於受傷無敵幀
    public bool isUIopen;//開關UI
    public CinemachineVirtualCamera virtualCamera;//鏡頭
    public GameObject moneyPrefab;

    //基底數值
    public int money;//身上持有的金幣數量
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Power = 5;
    public float base_SkillSpeed = 1;
    public float base_EnemyTimer = 1;
    public float base_AttackSize = 1;
    public float base_Cooldown = 1;
    public float base_CostDown = 1;
    public float base_Crit = 0;
    public float base_CritDmg = 2f;
    public float base_RestoreAP = 1f;//AP每秒自然恢復
    public float base_Damagereduction;//傷害減免
    public float base_Vision;
    public float base_BulletSpeed;

    //天賦數值
    public int passiveskillPoint = 0;//天賦點數
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Power;
    public float add_SkillSpeed;
    public float add_EnemyTimer;
    public float add_AttackSize;
    public float add_Cooldown;
    public float add_CostDown;
    public float add_Crit;
    public float add_CritDmg;
    public float add_RestoreAP;
    public float add_Damagereduction;
    public float add_Vision;
    public float add_BulletSpeed;

    //局外數值(預留)


    //當前總數值
    public bool[] PassiveSkills; //當前各天賦點的加點
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Power;//基礎傷害倍率
    public float SkillSpeed;//技能移動速度
    public float MoveSpeed;//移動速度
    public float EnemyTimer;//敵人生成速度%
    public float AttackSize;//技能大小%
    public float Cooldown;//冷卻倍率%
    public float CostDown;//魔耗倍率%
    public float Crit;//暴率
    public float CritDmg;//暴傷
    public float RestoreAP;
    public float Damagereduction;//傷害減免%
    public float Vision;//視野範圍(FOV)
    public float BulletSpeed;//投射物飛行速度%

    public Sprite[] SkillIcon;//技能icon
    public Sprite[] WeaponIcon;//武器icon

    //技能總表
    [NonSerialized]
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,0,"-",0,0,0,0,0,0),//無
        new SkillBase(1,0,"劈砍",2f,10,1f,1,0.2f,0.25f),
        new SkillBase(2,10,"衝刺",5f,0,1f,1,0,0),//size=位移距離
        new SkillBase(3,0,"音符",1f,10,1f,1,0,0),
        new SkillBase(4,10,"閃現",3.4f,0f,1f,1,1,0f),
        new SkillBase(5,20,"新月斬",2f,10,0.8f,1,1,0.1f),
        new SkillBase(6,0,"弦月斬",2f,20,1.1f,1,2,0.1f),
        new SkillBase(7,0,"明月斬",2f,40,1.4f,1,2,0.1f),
        new SkillBase(8,20,"The喪鐘",20f,0,1f,1,0,0f),
        new SkillBase(9,0,"飛箭",0.3f,3,1f,1,0.6f,0f),
        new SkillBase(10,30,"水曝",1f,0f,1f,1,0f,0f),
    };
    //技能介紹
    [NonSerialized]
    public string[] SkillIntro = new string[] {
        "-",
        "對前方半圓形範圍內所有敵人造成傷害",
        "衝刺一段距離，並恢復50%失去的魔力<BR>(速度會影響衝刺距離)",
        "",
        "閃現至滑鼠位置，沒有距離限制",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "生成一個持續6秒的喪鐘，你對喪鐘造成的傷害會被放大3倍後，被喪鐘以圓形造成範圍傷害",
        "朝滑鼠方向發射一枚飛彈，命中敵人後消失",
        "在滑鼠位置生成一個水球，一段時間後爆炸，消耗一半當前魔力並造成(消耗量×5)傷害",
    };
    //技能標籤
    [NonSerialized]
    public SkillTagType[][] SkillTag = new SkillTagType[][]
    {
        new SkillTagType[]{ } ,
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能1
        new SkillTagType[]{ SkillTagType.Movement } , //技能2
        new SkillTagType[]{ SkillTagType.Spell } , //技能3
        new SkillTagType[]{ SkillTagType.Movement, SkillTagType.Spell } , //技能4
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能5
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能6
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能7
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range } , //技能8
        new SkillTagType[]{ SkillTagType.Projectile, SkillTagType.Physical } , //技能9
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range, SkillTagType.Cold } , //技能10
    };

    //已裝備技能欄位
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//滑鼠L
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//滑鼠R
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//空白鍵
    };

    //裝備總表
    [NonSerialized]
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,RarityType.Normal,0,"空手", 1f, 1f, 1f, 1f, 1f, 0),//Dmg、CD、Size、Speed、Cost皆是倍率，1f=100%
        new WeaponBase(1,RarityType.Normal,15,"鐵劍", 1.5f, 0.85f, 1f, 1f, 1f, 0.1f),
        new WeaponBase(2,RarityType.Normal,15,"鐵弓", 1.2f, 1f, 1f , 1.4f, 0.85f, 0f),
        new WeaponBase(3,RarityType.Normal,15,"鐵斧", 1.2f, 1.5f, 1.5f, 0.8f, 1.2f, 0.2f),
        new WeaponBase(4,RarityType.Magic,15,"聚寶", 1, 1f, 1f, 1f, 1f, 0.05f),
        new WeaponBase(5,RarityType.Rare,30,"破曉", 0.5f, 1f, 1f, 0.8f, 0.8f, 0.25f),
        new WeaponBase(6,RarityType.Rare,30,"逐影", 2f, 1.8f, 0.75f, 1f, 1.6f, 0.15f),
        new WeaponBase(7,RarityType.Rare,30,"賽博義肢", 0.5f, 0.5f, 0.5f, 2f, 0.5f, 0f),
        new WeaponBase(8,RarityType.Rare,0,"碧浪", 1f, 1f, 2f, 1f, 1f, 0),
        new WeaponBase(9,RarityType.Magic,15,"招財", 1, 1f, 1f, 1f, 1f, 0f),
    };
    //裝備介紹
    [NonSerialized]
    public string[] WeaponIntro = new string[] {
        "-",
        "",
        "",
        "",
        "身上每1金幣提供1%傷害增幅",
        "技能暴擊時將冷卻降為0.3s",
        "技能重複2次",
        "使用位移技能時觸發L上的非位移技能",
        "冰冷技能同時命中複數目標時，每個目標使傷害提升20%",
        "擊殺敵人掉落的金幣為0~3倍",
    };

    //已裝備裝備
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//滑鼠L
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//滑鼠R
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//空白鍵
    };

    //技能商店池
    public List<int> skillstorePool = new List<int>()
    {
        2,4,5,8,10
    };
    //裝備商店池
    public List<int> weaponstorePool = new List<int>()
    {
        1,2,3,4,5,6,7,8,9
    };

    //每次加減天賦時呼叫，更新所有數值
    //還需要遍歷天賦點
    public void PlayerValueUpdate() {
        //重製
        add_maxAp = 0;
        add_maxHp = 0;
        add_MoveSpeed = 0;
        add_Power= 0;
        add_SkillSpeed = 0;
        add_EnemyTimer = 0;
        add_AttackSize = 0;
        add_Cooldown = 0;
        add_CostDown = 0;
        add_Crit = 0;
        add_CritDmg = 0;
        add_RestoreAP = 0;
        add_Damagereduction = 0;
        add_Vision = 0;
        add_BulletSpeed = 0;
        //天賦數值
        for (int i = 0; i < PassiveSkills.Length; i++) {
            if (PassiveSkills[i])
                PassiveSkillValueUpdate(i);
        }
        //加總
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Power = base_Power + add_Power;
        MoveSpeed = 1 + add_MoveSpeed;
        SkillSpeed = base_SkillSpeed + add_SkillSpeed;
        EnemyTimer = base_EnemyTimer + add_EnemyTimer;
        AttackSize = base_AttackSize + add_AttackSize;
        Cooldown = 1 - (base_Cooldown * add_Cooldown);
        CostDown = 1 - (base_CostDown * add_CostDown);
        Crit = base_Crit + add_Crit;
        CritDmg = base_CritDmg + add_CritDmg;
        RestoreAP = base_RestoreAP + add_RestoreAP;
        Damagereduction = base_Damagereduction + add_Damagereduction;
        Vision = base_Vision + add_Vision;
        BulletSpeed = base_BulletSpeed + add_BulletSpeed;
        //更新value UI
        UICtrl.Instance.UpdateValueUI();
        virtualCamera.m_Lens.FieldOfView = Vision;
    }

    //更換武器、技能時呼叫，呼叫前請確保有更新過PlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * Cooldown * WeaponField[id].Cooldown;
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * Power * WeaponField[id].Damage;
            SkillField[id].Size = Skill[SkillField[id].ID].Size * AttackSize * WeaponField[id].Size;
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * SkillSpeed * WeaponField[id].Speed;
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * CostDown * WeaponField[id].Costdown;
            SkillField[id].Crit = Skill[SkillField[id].ID].Crit + Crit + WeaponField[id].Crit;
            UICtrl.Instance.SkillfieldUI.transform.GetChild(id).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Skill, SkillField[id].ID, SkillField[id].Name, SkillField[id].maxCD, SkillField[id].Cost, SkillField[id].Damage, SkillField[id].Crit, SkillField[id].Size, SkillField[id].Speed, SkillIntro[SkillField[id].ID]);
        }
    }

    //單例實體
    public static ValueData Instance
    {
        get => instance;
    }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //天賦數值計算
    public void PassiveSkillValueUpdate(int id) { 
        switch (id)
        {
            case 0:
                add_Power += 0.1f;
                break;
            case 1:
                add_Power += 0.1f;
                break;
            case 2:
                add_Power += 0.1f;
                break;
            case 3:
                add_Cooldown += 0.05f;
                break;
            case 4:
                add_Cooldown += 0.05f;
                break;
            case 5:
                add_Cooldown += 0.05f;
                break;
            case 6:
                add_Cooldown += 0.05f;
                break;
            case 7:
                add_maxHp += 5f;
                break;
            case 8:
                add_maxHp += 5f;
                break;
            case 9:
                add_maxHp += 5f;
                break;
            case 10:
                add_maxHp += 5f;
                break;
            case 12:
                add_Crit += 0.1f;
                break;
            case 13:
                add_Crit += 0.1f;
                break;
            case 14:
                add_Crit += 0.1f;
                break;
            case 15:
                add_Crit += 0.1f;
                break;
            case 16:
                add_Crit += 0.15f;
                break;
            case 18:
                add_Vision += 5f;
                break;
            case 19:
                add_Vision += 5f;
                break;
            case 20:
                add_Vision += 5f;
                break;
            case 22:
                add_BulletSpeed += 0.2f;
                break;
            case 23:
                add_BulletSpeed += 0.2f;
                break;
            case 24:
                add_BulletSpeed += 0.2f;
                break;
            case 26:
                add_maxAp += 3f;
                break;
            case 27:
                add_maxAp += 3f;
                break;
            case 28:
                add_maxAp += 3f;
                break;
            case 29:
                add_maxAp += 3f;
                break;
            case 30:
                add_RestoreAP += 0.5f;
                break;
        }
    }

    public void GetMoney(int value) 
    { 
        if(value != 0)
        {
            money += value;
            UICtrl.Instance.UpdateMoneyUI();
        }
        //裝備4的特殊能力
        for(int i=0;i< WeaponField.Length;i++)
        {
            if (WeaponField[i].ID == 4)
            {
                WeaponField[i].Damage = 1 + (money * 0.01f);
                SkillFieldValueUpdate();
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(i).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[i].ID, ValueData.Instance.WeaponField[i].Name, ValueData.Instance.WeaponField[i].Cooldown, ValueData.Instance.WeaponField[i].Costdown, ValueData.Instance.WeaponField[i].Damage, ValueData.Instance.WeaponField[i].Crit, ValueData.Instance.WeaponField[i].Size, ValueData.Instance.WeaponField[i].Speed, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[i].ID]);
            }
        }
    }

    //降冷卻通用
    public void doCooldown(SkillFieldBase field, float reduce) 
    {
        if (field.nowCD == 0)
            return;
        else if (field.nowCD <= reduce)
            field.nowCD = 0;
        else
            field.nowCD -= reduce;
        UICtrl.Instance.UpdateSkillCD();
    }

    //加減生命通用
    public void GetHp(float value, bool useBehurtTimer = false)
    {
        if (value == 0)
            return;
        else if (value > 0) 
        {
            if (HP < maxHP - value)
                HP += value;
            else
                HP = maxHP;
        }
        else
        {
            value *= (1 - Damagereduction);
            if (HP > -value)
            {
                HP += value;
                if (useBehurtTimer == true)
                    StartCoroutine(PlayerCtrl.Instance.BehurtTimer());
            }
            else
            {
                HP = 0;
                UICtrl.Instance.gameover();
            }
        }
    }
    //加減魔力通用
    public void GetAp(float value)
    {
        if (value == 0)
            return;
        else if(value > 0)
        {
            if (maxAP > AP + value)
                AP += value;
            else
                AP = maxAP;
        }
        else
        {
            if (AP > value)
                AP += value;
            else
                AP = 0;
        }
    }

}

//稀有度架構
public enum RarityType
{
    Normal,
    Magic,
    Rare,
    Unique
}

//技能標籤
public enum SkillTagType
{
    Attack,//攻擊
    Spell,//法術
    Movement,//位移
    Range,//範圍
    Projectile,//投射物
    Physical,//物理
    Fire,//火
    Cold,//冰
    Lightning,//電
    Chaos,//混沌

}

//技能架構
public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }    // 名稱
    public float maxCD { get; set; }    // 冷卻時間
    public float Damage { get; set; }   // 傷害
    public float Size { get; set; }     // 技能範圍大小
    public float Speed { get; set; }    // 技能飛行速度
    public float Cost { get; set; }     // 魔耗
    public float Crit { get; set; }     // 暴擊率
    public int Price { get; set; }      // 價格

    public SkillBase(int id, int price,string name, float maxcd, float damage, float size, float speed, float cost, float crit)
    {
        ID = id;
        Name = name;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        Price = price;
    }
}

//技能欄位架構
public class SkillFieldBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float nowCD { get; set; }
    public float maxCD { get; set; }
    public float Damage { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Cost { get; set; }
    public float Crit { get; set; }

    public SkillFieldBase(int id, string name, float nowcd, float maxcd, float damage, float size, float speed, float cost, float crit)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
    }
}

//武器架構
public class WeaponBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public float Cooldown { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Costdown { get; set; }
    public float Crit { get; set; }
    public int Price { get; set; }
    public RarityType Rarity { get; set; }

    public WeaponBase(int id, RarityType rarity, int price, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
        Crit = crit;
        Price = price;
        Rarity = rarity;
    }
}

//武器欄位架構
public class WeaponFieldBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public float Cooldown { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Costdown { get; set; }
    public float Crit { get; set; }

    public WeaponFieldBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
        Crit = crit;
    }
}
