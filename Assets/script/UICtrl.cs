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
    public TextMeshProUGUI touchTargetUI;

    public GameObject Tip_skill;//說明窗相關
    public GameObject Tip_passiveskill;//天賦專用
    public TextMeshProUGUI Tip_Name;
    public TextMeshProUGUI Tip_Intro;
    public TextMeshProUGUI Tip_Cd;
    public TextMeshProUGUI Tip_Cost;
    public TextMeshProUGUI Tip_Dmg;
    public TextMeshProUGUI Tip_Crit;
    public TextMeshProUGUI Tip_Size;
    public TextMeshProUGUI Tip_Speed;
    public TextMeshProUGUI Tip_Passiveskillintro;

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

    public int[] UpgradeBtn;
    public int ChangeSkill_ID;//更換技能的ID暫存
    int ChangeWeapon_ID;//更換裝備的ID暫存
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
    public Line line;
    public Transform LineFather;
    public TextMeshProUGUI passiveskillPoint;//天賦點數

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
        UpdateMoneyUI();
    }

    private void FixedUpdate()
    {
        UIUpdate();//暴力解(暫)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (ValueData.Instance.isUIopen)
            {
                PassiveskilltreeUI.SetActive(false);
                //SkillStoreUI.SetActive(false);
                //WeaponStoreUI.SetActive(false);
                ValueUI.SetActive(false);
                //SkillFieldSelectUI.SetActive(false);
                //WeaponFieldSelectUI.SetActive(false);
                ChangeSkill_ID = 0;
                ChangeWeapon_ID = 0;
                ValueData.Instance.isUIopen = false;
            }
            else {
                //SkillStoreUI.SetActive(true);
                //UpdateSkillStore(new List<int> { 0, 1 });
                //WeaponStoreUI.SetActive(true);
                //UpdateWeaponStore(new List<int> { 0 });
                ValueUI.SetActive(true);
                PassiveskilltreeUI.SetActive(true);
                UpdatePassiveSkill();
                //UpdateLine();
                UpdateValueUI();
                ValueData.Instance.isUIopen = true;
            }
        }

        //Tip彈窗
        if (IsPointerOverUI(out GameObject uiElement) && uiElement.tag == "UI")
        {
            TipInfo _tipinfo = uiElement.GetComponent<TipInfo>();
            if (_tipinfo.Type == TipType.Passiveskill)
            {
                Tip_skill.SetActive(false);
                Tip_passiveskill.SetActive(true);
                Tip_tag.SetActive(false);
            }
            else if (_tipinfo.Type == TipType.Skill)
            {
                Tip_skill.SetActive(true);
                Tip_passiveskill.SetActive(false);
                if (ValueData.Instance.SkillTag[_tipinfo.Id].Length > 0)
                    Tip_tag.SetActive(true);
                else
                    Tip_tag.SetActive(false);
            }
            else
            {
                Tip_skill.SetActive(true);
                Tip_passiveskill.SetActive(false);
                Tip_tag.SetActive(false);
            }

        }
        else
        {
            Tip_skill.SetActive(false);
            Tip_passiveskill.SetActive(false);
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

    public void ChangeSkill(int target) {
        if (target < 0)
            return;
        if (ValueData.Instance.Skill[target].Price > ValueData.Instance.money)
        {
            Debug.Log("金幣不足");
            return;
        }
        if (ChangeSkill_ID == target) {
            ChangeSkill_ID = 0;
            SkillFieldSelectUI.SetActive(false);
        }
        else {
            ChangeSkill_ID = target;
            SkillFieldSelectUI.SetActive(true);
        }
    }
    public void SelectSkillChangeField(int Field) {
        SkillFieldSelectUI.SetActive(false);
        ValueData.Instance.SkillField[Field].ID = ValueData.Instance.Skill[ChangeSkill_ID].ID;
        ValueData.Instance.SkillField[Field].Name = ValueData.Instance.Skill[ChangeSkill_ID].Name;
        if (ValueData.Instance.SkillField[Field].nowCD == 0)
        {
            ValueData.Instance.SkillField[Field].nowCD = ValueData.Instance.Skill[ChangeSkill_ID].maxCD;
            StartCoroutine(SkillCD(Field));
        }
        SkillFieldIcon[Field].sprite = ValueData.Instance.SkillIcon[ChangeSkill_ID];
        SkillFieldIcon[Field].SetNativeSize();
        if(ValueData.Instance.Skill[ChangeSkill_ID].ID == 0)
            SkillFieldIcon[Field].tag = "Untagged";
        else
            SkillFieldIcon[Field].tag = "UI";
        ValueData.Instance.SkillFieldValueUpdate();
        if (ValueData.Instance.Skill[ChangeSkill_ID].Price > 0)
        {
            ValueData.Instance.GetMoney(-ValueData.Instance.Skill[ChangeSkill_ID].Price);
        }
        ChangeSkill_ID = -1;
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
            WeaponFieldSelectUI.SetActive(true);
        }
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
        ValueData.Instance.WeaponField[Field].Crit = ValueData.Instance.Weapon[ChangeWeapon_ID].Crit;
        WeaponFieldIcon[Field].sprite = ValueData.Instance.WeaponIcon[ChangeWeapon_ID];
        WeaponFieldIcon[Field].tag = "UI";
        WeaponfieldUI.transform.GetChild(Field).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[Field].ID, ValueData.Instance.WeaponField[Field].Name, ValueData.Instance.WeaponField[Field].Cooldown, ValueData.Instance.WeaponField[Field].Costdown, ValueData.Instance.WeaponField[Field].Damage, ValueData.Instance.WeaponField[Field].Crit, ValueData.Instance.WeaponField[Field].Size, ValueData.Instance.WeaponField[Field].Speed, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[Field].ID]);
        ValueData.Instance.SkillFieldValueUpdate();
        if (ValueData.Instance.Weapon[ChangeWeapon_ID].Price > 0)
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
        Costdown_text.text = (ValueData.Instance.CostDown * 100).ToString();
        Crit_text.text = (ValueData.Instance.Crit * 100).ToString();
        CritDmg_text.text = (ValueData.Instance.CritDmg * 100).ToString("0");
        Damagereduction_text.text = (ValueData.Instance.Damagereduction * 100).ToString("0");
    }

    //開啟技能商店
    public void showSkillstore(bool Switch, List<int> itemID) 
    {
        if (Switch)
        {
            SkillStoreUI.SetActive(true);
            UpdateSkillStore(itemID);
            ChangeSkill_ID = -1;
        }
        else
        {
            SkillStoreUI.SetActive(false);
            SkillFieldSelectUI.SetActive(false);
            ChangeSkill_ID = -1;
        }
    }

    public void UpdateSkillStore(List<int> itemID) {
        //清除舊內容
        foreach (Transform child in SkillStoreContent) 
        {
            Destroy(child.gameObject);
        }
        if (itemID.Count <= 0)
            return;
        for (int i = 0; i < itemID.Count; i++) {
            GameObject newButton = Instantiate(StoreskillbuttonPrefab, SkillStoreContent);
            SkillBase skill = ValueData.Instance.Skill[itemID[i]];
            newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Skill, skill.ID, skill.Name, skill.maxCD, skill.Cost, skill.Damage, skill.Crit, skill.Size, skill.Speed, ValueData.Instance.SkillIntro[skill.ID]);
            newButton.GetComponent<TipInfo>().UpdatePrice(skill.Price);
            if (ValueData.Instance.SkillIcon.Length > skill.ID)
            {
                Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = ValueData.Instance.SkillIcon[skill.ID];
                icon.SetNativeSize();
            }
            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeSkill(skill.ID));
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
    public void showWeaponstore(bool Switch , List<int> itemID) 
    {
        if (Switch)
        {
            WeaponStoreUI.SetActive(true);
            UpdateWeaponStore(itemID);
            ChangeWeapon_ID = -1;
        }
        else
        {
            WeaponStoreUI.SetActive(false);
            WeaponFieldSelectUI.SetActive(false);
            ChangeWeapon_ID = -1;
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
            newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, weapon.ID, weapon.Name, weapon.Cooldown, weapon.Costdown, weapon.Damage, weapon.Crit, weapon.Size, weapon.Speed, ValueData.Instance.WeaponIntro[weapon.ID]);
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
        Vector3 _position = new Vector3(Random.Range(position.x-0.5f, position.x+0.5f) , position.y, Random.Range(position.z-0.5f, position.z+1f));
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

    public void showTouchtargetName(bool toggle , string name = "")
    {
        touchTargetUI.gameObject.SetActive(toggle);
        touchTargetUI.text = name;
    }

}
