using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    public GameObject Player;
    private static ValueData instance;
    public bool canBehurt;//可被攻擊，用於受傷無敵幀
    public bool isUIopen;//開關UI

    //基底數值
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
    public int money;//身上持有的金幣數量

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

    public Sprite[] SkillIcon;//技能icon
    public Sprite[] WeaponIcon;//武器icon

    //技能總表
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,0,"-",0,0,0,0,0,0),//無
        new SkillBase(1,0,"基礎攻擊",1f,10,1f,1,1,0f),//基礎攻擊
        new SkillBase(2,1,"基礎閃避",3f,0,11f,1,0,0),//基礎閃避 size=位移距離
        new SkillBase(3,10,"音符",3f,10,1f,1,0,0),
    };

    //技能介紹
    public string[] SkillIntro = new string[] {
        "-",
        "技能1說明文",
        "技能2說明文",
        "技能3說明文",
    };

    //已裝備技能欄位
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//滑鼠L
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//滑鼠R
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//空白鍵
    };

    //裝備總表
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,0,"空手", 1f, 1f, 1f, 1f, 1f, 0),//Dmg、CD、Size、Speed、Cost皆是倍率，1f=100%
        new WeaponBase(1,10,"劍", 1.6f, 0.7f, 1f, 1f, 1f, 0.1f),
        new WeaponBase(2,10,"弓", 1.5f, 1f, 1f , 1.5f, 0.6f, 0.25f),
        new WeaponBase(3,10,"斧", 2.4f, 1.3f, 1.5f, 1f, 1f, 0.1f),
        new WeaponBase(4,10,"黃金槌", 1, 1f, 1f, 1f, 1f, 0f),
    };
    //裝備介紹
    public string[] WeaponIntro = new string[] {
        "-",
        "裝備1說明文",
        "裝備2說明文",
        "裝備3說明文",
        "身上每1金幣提供1%傷害增幅",
    };

    //已裝備裝備
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//滑鼠L
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//滑鼠R
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//空白鍵
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
        //更新value UI
        UICtrl.Instance.UpdateValueUI();
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
            UICtrl.Instance.SkillfieldUI.transform.GetChild(id).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(1, SkillField[id].Name, SkillField[id].maxCD, SkillField[id].Cost, SkillField[id].Damage, SkillField[id].Crit, SkillField[id].Size, SkillField[id].Speed, SkillIntro[SkillField[id].ID]);
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
        }
    }

    public void GetMoney(int value) 
    { 
        if(value > 0)
        {
            money += value;
        }
        //裝備4的特殊能力
        for(int i=0;i< WeaponField.Length;i++)
        {
            if (WeaponField[i].ID == 4)
            {
                WeaponField[i].Damage = 1 + (money * 0.01f);
                SkillFieldValueUpdate();
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(i).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(2, ValueData.Instance.WeaponField[i].Name, ValueData.Instance.WeaponField[i].Cooldown, ValueData.Instance.WeaponField[i].Costdown, ValueData.Instance.WeaponField[i].Damage, ValueData.Instance.WeaponField[i].Crit, ValueData.Instance.WeaponField[i].Size, ValueData.Instance.WeaponField[i].Speed, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[i].ID]);
            }
                
        }
    }

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

    public WeaponBase(int id, int price, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
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
