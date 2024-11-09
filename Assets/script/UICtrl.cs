using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.EventSystems;

public class UICtrl : MonoBehaviour
{
    private static UICtrl instance;
    public Canvas canvas;
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
    public GameObject SkillStoreUI;
    public GameObject SkillFieldSelectUI;//選擇技能要放的欄位
    public GameObject WeaponFieldSelectUI;//選擇技能要放的欄位

    public GameObject Tip;//說明窗相關
    public TextMeshProUGUI Tip_Name;

    public TextMeshProUGUI HP_text;//數值欄相關
    public TextMeshProUGUI AP_text;
    public TextMeshProUGUI Atk_text;
    public TextMeshProUGUI Movespeed_text;

    public int[] UpgradeBtn;
    int ChangeSkill_ID;//更換技能的ID暫存
    int ChangeWeapon_ID;//更換裝備的ID暫存
    public Image[] SkillCdMask = new Image[3];//技能CD遮罩
    public Image[] SkillFieldIcon = new Image[3];//已裝備技能icon
    public Image[] WeaponFieldIcon = new Image[3];//已裝備技能icon
    public GameObject _passiveskill;
    public PassiveSkill[] passiveskill;

    public Color BtnColor_Have;//有天賦時的天賦點顏色
    public Color BtnColor_Normal;//無天賦時的
    public Line line;
    public Transform LineFather;

    private EventSystem eventSystem;//滑鼠射線用
    private GraphicRaycaster graphicRaycaster;

    private void Start(){
        ValueData.Instance.PlayerValueUpdate();
        ValueData.Instance.HP = ValueData.Instance.maxHP;
        ValueData.Instance.AP = ValueData.Instance.maxAP;
        ValueData.Instance.EXP = 0;
        UpgradeBtn = new int[3] { 0, 0, 0 };
        passiveskill = _passiveskill.GetComponentsInChildren<PassiveSkill>();
        _passiveskill.transform.parent.gameObject.SetActive(false);
        eventSystem = EventSystem.current;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
    }

    void Update()
    {
        UIUpdate();

        if (Input.GetKeyDown(KeyCode.F)) {
            if (_passiveskill.transform.parent.gameObject.activeSelf) {
                _passiveskill.transform.parent.gameObject.SetActive(false);
                Time.timeScale = 1;
            }
            else {
                _passiveskill.transform.parent.gameObject.SetActive(true);
                UpdatePassiveSkill();
                UpdateLine();
                Time.timeScale = 0;
            }
            UpdateValueUI();
        }
        //Tip彈窗
        if (IsPointerOverUI(out GameObject uiElement))
        {
            Tip.SetActive(true);
            Tip_Name.text = uiElement.name;
        }
        else
            Tip.SetActive(false);
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
    }

    void UIUpdate() {
        float valueAP = ValueData.Instance.AP / ValueData.Instance.maxAP;
        Value_AP.fillAmount = valueAP;
        float valueHP = ValueData.Instance.HP / ValueData.Instance.maxHP;
        Value_HP.fillAmount = valueHP;
        float valueEXP = ValueData.Instance.EXP / ValueData.Instance.maxEXP;
        Value_EXP.fillAmount = valueEXP;
    }

