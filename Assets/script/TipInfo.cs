using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TipInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int Type;
    public string Name;
    public float Cd;
    public float Cost;
    public float Dmg;
    public float Crit;
    public float Size;
    public float Speed;
    public string Intro;

    public void OnPointerEnter(PointerEventData eventData) {
        UICtrl.Instance.Tip_Name.text = Name;
        if (Type == 1) //技能
        {
            UICtrl.Instance.Tip_Cd.text = Cd.ToString() + " s";
            UICtrl.Instance.Tip_Cost.text = Cost.ToString("0.0");
            UICtrl.Instance.Tip_Dmg.text = Dmg.ToString("0");
            UICtrl.Instance.Tip_Size.text = Size.ToString("0.0");
            UICtrl.Instance.Tip_Speed.text = Speed.ToString("0");
        }
        else if (Type == 2) //裝備
        {
            UICtrl.Instance.Tip_Cd.text = (Cd * 100).ToString() + " %";
            UICtrl.Instance.Tip_Cost.text = (Cost * 100).ToString("0") + " %";
            UICtrl.Instance.Tip_Dmg.text = (Dmg * 100).ToString("0") + " %";
            UICtrl.Instance.Tip_Size.text = (Size * 100).ToString("0") + " %";
            UICtrl.Instance.Tip_Speed.text = (Speed * 100).ToString("0") + " %";
        }
        UICtrl.Instance.Tip_Crit.text = (Crit * 100).ToString("0") + " %";
        UICtrl.Instance.Tip_Intro.text = Intro;
    }

    public void OnPointerExit(PointerEventData eventData) { 

    }
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
    public void UpdateInfo(int type,string name,float cd, float cost, float dmg, float crit, float size, float speed,string intro) {
        Type = type;
        Name = name;
        Cd = cd;
        Cost = cost;
        Dmg = dmg;
        Crit = crit;
        Size = size;
        Speed = speed;
        Intro = intro;
    }

}
