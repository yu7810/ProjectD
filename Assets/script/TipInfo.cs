using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TipInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TipType Type;
    public int Id;
    public string Name;
    public float Cd;
    public float Cost;
    public float Dmg;
    public float Crit;
    public float Size;
    public float Speed;
    public string Intro;
    public TextMeshProUGUI Price;

    public void OnPointerEnter(PointerEventData eventData) {
        UICtrl.Instance.Tip_Name.text = Name;
        if (Type == TipType.Skill) //技能
        {
            UICtrl.Instance.Tip_Name.color = UICtrl.Instance.RarityColor_Normal;//技能名稱一律白色
            UICtrl.Instance.Tip_Cd.text = Cd.ToString("0.0") + " s";
            UICtrl.Instance.Tip_Cost.text = Cost.ToString("0.0");
            UICtrl.Instance.Tip_Dmg.text = Dmg.ToString("0");
            UICtrl.Instance.Tip_Size.text = Size.ToString("0.0");
            UICtrl.Instance.Tip_Speed.text = Speed.ToString("0.0");
            UICtrl.Instance.Tip_Crit.text = (Crit * 100).ToString("0") + " %";
            UICtrl.Instance.Tip_Intro.text = Intro;
            //tag ui
            if(ValueData.Instance.SkillTag[Id].Length == 0)
            {
                return;
            }
            foreach (Transform child in UICtrl.Instance.Tip_tag.transform)
            {
                child.gameObject.SetActive(false);
            }
            for (int i=0 ; i<ValueData.Instance.SkillTag[Id].Length ; i++)
            {
                if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Attack)
                    UICtrl.Instance.tagAttack.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Chaos)
                    UICtrl.Instance.tagChaos.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Cold)
                    UICtrl.Instance.tagCold.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Fire)
                    UICtrl.Instance.tagFire.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Lightning)
                    UICtrl.Instance.tagLightning.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Movement)
                    UICtrl.Instance.tagMovement.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Physical)
                    UICtrl.Instance.tagPhysical.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Projectile)
                    UICtrl.Instance.tagProjectile.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Spell)
                    UICtrl.Instance.tagSpell.gameObject.SetActive(true);
                else if (ValueData.Instance.SkillTag[Id][i] == SkillTagType.Range)
                    UICtrl.Instance.tagRange.gameObject.SetActive(true);
            }
        }
        else if (Type == TipType.Weapon) //裝備
        {
            UICtrl.Instance.Tip_Cd.text = toText(Cd * 100) + " %";
            UICtrl.Instance.Tip_Cost.text = toText(Cost * 100) + " %";
            UICtrl.Instance.Tip_Dmg.text = toText(Dmg * 100) + " %";
            UICtrl.Instance.Tip_Size.text = toText(Size * 100) + " %";
            UICtrl.Instance.Tip_Speed.text = toText(Speed * 100) + " %";
            UICtrl.Instance.Tip_Crit.text = toText(Crit * 100) + " %";
            UICtrl.Instance.Tip_Intro.text = Intro;
            if (ValueData.Instance.Weapon[Id].Rarity == RarityType.Normal)
                UICtrl.Instance.Tip_Name.color = UICtrl.Instance.RarityColor_Normal;
            else if (ValueData.Instance.Weapon[Id].Rarity == RarityType.Magic)
                UICtrl.Instance.Tip_Name.color = UICtrl.Instance.RarityColor_Magic;
            else if (ValueData.Instance.Weapon[Id].Rarity == RarityType.Rare)
                UICtrl.Instance.Tip_Name.color = UICtrl.Instance.RarityColor_Rare;
            else if (ValueData.Instance.Weapon[Id].Rarity == RarityType.Unique)
                UICtrl.Instance.Tip_Name.color = UICtrl.Instance.RarityColor_Unique;
        }
        else if (Type == TipType.Passiveskill) //天賦
        {
            UICtrl.Instance.Tip_Passiveskillintro.text = Intro;
        }
        
    }

    private string toText(float value = 0, string text = "0") {
        if (value == 0)
            return "-";
        else
            return value.ToString(text);
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
    public void UpdateInfo(TipType type, int id, string name, float cd, float cost, float dmg, float crit, float size, float speed, string intro)
    {
        Type = type;
        Id = id;
        Name = name;
        Cd = cd;
        Cost = cost;
        Dmg = dmg;
        Crit = crit;
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