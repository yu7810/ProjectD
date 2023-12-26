using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UICtrl : MonoBehaviour
{
    public GameObject PlayerCtrl;
    public GameObject Value_AP;
    public GameObject Value_HP;
    public GameObject Value_EXP;
    public GameObject Upgrade;
    public GameObject Upgrade_A;
    public GameObject Upgrade_B;
    public GameObject Upgrade_C;
    public GameObject UpgradeSys;//UpgradeSystem
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
    public static float Attack = 1;//基底攻擊力
    public float MoveSpeed;
    public float FlashDistance;
    public float value_FlashDistance;
    public float FlashCost;
    public float value_FlashCost;
    public float EnemyTimer;
    public int[] UpgradeBtn;
    //Skill A
    public static float Skill_A_AttackSpeed = 0.6f;//每秒攻擊次數
    public static float Skill_A_Size = 1;//範圍
    public static float Skill_A_DmgAdd = 1;//傷害倍率
    //Skill B
    public static float Skill_B_AttackSpeed = 1;//轉圈速度
    public static float Skill_B_Range = 3;//距離
    public static float Skill_B_DmgAdd = 1;//傷害倍率

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

    //����������ʮɭ���ƭȥ�
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
            LevelUP();
        }
        else {
            EXP += Value;
        }
    }

    void LevelUP() {
        UpgradeBtn = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeBtn();
        if (UpgradeBtn.Length <= 0)//池子為空時直接跳過3選1階段
        {
            Debug.Log("池子為空");
            return;
        }
        Upgrade.SetActive(true);
        Upgrade_A.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[0]].Name.ToString();
        Upgrade_A.transform.Find("Lv").GetComponent<TextMeshProUGUI>().text = "(" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[0]].Lv.ToString() + "/" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[0]].maxLv.ToString() + ")";
        Upgrade_A.transform.Find("Context").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[0]].Context.ToString();
        Upgrade_B.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[1]].Name.ToString();
        Upgrade_B.transform.Find("Lv").GetComponent<TextMeshProUGUI>().text = "(" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[1]].Lv.ToString() + "/" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[1]].maxLv.ToString() + ")";
        Upgrade_B.transform.Find("Context").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[1]].Context.ToString();
        Upgrade_C.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[2]].Name.ToString();
        Upgrade_C.transform.Find("Lv").GetComponent<TextMeshProUGUI>().text = "(" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[2]].Lv.ToString() + "/" + UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[2]].maxLv.ToString() + ")";
        Upgrade_C.transform.Find("Context").GetComponent<TextMeshProUGUI>().text = UpgradeSys.GetComponent<UpgradeSystem>().UpgradeList[UpgradeBtn[2]].Context.ToString();
        Time.timeScale = 0;
    }

    //按鈕事件
    public void _UpgradeBtn(int btnID) {
        if (btnID == 1) { 
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[0]);
        }
        else if (btnID == 2){
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[1]);
        }
        else if (btnID == 3){
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[2]);
        }
        Upgrade.SetActive(false);
        Time.timeScale = 1;
    }
}
