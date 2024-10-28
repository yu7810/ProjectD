using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UICtrl : MonoBehaviour
{
    public GameObject PlayerCtrl;
    public Image Value_AP;
    public Image Value_HP;
    public Image Value_EXP;
    public GameObject Upgrade;
    public GameObject Upgrade_A;
    public GameObject Upgrade_B;
    public GameObject Upgrade_C;
    public GameObject UpgradeSys;//UpgradeSystem
    public GameObject GameOverUI;

    public ValueData valuedata;
    public int[] UpgradeBtn;

    //技能欄位icon
    public Image Skill_0;
    public Image Skill_1;
    public Image Skill_2;

    /*
    //Skill A
    public static float Skill_A_AttackSpeed = 0.5f;//每秒攻擊次數
    public static float Skill_A_Size = 1;//範圍
    public static float Skill_A_DmgAdd = 1;//傷害倍率
    //Skill B
    public static float Skill_B_AttackSpeed = 1;//轉圈速度
    public static float Skill_B_Range = 3;//距離
    public static float Skill_B_DmgAdd = 1;//傷害倍率
    */

    private void Start(){
        valuedata.ValueUpdate();
        valuedata.HP = valuedata.maxHP;
        valuedata.AP = valuedata.maxAP;
        valuedata.EXP = 0;
        UpgradeBtn = new int[3] { 0, 0, 0 };
    }

    void Update()
    {
        UIUpdate();
    }

    void UIUpdate() {
        float valueAP = valuedata.AP / valuedata.maxAP;
        Value_AP.fillAmount = valueAP;
        float valueHP = valuedata.HP / valuedata.maxHP;
        Value_HP.fillAmount = valueHP;
        float valueEXP = valuedata.EXP / valuedata.maxEXP;
        Value_EXP.fillAmount = valueEXP;
    }

    public void GetEXP(float Value) {
        if (Value >= valuedata.maxEXP - valuedata.EXP){
            //升級
            valuedata.EXP = 0;
            valuedata.maxEXP += valuedata.maxEXP /2;
            LevelUP();
        }
        else {
            valuedata.EXP += Value;
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
            if (UpgradeBtn[0] == 0)
                return;
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[0]);
        }
        else if (btnID == 2){
            if (UpgradeBtn[1] == 0)
                return;
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[1]);
        }
        else if (btnID == 3){
            if (UpgradeBtn[2] == 0)
                return;
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[2]);
        }
        Upgrade.SetActive(false);
        Time.timeScale = 1;
    }

    public void gameover() {
        Time.timeScale = 0;
        GameOverUI.SetActive(true);
    }

    public void retry_Event() {
        Time.timeScale = 1;
        GameOverUI.SetActive(false);
        SceneManager.LoadScene(0);
    }

    public void ExitGame() {
        Application.Quit();
    }

    public IEnumerator SkillCD(int FieldID) {
        if (valuedata.SkillField[FieldID].nowCD <= 0.01f && valuedata.SkillField[FieldID].nowCD > 0) {
            valuedata.SkillField[FieldID].nowCD = 0;
            yield return new WaitForSeconds(valuedata.SkillField[FieldID].nowCD);
        }
        else {
            valuedata.SkillField[FieldID].nowCD -= 0.01f;
            yield return new WaitForSeconds(0.01f);
            StartCoroutine(SkillCD(FieldID));
        }
        UpdateSkillCD();
    }

    public void UpdateSkillCD() {
        for (int FieldID = 0; FieldID < 3; FieldID++) {
            float now = valuedata.SkillField[FieldID].nowCD;
            float max = valuedata.SkillField[FieldID].maxCD;
            float value = now / max;
            if (FieldID == 0)
                Skill_0.fillAmount = value;
            else if (FieldID == 1)
                Skill_1.fillAmount = value;
            else if (FieldID == 2)
                Skill_2.fillAmount = value;
        }
    }

}
