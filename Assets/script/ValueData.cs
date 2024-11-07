using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    private static ValueData instance;
    public bool canBehurt;//可被攻擊，用於受傷無敵幀

    //基底數值
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Power = 5;

    //天賦數值
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Power;

    //局外數值(預留)


    //當前總數值
    public bool[] PassiveSkills; //當前各天賦點的加點
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Power;//攻擊力
    public float SkillSpeed;//技能移動速度
    public float MoveSpeed;//移動速度
    public float EnemyTimer;//敵人生成速度%
    public float AttackSize;//技能大小%
    public float Cooldown;//冷卻倍率%
    public float CostDown;//魔耗倍率%


    //技能icon
    public Sprite[] SkillIcon;

    //技能總表
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,"-",1f,1,1,1,1,0),//無
        new SkillBase(1,"基礎攻擊",0,1f,10,1.5f,1,1),//基礎攻擊
        new SkillBase(2,"基礎閃避",0,3f,0,11f,1,0),//基礎閃避 size=位移距離
        new SkillBase(3,"音符",0,3f,10,1f,1,0),
    };

    //已裝備技能欄位
    public SkillBase[] SkillField = new SkillBase[] {
        new SkillBase(0,"-",1f,1,1,1,1,0),//滑鼠L
        new SkillBase(0,"-",1f,1,1,1,1,0),//滑鼠R
        new SkillBase(0,"-",1f,1,1,1,1,0),//空白鍵
    };

    //裝備總表
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,"-",0,1f,1f,1f,1f),//Dmg、CD、Size、Speed、Cost皆是倍率，1f=100%
    };

    //已裝備裝備
    public WeaponBase[] WeaponField = new WeaponBase[] {
        new WeaponBase(0,"-",0,1f,1f,1f,1f),//滑鼠L
        new WeaponBase(0,"-",0,1f,1f,1f,1f),//滑鼠R
        new WeaponBase(0,"-",0,1f,1f,1f,1f),//空白鍵
    };

    //每次加減天賦時呼叫，更新所有數值
    //還需要遍歷天賦點
    public void PlayerValueUpdate() {
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Power = base_Power + add_Power;
        MoveSpeed = base_MoveSpeed + add_MoveSpeed;

    }

    //更換武器、技能時呼叫，呼叫前請確保有更新過PlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * Cooldown * Weapon[WeaponField[id].ID].Cooldown;
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * Power * Weapon[WeaponField[id].ID].Damage;
            SkillField[id].Size = Skill[SkillField[id].ID].Size * AttackSize * Weapon[WeaponField[id].ID].Size;
            Debug.Log(SkillField[1].Name + " 大小 " + SkillField[1].Size);
            //Debug.Log(ValueData.Instance.SkillField[1].Name + " 大小 " + ValueData.Instance.SkillField[1].Size);//BUG：SkillField與Skill的數值會自動覆蓋對方
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * SkillSpeed * Weapon[WeaponField[id].ID].Speed;
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * CostDown * Weapon[WeaponField[id].ID].Costdown;
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

}

//技能架構
public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }      // 技能名稱
    public float nowCD { get; set; }   // 技能當前冷卻 (技能欄位用)
    public float maxCD { get; set; }   // 技能冷卻時間
    public float Damage { get; set; }       // 技能傷害
    public float Size { get; set; }       // 技能範圍大小
    public float Speed { get; set; }       // 技能飛行速度
    public float Cost { get; set; }       // 技能魔耗

    public SkillBase(int id, string name, float nowcd, float maxcd, float damage, float size, float speed, float cost)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
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

    public WeaponBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
    }
}