    public void GetEXP(float Value) {
        if (Value >= ValueData.Instance.maxEXP - ValueData.Instance.EXP){
            //升級
            ValueData.Instance.EXP = 0;
            ValueData.Instance.maxEXP += ValueData.Instance.maxEXP /2;
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
        if (ValueData.Instance.SkillField[FieldID].nowCD <= 0.01f && ValueData.Instance.SkillField[FieldID].nowCD > 0) {
            yield return new WaitForSeconds(ValueData.Instance.SkillField[FieldID].nowCD);
            ValueData.Instance.SkillField[FieldID].nowCD = 0;
            UpdateSkillCD();
        }
        else {
            yield return new WaitForSeconds(0.01f);
            ValueData.Instance.SkillField[FieldID].nowCD -= 0.01f;
            UpdateSkillCD();
            StartCoroutine(SkillCD(FieldID));
        }
        
    }

    public void UpdateSkillCD() {
        for (int FieldID = 0; FieldID < 3; FieldID++) {
            float now = ValueData.Instance.SkillField[FieldID].nowCD;
            float max = ValueData.Instance.SkillField[FieldID].maxCD;
            float value = now / max;
            SkillCdMask[FieldID].fillAmount = value;
        }
    }

    public void ChangeSkill(int target) {
        ChangeSkill_ID = target;
        SkillFieldSelectUI.SetActive(true);
    }
    public void SelectSkillChangeField(int Field) {
        SkillFieldSelectUI.SetActive(false);
        ValueData.Instance.SkillField[Field].ID = ValueData.Instance.Skill[ChangeSkill_ID].ID;
        ValueData.Instance.SkillField[Field].Name = ValueData.Instance.Skill[ChangeSkill_ID].Name;
        ValueData.Instance.SkillField[Field].nowCD = 0;
        SkillFieldIcon[Field].sprite = ValueData.Instance.SkillIcon[ChangeSkill_ID];
        SkillFieldIcon[Field].SetNativeSize();
        ValueData.Instance.SkillFieldValueUpdate();
    }
    public void ChangeWeapon(int target){
        ChangeWeapon_ID = target;
        WeaponFieldSelectUI.SetActive(true);
    }
    public void SelectWeaponChangeField(int Field){
        WeaponFieldSelectUI.SetActive(false);
        ValueData.Instance.WeaponField[Field].ID = ValueData.Instance.Weapon[ChangeWeapon_ID].ID;
        ValueData.Instance.WeaponField[Field].Name = ValueData.Instance.Weapon[ChangeWeapon_ID].Name;
        ValueData.Instance.WeaponField[Field].Damage = ValueData.Instance.Weapon[ChangeWeapon_ID].Damage;
        ValueData.Instance.WeaponField[Field].Cooldown = ValueData.Instance.Weapon[ChangeWeapon_ID].Cooldown;
        ValueData.Instance.WeaponField[Field].Size = ValueData.Instance.Weapon[ChangeWeapon_ID].Size;
        ValueData.Instance.WeaponField[Field].Speed = ValueData.Instance.Weapon[ChangeWeapon_ID].Speed;
        ValueData.Instance.WeaponField[Field].Costdown = ValueData.Instance.Weapon[ChangeWeapon_ID].Costdown;
        WeaponFieldIcon[Field].sprite = ValueData.Instance.WeaponIcon[ChangeWeapon_ID];
        ValueData.Instance.SkillFieldValueUpdate();
    }

    public void UpdatePassiveSkill() {
        for (int i = 0; i < passiveskill.Length; i++) { //重製所有天賦，避免出錯
            passiveskill[i].Btn.interactable = false;
            passiveskill[i].Img.color = BtnColor_Normal;
        }
        passiveskill[0].Btn.interactable = true; //開放初始天賦
        for (int i = 0; i < passiveskill.Length; i++) {
            if (ValueData.Instance.PassiveSkills[i]) //若已有當前天賦
            {
                passiveskill[i].Btn.interactable = true; //開放點擊(後悔天賦)
                passiveskill[i].Img.color = BtnColor_Have;
                int x = passiveskill[i].link.Length; //開放下層天賦
                for (int z = 0; z < x; z++) {
                    passiveskill[i].link[z].Btn.interactable = true;
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
            for (int x = 0; x < passiveskill[i].link.Length; x++)
            {
                Line L = Instantiate(line, LineFather);
                L.line = L.GetComponent<RectTransform>();
                L.button1 = passiveskill[i].Btn;
                L.button2 = passiveskill[i].link[x].Btn;
                L.UpdateLine();
            }
        }
    }

    public void UpdateValueUI() {
        HP_text.text = ValueData.Instance.maxHP.ToString();
        AP_text.text = ValueData.Instance.maxAP.ToString();
        Atk_text.text = ValueData.Instance.Power.ToString();
        Movespeed_text.text = ValueData.Instance.MoveSpeed.ToString();
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

}
