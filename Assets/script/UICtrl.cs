﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class UICtrl : MonoBehaviour
{
    private static UICtrl instance;
    public Canvas canvas;
    public Canvas worldspaceCanvas;
    public Image Value_AP;
    public Image Value_HP;
    public Image Value_EXP;
    public GameObject Upgrade;
    public GameObject Upgrade_A;
    public GameObject Upgrade_B;
    public GameObject Upgrade_C;
    public GameObject UpgradeSys;//UpgradeSystem
    public GameObject GameOverUI;
    public GameObject SkillStoreUI;
    public GameObject WeaponStoreUI;
    public GameObject PassiveskilltreeUI;
    public Transform SkillStoreContent;//商店內容頁父物件
    public Transform WeaponStoreContent;
    public GameObject SkillFieldSelectUI;//選擇技能要放的欄位
    public GameObject WeaponFieldSelectUI;//選擇技能要放的欄位
    public GameObject StoreskillbuttonPrefab;
    public GameObject StoreweaponbuttonPrefab;
    public GameObject SkillfieldUI;
    public GameObject WeaponfieldUI;
    public TextMeshPro DamagetextPrefab;//傷害數字
    public GameObject DamagetextParent;//傷害數字的父物件
    public GameObject[] DontDestroy;
    public TextMeshProUGUI MoneyValue;
    public TextMeshProUGUI nowhpUI;
    public TextMeshProUGUI maxhpUI;
    public TextMeshProUGUI nowapUI;
    public TextMeshProUGUI maxapUI;
    public Image ScreenMask;

    public GameObject Tip; // 說明窗相關
    public GameObject Tip_line; // 美化UI用
    public GameObject Tip_affix; // 數值欄位的父物件
    public TextMeshProUGUI Tip_Name;
    public TextMeshProUGUI Tip_Intro;
    public TextMeshProUGUI Tip_Cd;
    public TextMeshProUGUI Tip_Cost;
    public TextMeshProUGUI Tip_Dmg;
    public TextMeshProUGUI Tip_Crit;
    public TextMeshProUGUI Tip_CritDmg;
    public TextMeshProUGUI Tip_Size;
    public TextMeshProUGUI Tip_Speed;
    public GameObject Tip_Level;

    public GameObject ValueUI;//數值欄相關
    public TextMeshProUGUI HP_text;
    public TextMeshProUGUI AP_text;
    public TextMeshProUGUI Power_text;
    public TextMeshProUGUI Movespeed_text;
    public TextMeshProUGUI SkillSpeed_text;
    public TextMeshProUGUI EnemyTimer_text;
    public TextMeshProUGUI AttackSize_text;
    public TextMeshProUGUI Cooldown_text;
    public TextMeshProUGUI Costdown_text;
    public TextMeshProUGUI Crit_text;
    public TextMeshProUGUI CritDmg_text;
    public TextMeshProUGUI Damagereduction_text;
    public TextMeshProUGUI Bulletspeed_text;
    public TextMeshProUGUI RestoreAP_text;

    public GameObject Tip_tag;//Tip上的Tag欄相關
    public TextMeshProUGUI tagAttack;
    public TextMeshProUGUI tagChaos;
    public TextMeshProUGUI tagCold;
    public TextMeshProUGUI tagFire;
    public TextMeshProUGUI tagLightning;
    public TextMeshProUGUI tagMovement;
    public TextMeshProUGUI tagPhysical;
    public TextMeshProUGUI tagProjectile;
    public TextMeshProUGUI tagSpell;
    public TextMeshProUGUI tagRange;

    public int[] UpgradeBtn;
    [NonSerialized]
    public int[] ChangeSkill_ID = new int[] {0,0};//更換技能的ID暫存 (技能ID, 技能等級)
    public int ChangeWeapon_ID;//更換裝備的ID暫存
    public bool isSpendmoney;//更換技能、裝備時是否消耗金幣
    public Image[] SkillCdMask = new Image[3];//技能CD遮罩
    public Image[] SkillFieldIcon = new Image[3];//已裝備技能icon
    public Image[] WeaponFieldIcon = new Image[3];//已裝備技能icon
    public GameObject _passiveskill;
    public PassiveSkill[] passiveskill;
    public Color BtnColor_Have;//有天賦時的天賦點顏色
    public Color BtnColor_Normal;//無天賦時的
    public Color DamagetextColor_Normal;//傷害數字顏色
    public Color DamagetextColor_Crit;//暴擊時
    public Color RarityColor_Normal;//物品不同稀有度的顏色，用在名字文字上
    public Color RarityColor_Magic;
    public Color RarityColor_Rare;
    public Color RarityColor_Unique;
    public Color valueColor_add;//數值為正時的顏色
    public Color valueColor_reduce;//數值為負時的顏色
    public Color valueColor_normal;//技能數值的顏色
    public Color vignetteColor; // Global Volume裡vignette的初始顏色
    public Color vignetteBehurtColor;
    public Volume volume; // Global Volume
    public Vignette vignette;
    public Line line;
    public Transform LineFather;
    public TextMeshProUGUI passiveskillPoint;//天賦點數
    public Npc nowSkillstore; // 儲存現在開的商店，用於重骰等功能
    public Npc nowWeaponstore;
    public GameObject settingUI;

    private EventSystem eventSystem;//滑鼠射線用
    private GraphicRaycaster graphicRaycaster;

    private void Start() {
        ValueData.Instance.PlayerValueUpdate();
        ValueData.Instance.HP = ValueData.Instance.maxHP;
        ValueData.Instance.AP = ValueData.Instance.maxAP;
        ValueData.Instance.EXP = 0;
        UpgradeBtn = new int[3] { 0, 0, 0 };
        passiveskill = _passiveskill.GetComponentsInChildren<PassiveSkill>();
        eventSystem = EventSystem.current;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        ValueData.Instance.isUIopen = false;
        SkillStoreUI.SetActive(false);
        WeaponStoreUI.SetActive(false);
        UpdatePassiveSkill();
        PassiveskilltreeUI.SetActive(false);
        ValueUI.SetActive(false);
        WeaponfieldUI.SetActive(false);
        UpdateMoneyUI();
        if (volume.profile.TryGet(out vignette))
            vignetteColor = vignette.color.value;
            
    }

    private void FixedUpdate()
    {
        UIUpdate();//暴力解(暫)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { // 天賦介面 Tab
            if (ValueData.Instance.isUIopen)
            {
                PassiveskilltreeUI.SetActive(false);
                ValueUI.SetActive(false);
                ChangeSkill_ID = new int[] { -1, 0 };
                ChangeWeapon_ID = 0;
                ValueData.Instance.isUIopen = false;
                if(!SkillStoreUI.activeSelf && !WeaponStoreUI.activeSelf)
                    WeaponfieldUI.SetActive(false);
                Time.timeScale = 1;
            }
            else {
                ValueUI.SetActive(true);
                PassiveskilltreeUI.SetActive(true);
                UpdatePassiveSkill();
                UpdateValueUI();
                ValueData.Instance.isUIopen = true;
                WeaponfieldUI.SetActive(true);
                Time.timeScale = 0;
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape)) // 暫停選單 Esc
        {
            if (!settingUI.activeSelf)
            {
                PlayerCtrl.Instance.openedTarget = null;
                settingUI.SetActive(true);
                showWeaponstore(false);
                showSkillstore(false);
                Time.timeScale = 0;
            }
            else
            {
                settingUI.SetActive(false);
                if (!ValueData.Instance.isUIopen)
                    Time.timeScale = 1;
            }
        }

        //Tip彈窗
        if (IsPointerOverUI(out GameObject uiElement) && uiElement.tag == "UI")
        {
            Tip.SetActive(true);
            TipInfo _tipinfo = uiElement.GetComponent<TipInfo>();
            Tip_Name.text = _tipinfo.Name;
            Tip_Intro.text = _tipinfo.Intro;
            if (_tipinfo.Type == TipType.Passiveskill)
            {
                Tip_Name.color = RarityColor_Normal;//天賦名稱一律白色
                Tip_tag.SetActive(false);
                Tip_line.SetActive(false);
                Tip_affix.SetActive(false);
                Tip_Level.SetActive(false);
            }
            else if (_tipinfo.Type == TipType.Skill)
            {
                Tip_Name.color = RarityColor_Normal;//技能名稱一律白色
                Tip_tag.SetActive(true);
                Tip_line.SetActive(true);
                Tip_affix.SetActive(true);
                Tip_Level.SetActive(true);
                //顯示tag
                foreach (Transform child in Tip_tag.transform)
                {
                    child.gameObject.SetActive(false);
                }
                for (int i = 0; i < ValueData.Instance.SkillTag[_tipinfo.Id].Length; i++)
                {
                    if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Attack)
                        tagAttack.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Chaos)
                        tagChaos.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Cold)
                        tagCold.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Fire)
                        tagFire.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Lightning)
                        tagLightning.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Movement)
                        tagMovement.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Physical)
                        tagPhysical.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Projectile)
                        tagProjectile.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Spell)
                        tagSpell.gameObject.SetActive(true);
                    else if (ValueData.Instance.SkillTag[_tipinfo.Id][i] == SkillTagType.Range)
                        tagRange.gameObject.SetActive(true);
                }
                //顯示數值
                Tip_Cd.transform.parent.gameObject.SetActive(true);
                Tip_Cost.transform.parent.gameObject.SetActive(true);
                Tip_Dmg.transform.parent.gameObject.SetActive(true);
                Tip_Size.transform.parent.gameObject.SetActive(true);
                Tip_Speed.transform.parent.gameObject.SetActive(true);
                Tip_Crit.transform.parent.gameObject.SetActive(true);
                Tip_CritDmg.transform.parent.gameObject.SetActive(true);

                Tip_Cd.color = valueColor_normal;
                Tip_Cost.color = valueColor_normal;
                Tip_Dmg.color = valueColor_normal;
                Tip_Size.color = valueColor_normal;
                Tip_Speed.color = valueColor_normal;
                Tip_Crit.color = valueColor_normal;
                Tip_CritDmg.color = valueColor_normal;

                Tip_Cd.text = _tipinfo.Cd.ToString("0.0") + " s";
                Tip_Cost.text = _tipinfo.Cost.ToString("0.0");
                Tip_Dmg.text = _tipinfo.Dmg.ToString("0");
                Tip_Size.text = _tipinfo.Size.ToString("0.0");
                Tip_Speed.text = _tipinfo.Speed.ToString("0.0");
                Tip_Crit.text = (_tipinfo.Crit * 100).ToString("0") + " %";
                Tip_CritDmg.text = (_tipinfo.CritDmg * 100).ToString("0") + " %";
                Tip_Intro.text = _tipinfo.Intro;

                //等級
                int childcount = Tip_Level.transform.childCount;
                for (int i = 0; i < childcount; i++)
                {
                    if (i < _tipinfo.Level)
                        Tip_Level.transform.GetChild(i).gameObject.SetActive(true);
                    else
                        Tip_Level.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else if (_tipinfo.Type == TipType.Weapon)
            {
                Tip_tag.SetActive(false);
                Tip_line.SetActive(false);
                Tip_affix.SetActive(true);
                Tip_Level.SetActive(false);
                toText(Tip_Cd, _tipinfo.Cd, "%", false);
                toText(Tip_Cost, _tipinfo.Cost, "%", false);
                toText(Tip_Dmg, _tipinfo.Dmg, "%");
                toText(Tip_Size, _tipinfo.Size, "%");
                toText(Tip_Speed, _tipinfo.Speed, "%");
                toText(Tip_Crit, _tipinfo.Crit, "%");
                toText(Tip_CritDmg, _tipinfo.CritDmg, "%");
                //稀有度顏色
                if (ValueData.Instance.Weapon[_tipinfo.Id].Rarity == RarityType.Normal)
                    Tip_Name.color = RarityColor_Normal;
                else if (ValueData.Instance.Weapon[_tipinfo.Id].Rarity == RarityType.Magic)
                    Tip_Name.color = RarityColor_Magic;
                else if (ValueData.Instance.Weapon[_tipinfo.Id].Rarity == RarityType.Rare)
                    Tip_Name.color = RarityColor_Rare;
                else if (ValueData.Instance.Weapon[_tipinfo.Id].Rarity == RarityType.Unique)
                    Tip_Name.color = RarityColor_Unique;
            }
        }
        else
        {
            Tip.SetActive(false);
        }
            
    }

    //武器tip用
    private void toText(TextMeshProUGUI targetUI , float value, string unit, bool trigger = true)
    {
        
        if (value == 0)
            targetUI.transform.parent.gameObject.SetActive(false);
        else
        { // 數值有加減時才顯示對應UI
            targetUI.transform.parent.gameObject.SetActive(true);
            
            if (value > 0)
            {
                targetUI.text = "+ " + (value * 100).ToString() + " " + unit;
                if(trigger)
                    targetUI.color = valueColor_add;
                else
                    targetUI.color = valueColor_reduce;
            }
            else if (value < 0)
            {
                targetUI.text = "-  " + (value * -100).ToString() + " " + unit;
                if(trigger)
                    targetUI.color = valueColor_reduce;
                else
                    targetUI.color = valueColor_add;
            }
        }
    }

    //單例實體
    public static UICtrl Instance {
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

        if (DontDestroy.Length > 0)
        {
            GameObject[] objects = new GameObject[DontDestroy.Length];
            for (int i = 1; i < objects.Length; i++)
            {
                Destroy(objects[i]);
            }
            //= GameObject.Find(DontDestroy[0].name);
        }

        foreach (GameObject obj in DontDestroy)
        {
            DontDestroyOnLoad(obj);
        }
    }

    void UIUpdate() {
        float valueAP = ValueData.Instance.AP / ValueData.Instance.maxAP;
        Value_AP.fillAmount = valueAP;
        maxapUI.text = ValueData.Instance.maxAP.ToString("0");
        nowapUI.text = ValueData.Instance.AP.ToString("0");
        float valueHP = ValueData.Instance.HP / ValueData.Instance.maxHP;
        Value_HP.fillAmount = valueHP;
        maxhpUI.text = ValueData.Instance.maxHP.ToString("0");
        nowhpUI.text = ValueData.Instance.HP.ToString("0");
        //float valueEXP = ValueData.Instance.EXP / ValueData.Instance.maxEXP;
        //Value_EXP.fillAmount = valueEXP;
    }

    public void GetEXP(float Value) {
        if (Value >= ValueData.Instance.maxEXP - ValueData.Instance.EXP) {
            //升級
            ValueData.Instance.EXP = 0;
            ValueData.Instance.maxEXP += ValueData.Instance.maxEXP / 2;
            LevelUP();
        }
        else {
            ValueData.Instance.EXP += Value;
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
        else if (btnID == 2) {
            if (UpgradeBtn[1] == 0)
                return;
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[1]);
        }
        else if (btnID == 3) {
            if (UpgradeBtn[2] == 0)
                return;
            UpgradeSys.GetComponent<UpgradeSystem>().Upgrade(UpgradeBtn[2]);
        }
        Upgrade.SetActive(false);
        Time.timeScale = 1;
    }

    public void gameover() {
        UIUpdate();
        Time.timeScale = 0;
        GameOverUI.SetActive(true);
    }

    public void retry_Event() {
        Time.timeScale = 1;
        GameOverUI.SetActive(false);
        LevelCtrl.Instance.Enemys.SetActive(false);
        LevelCtrl.Instance.NextLevel(0);
    }

    public void ExitGame() {
        Application.Quit();
    }

    public IEnumerator SkillCD(int FieldID) {
        if (ValueData.Instance.SkillField[FieldID].nowCD <= 0) {
            ValueData.Instance.SkillField[FieldID].nowCD = 0;
            UpdateSkillCD();
        }
        else if (ValueData.Instance.SkillField[FieldID].nowCD <= 0.02f) {
            yield return new WaitForSeconds(ValueData.Instance.SkillField[FieldID].nowCD);
            ValueData.Instance.SkillField[FieldID].nowCD = 0;
            UpdateSkillCD();
        }
        else {
            yield return new WaitForSeconds(0.01f);
            ValueData.Instance.SkillField[FieldID].nowCD -= 0.02f;
            UpdateSkillCD();
            StartCoroutine(SkillCD(FieldID));
        }

    }

    public void UpdateSkillCD() {
        for (int FieldID = 0; FieldID < 3; FieldID++) {
            if (ValueData.Instance.SkillField[FieldID].maxCD == 0)
                SkillCdMask[FieldID].fillAmount = 0;
            else
            {
                float now = ValueData.Instance.SkillField[FieldID].nowCD;
                float max = ValueData.Instance.SkillField[FieldID].maxCD;
                float value = now / max;
                SkillCdMask[FieldID].fillAmount = value;
            }
        }
    }

    public void ChangeSkill(int target , int level) {
        if (target < 0)
            return;
        if (ValueData.Instance.Skill[target].Price > ValueData.Instance.money)
        {
            Debug.Log("金幣不足");
            return;
        }
        if (ChangeSkill_ID[0] == target && ChangeSkill_ID[1] == level) {
            ChangeSkill_ID[0] = -1;
            ChangeSkill_ID[1] = 0;
            SkillFieldSelectUI.SetActive(false);
        }
        else {
            ChangeSkill_ID[0] = target;
            ChangeSkill_ID[1] = level;
            isSpendmoney = true;
            SkillFieldSelectUI.SetActive(true);
        }
    }
    public void SelectSkillChangeField(int Field) {
        SkillFieldSelectUI.SetActive(false);
        ValueData.Instance.SkillField[Field].ID = ValueData.Instance.Skill[ChangeSkill_ID[0]].ID;
        ValueData.Instance.SkillField[Field].Name = ValueData.Instance.Skill[ChangeSkill_ID[0]].Name;
        ValueData.Instance.SkillField[Field].Level = ChangeSkill_ID[1];
        if (ValueData.Instance.SkillField[Field].nowCD == 0)
        {
            ValueData.Instance.SkillField[Field].nowCD = ValueData.Instance.Skill[ChangeSkill_ID[0]].maxCD;
            StartCoroutine(SkillCD(Field));
        }
        SkillFieldIcon[Field].sprite = ValueData.Instance.SkillIcon[ChangeSkill_ID[0]];
        SkillFieldIcon[Field].SetNativeSize();
        if(ValueData.Instance.Skill[ChangeSkill_ID[0]].ID == 0)
            SkillFieldIcon[Field].tag = "Untagged";
        else
            SkillFieldIcon[Field].tag = "UI";
        ValueData.Instance.SkillFieldValueUpdate();
        if (isSpendmoney && ValueData.Instance.Skill[ChangeSkill_ID[0]].Price > 0)
        {
            ValueData.Instance.GetMoney(-ValueData.Instance.Skill[ChangeSkill_ID[0]].Price);
        }
        ChangeSkill_ID[0] = -1;
    }
    public void ChangeWeapon(int target){
        if (target < 0)
            return;
        if (ValueData.Instance.Weapon[target].Price > ValueData.Instance.money)
        {
            Debug.Log("金幣不足");
            return;
        }
        if (ChangeWeapon_ID == target){
            ChangeWeapon_ID = -1;
            WeaponFieldSelectUI.SetActive(false);
        }
        else {
            ChangeWeapon_ID = target;
            isSpendmoney = true;
            WeaponFieldSelectUI.SetActive(true);
        }
    }
    public void SelectWeaponChangeField(int Field)
    {
        WeaponFieldSelectUI.SetActive(false);
        ValueData.Instance.WeaponField[Field].ID = ValueData.Instance.Weapon[ChangeWeapon_ID].ID;
        ValueData.Instance.WeaponField[Field].Name = ValueData.Instance.Weapon[ChangeWeapon_ID].Name;
        ValueData.Instance.WeaponField[Field].Damage = ValueData.Instance.Weapon[ChangeWeapon_ID].Damage;
        ValueData.Instance.WeaponField[Field].Cooldown = ValueData.Instance.Weapon[ChangeWeapon_ID].Cooldown;
        ValueData.Instance.WeaponField[Field].Size = ValueData.Instance.Weapon[ChangeWeapon_ID].Size;
        ValueData.Instance.WeaponField[Field].Speed = ValueData.Instance.Weapon[ChangeWeapon_ID].Speed;
        ValueData.Instance.WeaponField[Field].Cost = ValueData.Instance.Weapon[ChangeWeapon_ID].Cost;
        ValueData.Instance.WeaponField[Field].Crit = ValueData.Instance.Weapon[ChangeWeapon_ID].Crit;
        ValueData.Instance.WeaponField[Field].CritDmg = ValueData.Instance.Weapon[ChangeWeapon_ID].CritDmg;
        WeaponFieldIcon[Field].sprite = ValueData.Instance.WeaponIcon[ChangeWeapon_ID];
        WeaponFieldIcon[Field].tag = "UI";
        WeaponfieldUI.transform.GetChild(Field).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[Field].ID, ValueData.Instance.WeaponField[Field].Name, ValueData.Instance.WeaponField[Field].Cooldown, ValueData.Instance.WeaponField[Field].Cost, ValueData.Instance.WeaponField[Field].Damage, ValueData.Instance.WeaponField[Field].Crit, ValueData.Instance.WeaponField[Field].CritDmg, ValueData.Instance.WeaponField[Field].Size, ValueData.Instance.WeaponField[Field].Speed, 0, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[Field].ID]);
        ValueData.Instance.SkillFieldValueUpdate();
        if (isSpendmoney && ValueData.Instance.Weapon[ChangeWeapon_ID].Price > 0)
        {
            ValueData.Instance.GetMoney(-ValueData.Instance.Weapon[ChangeWeapon_ID].Price);
        }
        ChangeWeapon_ID = -1;
    }

    public void UpdatePassiveSkill() {
        passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
        for (int i = 0; i < passiveskill.Length; i++) { //重製所有天賦，避免出錯
            passiveskill[i].Btn.interactable = false;
            passiveskill[i].Img.color = BtnColor_Normal;
        }
        for (int i = 0; i < passiveskill.Length; i++) {
            if(passiveskill[i].top.Length == 0)
                passiveskill[i].Btn.interactable = true; //開放初始天賦
            if (ValueData.Instance.PassiveSkills[i]) //若已有當前天賦
            {
                passiveskill[i].Btn.interactable = true; //開放點擊(後悔天賦)
                passiveskill[i].Img.color = BtnColor_Have;
                if (ValueData.Instance.passiveskillPoint > 0) //開放下層天賦
                {
                    int x = passiveskill[i].down.Length;
                    for (int z = 0; z < x; z++)
                    {
                        passiveskill[i].down[z].Btn.interactable = true;
                    }
                }
            }
        }
    }
    
    public void UpdateLine() {
        if (LineFather.childCount > 0) { //刪除場上已有的Line
            for (int i = 0; i < LineFather.childCount; i++) {
                Destroy(LineFather.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < passiveskill.Length; i++){ //生成Line
            for (int x = 0; x < passiveskill[i].down.Length; x++)
            {
                Line L = Instantiate(line, LineFather);
                L.line = L.GetComponent<RectTransform>();
                L.button1 = passiveskill[i].Btn;
                L.button2 = passiveskill[i].down[x].Btn;
                L.UpdateLine();
            }
        }
    }

    public void UpdateValueUI() {
        HP_text.text = ValueData.Instance.maxHP.ToString();
        AP_text.text = ValueData.Instance.maxAP.ToString();
        Power_text.text = (ValueData.Instance.Power * 100).ToString();
        Movespeed_text.text = (ValueData.Instance.MoveSpeed * 100).ToString();
        SkillSpeed_text.text = (ValueData.Instance.SkillSpeed * 100).ToString();
        EnemyTimer_text.text = ValueData.Instance.EnemyTimer.ToString();
        AttackSize_text.text = (ValueData.Instance.AttackSize * 100).ToString();
        Cooldown_text.text = (ValueData.Instance.Cooldown * 100).ToString();
        Costdown_text.text = (ValueData.Instance.Cost * 100).ToString();
        Crit_text.text = (ValueData.Instance.Crit * 100).ToString();
        CritDmg_text.text = (ValueData.Instance.CritDmg * 100).ToString("0");
        Damagereduction_text.text = (ValueData.Instance.Damagereduction * 100).ToString("0");
        Bulletspeed_text.text = (ValueData.Instance.BulletSpeed * 100).ToString("0");
        RestoreAP_text.text = ValueData.Instance.RestoreAP.ToString("0.0");
    }

    //開啟技能商店
    public void showSkillstore(bool Switch, List<int> itemID = null , List<int> itemlevel = null) 
    {
        if (Switch)
        {
            SkillStoreUI.SetActive(true);
            UpdateSkillStore(itemID, itemlevel);
            ChangeSkill_ID[0] = -1;
            ChangeSkill_ID[1] = 0;
            WeaponfieldUI.SetActive(true);
        }
        else
        {
            SkillStoreUI.SetActive(false);
            SkillFieldSelectUI.SetActive(false);
            ChangeSkill_ID[0] = -1;
            ChangeSkill_ID[1] = 0;
            if (!settingUI.activeSelf)
                WeaponfieldUI.SetActive(false);
        }
    }

    public void UpdateSkillStore(List<int> itemID , List<int> itemlevel) {
        //清除舊內容
        foreach (Transform child in SkillStoreContent) 
        {
            Destroy(child.gameObject);
        }
        if (itemID.Count <= 0)
            return;
        for (int i = 0; i < itemID.Count; i++) 
        {
            GameObject newButton = Instantiate(StoreskillbuttonPrefab, SkillStoreContent);
            SkillBase skill = ValueData.Instance.Skill[itemID[i]];
            newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Skill, skill.ID, skill.Name, skill.maxCD, skill.Cost, skill.Damage, skill.Crit, skill.CritDmg, skill.Size, skill.Speed, itemlevel[i], ValueData.Instance.SkillIntro[skill.ID]);
            newButton.GetComponent<TipInfo>().UpdatePrice(skill.Price);
            if (ValueData.Instance.SkillIcon.Length > skill.ID)
            {
                Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = ValueData.Instance.SkillIcon[skill.ID];
                icon.SetNativeSize();
            }
            int lv = itemlevel[i];
            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeSkill(skill.ID, lv));
        }
        /*foreach (SkillBase skill in ValueData.Instance.Skill) {
            if (skill.ID != 0) {
                GameObject newButton = Instantiate(StoreskillbuttonPrefab, SkillStoreContent);
                newButton.GetComponent<TipInfo>().UpdateInfo(1, skill.Name, skill.maxCD, skill.Cost, skill.Damage, skill.Crit, skill.Size, skill.Speed, ValueData.Instance.SkillIntro[skill.ID]);
                newButton.transform.Find("Icon").GetComponent<Image>().sprite = ValueData.Instance.SkillIcon[skill.ID];
                newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeSkill(skill.ID));
            }
        }*/
    }

    //開啟武器商店
    public void showWeaponstore(bool Switch , List<int> itemID = null) 
    {
        if (Switch)
        {
            WeaponStoreUI.SetActive(true);
            UpdateWeaponStore(itemID);
            ChangeWeapon_ID = -1;
            WeaponfieldUI.SetActive(true);
        }
        else
        {
            WeaponStoreUI.SetActive(false);
            WeaponFieldSelectUI.SetActive(false);
            ChangeWeapon_ID = -1;
            if(!settingUI.activeSelf)
                WeaponfieldUI.SetActive(false);
        }
        
    }
    public void UpdateWeaponStore(List<int> itemID) {
        //清除舊內容
        foreach (Transform child in WeaponStoreContent)
        {
            Destroy(child.gameObject);
        }
        if (itemID.Count <= 0)
            return;
        for (int i = 0; i < itemID.Count; i++) 
        {
            GameObject newButton = Instantiate(StoreweaponbuttonPrefab, WeaponStoreContent);
            WeaponBase weapon = ValueData.Instance.Weapon[itemID[i]];
            newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, weapon.ID, weapon.Name, weapon.Cooldown, weapon.Cost, weapon.Damage, weapon.Crit, weapon.CritDmg, weapon.Size, weapon.Speed, 0, ValueData.Instance.WeaponIntro[weapon.ID]);
            newButton.GetComponent<TipInfo>().UpdatePrice(weapon.Price);
            if (ValueData.Instance.WeaponIcon.Length > weapon.ID)
            {
                Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = ValueData.Instance.WeaponIcon[weapon.ID];
                icon.SetNativeSize();
            }
            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeWeapon(weapon.ID));
        }
        /*foreach (WeaponBase weapon in ValueData.Instance.Weapon)
        {
            if (weapon.ID != 0)
            {
                GameObject newButton = Instantiate(StoreweaponbuttonPrefab, WeaponStoreContent);
                newButton.GetComponent<TipInfo>().UpdateInfo(2, weapon.Name, weapon.Cooldown, weapon.Costdown, weapon.Damage, weapon.Crit, weapon.Size, weapon.Speed, ValueData.Instance.WeaponIntro[weapon.ID]);
                newButton.transform.Find("Icon").GetComponent<Image>().sprite = ValueData.Instance.WeaponIcon[weapon.ID];
                newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeWeapon(weapon.ID));
            }
        }*/
    }

    //滑鼠射線
    public bool IsPointerOverUI(out GameObject uiElement)
    {
        // 用來儲存射線檢測結果
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        // 射線檢測結果清單
        List<RaycastResult> results = new List<RaycastResult>();

        // 執行射線檢測
        graphicRaycaster.Raycast(pointerEventData, results);

        // 若檢測到UI元素，返回該元素
        if (results.Count > 0)
        {
            uiElement = results[0].gameObject;
            return true;
        }

        uiElement = null;
        return false;
    }

    //傷害數字
    public void ShowDamage(float value, Vector3 position ,bool isCrit) {
        //位置偏移
        Vector3 _position = new Vector3(UnityEngine.Random.Range(position.x-0.5f, position.x+0.5f) , position.y, UnityEngine.Random.Range(position.z-0.5f, position.z+1f));
        //提高高度避免被較高的物體擋到
        Vector3 _canvas = new Vector3(_position.x, worldspaceCanvas.transform.position.y, worldspaceCanvas.transform.position.z);
        Vector3 _lerp = Vector3.Lerp(_position, Camera.main.transform.position , 0.1f);
        TextMeshPro damagetext = Instantiate(DamagetextPrefab, _lerp, worldspaceCanvas.transform.rotation, DamagetextParent.transform);
        damagetext.text = value.ToString("0");
        if (isCrit)
        {
            damagetext.color = DamagetextColor_Crit;
            damagetext.fontSize += 0.8f;
        }
        else {
            damagetext.color = DamagetextColor_Normal;
        }
        
    }

    public void UpdateMoneyUI() {
        MoneyValue.text = ValueData.Instance.money.ToString();
    }

    public void resetlevel()
    {
        LevelCtrl.Instance.NextLevel(LevelCtrl.Instance.nowLevel);
        settingUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void randomweaponstoreEvent()
    {
        if(ValueData.Instance.money < 5)
            return;
        if (!nowWeaponstore)
            return;
        if (!nowWeaponstore.startRandom)
            return;
        ValueData.Instance.money -= 5;
        UpdateMoneyUI();
        nowWeaponstore.RandomItem();
        nowWeaponstore.doNpc(true);
    }
    public void randomskillstoreEvent()
    {
        if (ValueData.Instance.money < 5)
            return;
        if (!nowSkillstore)
            return;
        if (!nowSkillstore.startRandom)
            return;
        ValueData.Instance.money -= 5;
        UpdateMoneyUI();
        nowSkillstore.RandomItem();
        nowSkillstore.doNpc(true);
    }
    public void Newplayer() // 展場用
    {
        LevelCtrl.Instance.NextLevel(0);
        settingUI.SetActive(false);
        Time.timeScale = 1;
        for(int i=0;i< ValueData.Instance.PassiveSkills.Length;i++)
        {
            ValueData.Instance.PassiveSkills[i] = false;
        }
        ValueData.Instance.money = 0;
        UpdateMoneyUI();
        ValueData.Instance.passiveskillPoint = 0;
        ValueData.Instance.PlayerValueUpdate();
        PlayerCtrl.Instance.Start();
        for(int x=0;x<3;x++)
        {
            ChangeWeapon_ID = 0;
            SelectWeaponChangeField(x);
        }
    }

}
