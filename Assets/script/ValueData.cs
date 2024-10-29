using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    //基底數值
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Attack = 5;

    //局內數值
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Attack;

    //局外數值(預留)


    //當前數值
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Attack;
    public float MoveSpeed;
    public float EnemyTimer;

    //技能icon
    public Sprite[] SkillIcon;

    //技能總表
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,"-",1f,1,1,1,1,0),//無
        new SkillBase(1,"基礎攻擊",0,1.1f,10,1f,1,0),//基礎攻擊
        new SkillBase(2,"基礎閃避",0,3f,0,11f,1,0),//基礎閃避 size=位移距離
        new SkillBase(3,"音符",0,3f,10,1f,1,0),
    };

    //已裝備技能欄位
    public SkillBase[] SkillField = new SkillBase[] {
        new SkillBase(1,"基礎攻擊",0,1f,10,1f,1,0),//滑鼠L
        new SkillBase(0,"-",1f,1,1,1,1,0),//滑鼠R
        new SkillBase(0,"-",1f,1,1,1,1,0),//空白鍵
    };

    //每次加減天賦時呼叫，更新所有數值
    //還需要遍歷天賦點
    public void ValueUpdate() {
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Attack = base_Attack + add_Attack;
        MoveSpeed = base_MoveSpeed + MoveSpeed;

    }

}

public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }      // 技能名稱
    public float nowCD { get; set; }   // 技能當前冷卻 (技能欄位用)
    public float maxCD { get; set; }   // 技能冷卻時間
    public float DamageAdd { get; set; }       // 技能傷害
    public float Size { get; set; }       // 技能範圍大小
    public float Speed { get; set; }       // 技能速度
    public float Cost { get; set; }       // 技能魔耗

    public SkillBase(int id, string name, float nowcd, float maxcd, float damageadd, float size, float speed, float cost)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        DamageAdd = damageadd;
        Size = size;
        Speed = speed;
        Cost = cost;
    }

}