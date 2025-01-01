using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TipInfo : MonoBehaviour
{
    public TipType Type;
    public int Id;
    public string Name;
    public float Cd;
    public float Cost;
    public float Dmg;
    public float Crit;
    public float CritDmg;
    public float Size;
    public float Speed;
    public string Intro;
    public TextMeshProUGUI Price;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">1=技能，2=裝備</param>
    /// <param name="name"></param>
    /// <param name="cd"></param>
    /// <param name="cost"></param>
    /// <param name="dmg"></param>
    /// <param name="crit"></param>
    /// <param name="size"></param>
    /// <param name="speed"></param>
    /// <param name="intro"></param>
    public void UpdateInfo(TipType type, int id, string name, float cd, float cost, float dmg, float crit, float critdmg, float size, float speed, string intro)
    {
        Type = type;
        Id = id;
        Name = name;
        Cd = cd;
        Cost = cost;
        Dmg = dmg;
        Crit = crit;
        CritDmg = critdmg;
        Size = size;
        Speed = speed;
        Intro = intro;
    }

    public void UpdatePrice(int price) 
    {
        Price.text = price.ToString();
    }

}

public enum TipType{
    Skill,
    SkillField,
    Weapon,
    WeaponField,
    Passiveskill
}