using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UICtrl : MonoBehaviour
{
    public GameObject PlayerCtrl;
    public GameObject Value_AP;
    public GameObject Value_HP;
    public GameObject Value_EXP;
    public GameObject Upgrade_A;
    public GameObject Upgrade_B;
    public GameObject Upgrade_C;
    public float AP;
    public float maxAP;
    public float EXP;
    public float maxEXP;
    public float HP;
    public float maxHP;
    public float value_maxAp = 10;
    public float value_maxHp = 10;
    public float value_MoveSpeed = 5f;
    public float value_Attack = 5;
    public static float Attack;//基礎攻擊力
    public float MoveSpeed;
    public float FlashDistance;
    public float value_FlashDistance;
    public float FlashCost;
    public float value_FlashCost;
    public float EnemyTimer;
    public int[] UpgradeBtn;

    public static float Skill_A_AttackSpeed = 1;//技能A每秒攻擊次數
    public static float Skill_A_Size = 1;//只影響x跟z軸的scale
    public static float Skill_A_DmgAdd = 1;//傷害傷害倍率

    private void Start(){
        ValueUpdate();
        HP = maxHP;
        AP = maxAP;
        EXP = 0;
        UpgradeBtn = new int[3] { 0, 0, 0 };
    }

    void Update()
    {
        UIUpdate();
    }

    void UIUpdate() {
        float valueAP = AP / maxAP;
        Value_AP.GetComponent<Image>().fillAmount = valueAP;
        float valueHP = HP / maxHP;
        Value_HP.GetComponent<Image>().fillAmount = valueHP;
        float valueEXP = EXP / maxEXP;
        Value_EXP.GetComponent<Image>().fillAmount = valueEXP;
    }

    //當角色機體更動時重算數值用
    void ValueUpdate() {
        maxAP = value_maxAp;
        maxHP = value_maxHp;
        FlashDistance = value_FlashDistance;
        FlashCost = value_FlashCost;
        Attack = value_Attack;
    }

    public void GetEXP(float Value) {
        if (Value >= maxEXP - EXP){
            //升級
            EXP = 0;
            maxEXP += maxEXP;
        }
        else {
            EXP += Value;
        }
    }

}
