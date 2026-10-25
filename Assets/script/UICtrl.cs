using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Vignette = UnityEngine.Rendering.Universal.Vignette;
using System;
using I2.Loc;

public partial class UICtrl : MonoBehaviour
{
    private static UICtrl instance;
    public Canvas canvas;
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
    public GameObject SkillStoreReloadBtn; // 重骰商品按鈕
    public GameObject WeaponStoreUI;
    public GameObject WeaponStoreReloadBtn; // 重骰商品按鈕
    public GameObject BoxUI; // 地上的寶箱
    public GameObject PassiveskilltreeUI;
    public Transform SkillStoreContent;//商店內容頁父物件
    public Transform WeaponStoreContent;
    public Transform BoxContent;
    public GameObject SkillFieldSelectUI;//選擇技能要放的欄位
    public GameObject WeaponFieldSelectUI;//選擇技能要放的欄位
    public GameObject StoreskillbuttonPrefab;
    public GameObject StoreweaponbuttonPrefab;
    public GameObject SkillfieldUI;
    public GameObject WeaponfieldUI;
    public GameObject WeaponbagdUI;
    public GameObject WeaponBagSelectUI;
    public TextMeshPro DamagetextPrefab;//傷害數字
    public GameObject DamagetextParent;//傷害數字的父物件
    public TextMeshProUGUI MoneyValue;
    public TextMeshProUGUI nowhpUI;
    public TextMeshProUGUI maxhpUI;
    public TextMeshProUGUI nowapUI;
    public TextMeshProUGUI maxapUI;
    public GameObject UI_Rage; // 盛怒UI的最上層物件
    public Image Value_Rage;
    public TextMeshProUGUI nowrageUI;
    public TextMeshProUGUI maxrageUI;
    public TMP_Dropdown LanguageDropdown; // 語言選項下拉UI
    public TMP_Dropdown ClassDropdown; // 關卡選項下拉UI
    public Transform UIDrogParent; // 存放當前拖曳的UI，使其不被其他UI覆蓋
    public GameObject GarbageCan;
    public GameObject ButtontipUI; // 按鈕提示UI
    public GameObject passivetreeSelectUI; // 天賦樹選擇UI
    public GameObject[] passivetreePrefab;
    [SerializeField] private UnityEngine.UI.Button[] passiveTreeRefreshButtons = new UnityEngine.UI.Button[3];

    public GameObject Tip; // 說明窗相關
    public RectTransform TiplayoutTransform; // Tip子物件tip的RectTransform，用來更新content size fitter
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
    public TextMeshProUGUI AttackSize_text;
    public TextMeshProUGUI Cooldown_text;
    public TextMeshProUGUI Costdown_text;
    public TextMeshProUGUI Crit_text;
    public TextMeshProUGUI CritDmg_text;
    public TextMeshProUGUI Damagereduction_text;
    public TextMeshProUGUI RestoreAP_text;
    public TextMeshProUGUI Rage_text;

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
    public int[] ChangeSkill_ID = new int[] { 0, 0 };//更換技能的ID暫存 (技能ID, 技能等級)
    public int ChangeWeapon_ID;//更換裝備的ID暫存
    public bool isSpendmoney;//更換技能、裝備時是否消耗金幣
    public Image[] SkillCdMask = new Image[3];//技能CD遮罩
    [SerializeField] private Color activeSkillMaskColor = new Color(1f, 0.85f, 0f, 0.55f);
    private Image[] activeSkillMasks;
    public Image[] SkillFieldIcon = new Image[3];//已裝備技能icon
    public CanvasGroup[] SkillFieldCanvasgroup = new CanvasGroup[3];//已裝備技能icon的CanvasGroup
    public Image[] WeaponFieldIcon = new Image[3];//已裝備武器icon
    //public GameObject _passiveskill;
    public GameObject[] passivetree; // 天賦樹
    public PassiveSkill[] passiveskill; // 天賦點
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
    public Color skillMergePreviewColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color vignetteColor; // Global Volume裡vignette的初始顏色
    public Color vignetteBehurtColor;
    public Color weaponIconRarity_Normal;
    public Color weaponIconRarity_Magic;
    public Color weaponIconRarity_Rare;
    public Color weaponIconRarity_Unique;
    [Header("Skill level background colors")]
    public Color skillLevel1Color = new Color(0.1981132f, 0.1981132f, 0.1981132f, 1f);
    public Color skillLevel2Color = new Color(0.24359201f, 0.30661494f, 0.3207547f, 1f);
    public Color skillLevel3Color = new Color(0.3490566f, 0.34200794f, 0.22556959f, 1f);
    public Color skillLevel4Color = new Color(0.7058299f, 0.5484603f, 0.8018868f, 1f);
    [Header("Store affordability")]
    public Color unaffordableStoreColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public UnityEngine.Rendering.Volume volume; // 鏡頭渲染
    public Vignette vignette;
    public Line line;
    public Transform LineFather;
    public TextMeshProUGUI passiveskillPoint;//天賦點數
    public Npc nowSkillstore; // 儲存現在開的商店，用於重骰等功能
    public Npc nowWeaponstore;
    public int nowSelectitem; // 當前選擇item的list編號
    public GameObject settingUI;
    public TextMeshProUGUI classUI; // 關卡名稱/編號UI

    [SerializeField]
    private Color dragDestinationColor = new Color(1f, 1f, 1f, 0.39215687f);
    private readonly Dictionary<Image, Color> dragDestinationColors = new Dictionary<Image, Color>();

    private readonly Dictionary<Transform, Image> backpackLockOverlays = new Dictionary<Transform, Image>();

    // Keep locked slots visible, but exclude them from all pointer/selection interaction.
    public void UpdateBackpackCapacityUI()
    {
        ValueData data = ValueData.Instance;
        for (int field = data.EquipmentFieldCount; field < WeaponFieldIcon.Length; field++)
        {
            Image icon = WeaponFieldIcon[field];
            if (icon == null) continue;
            Transform slot = icon.transform.parent;
            bool unlocked = data.IsWeaponFieldUnlocked(field);
            // Locked cells show only the dark shade, without their placeholder artwork.
            icon.enabled = unlocked;
            Image backgroundImage = slot.Find("BG")?.GetComponent<Image>();
            if (backgroundImage != null)
                backgroundImage.enabled = unlocked;
            CanvasGroup group = slot.GetComponent<CanvasGroup>();
            if (group == null) group = slot.gameObject.AddComponent<CanvasGroup>();
            group.interactable = unlocked;
            group.blocksRaycasts = unlocked;
            if (!backpackLockOverlays.TryGetValue(slot, out Image overlay) || overlay == null)
            {
                GameObject shade = new GameObject("BackpackCapacityLock", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                shade.layer = slot.gameObject.layer;
                shade.transform.SetParent(slot, false);
                overlay = shade.GetComponent<Image>();
                overlay.color = new Color(0f, 0f, 0f, 0.85f);
                overlay.raycastTarget = false;
                RectTransform background = slot.Find("BG") as RectTransform;
                if (background != null)
                {
                    // Slot roots are larger than their visible backgrounds; shade the actual cell only.
                    overlay.rectTransform.anchorMin = background.anchorMin;
                    overlay.rectTransform.anchorMax = background.anchorMax;
                    overlay.rectTransform.pivot = background.pivot;
                    overlay.rectTransform.sizeDelta = background.sizeDelta;
                    overlay.rectTransform.anchoredPosition = background.anchoredPosition;
                    overlay.rectTransform.localScale = background.localScale;
                    overlay.rectTransform.localRotation = background.localRotation;
                }
                else
                {
                    overlay.rectTransform.anchorMin = Vector2.zero;
                    overlay.rectTransform.anchorMax = Vector2.one;
                    overlay.rectTransform.offsetMin = Vector2.zero;
                    overlay.rectTransform.offsetMax = Vector2.zero;
                }
                backpackLockOverlays[slot] = overlay;
            }
            overlay.transform.SetAsLastSibling();
            overlay.gameObject.SetActive(!unlocked);
            int bagIndex = field - data.EquipmentFieldCount;
            if (WeaponBagSelectUI != null && bagIndex < WeaponBagSelectUI.transform.childCount)
            {
                GameObject selection = WeaponBagSelectUI.transform.GetChild(bagIndex).gameObject;
                selection.SetActive(unlocked);
                if (selection.TryGetComponent<UnityEngine.UI.Button>(out UnityEngine.UI.Button button))
                    button.interactable = unlocked;
            }
        }
    }

    private TipInfo displayedTip;
    private uint displayedTipRevision;
    private string displayedTipLanguage;
    private bool displayedSkillMergePreview;

    private EventSystem eventSystem;//滑鼠射線用
    private GraphicRaycaster graphicRaycaster;

    private void Start() {
        RefreshStageDropdown();
        ValueData.Instance.PlayerValueUpdate();
        ValueData.Instance.HP = ValueData.Instance.maxHP;
        ValueData.Instance.AP = ValueData.Instance.maxAP;
        ValueData.Instance.EXP = 0;
        UpgradeBtn = new int[3] { 0, 0, 0 };
        eventSystem = EventSystem.current;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        ValueData.Instance.isUIopen = false;
        SkillStoreUI.SetActive(false);
        WeaponStoreUI.SetActive(false);
        //UpdatePassiveSkill();
        PassiveskilltreeUI.SetActive(false);
        ValueUI.SetActive(false);
        WeaponfieldUI.SetActive(false);
        WeaponbagdUI.SetActive(false);
        SetWeaponSelectionActive(false);
        UpdateMoneyUI();
        if (volume != null && volume.profile != null)
            volume.profile.TryGet(out vignette);
        GarbageCan.SetActive(false);
        ButtontipUI.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            SkillFieldCanvasgroup[i] = SkillFieldIcon[i].GetComponent<CanvasGroup>();
        }
        UpdateBackpackCapacityUI();
        RefreshSkillLevelColors();
    }

    private void FixedUpdate()
    {
        UIUpdate();//暴力解(暫)
    }

    private void LateUpdate()
    {
        UpdateActiveSkillMasks();
        RefreshOwnedItemDropArea();
    }

    private void UpdateActiveSkillMasks()
    {
        if (SkillFieldIcon == null) return;
        if (activeSkillMasks == null)
            activeSkillMasks = new Image[SkillFieldIcon.Length];

        for (int fieldId = 0; fieldId < activeSkillMasks.Length; fieldId++)
        {
            Image icon = SkillFieldIcon[fieldId];
            if (icon == null) continue;
            Image cooldown = SkillCdMask[fieldId];
            if (cooldown == null) continue;
            Image background = cooldown.transform.parent.Find("BG")?.GetComponent<Image>();
            Image buttonShape = background != null ? background : cooldown;
            Image mask = activeSkillMasks[fieldId];
            if (mask == null)
            {
                var overlay = new GameObject("ActiveMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                overlay.layer = icon.gameObject.layer;
                // Cover the button background, immediately below the cooldown layer.
                overlay.transform.SetParent(cooldown.transform.parent, false);
                overlay.transform.SetSiblingIndex(cooldown.transform.GetSiblingIndex());
                mask = overlay.GetComponent<Image>();
                mask.raycastTarget = false;
                activeSkillMasks[fieldId] = mask;
            }
            RectTransform shape = buttonShape.rectTransform;
            mask.rectTransform.anchorMin = shape.anchorMin;
            mask.rectTransform.anchorMax = shape.anchorMax;
            mask.rectTransform.pivot = shape.pivot;
            mask.rectTransform.sizeDelta = shape.sizeDelta;
            mask.rectTransform.anchoredPosition3D = shape.anchoredPosition3D;
            mask.rectTransform.localScale = shape.localScale;
            mask.rectTransform.localRotation = shape.localRotation;
            mask.sprite = buttonShape.overrideSprite;
            mask.type = background != null ? background.type : Image.Type.Simple;
            mask.fillAmount = 1f;
            mask.preserveAspect = buttonShape.preserveAspect;
            mask.pixelsPerUnitMultiplier = buttonShape.pixelsPerUnitMultiplier;
            mask.color = activeSkillMaskColor;
            bool active = Skill.Instance != null && Skill.Instance.IsSkillActive(fieldId);
            if (mask.gameObject.activeSelf != active)
                mask.gameObject.SetActive(active);
        }
    }

    void Update()
    {
        UpdateSkillCooldowns(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.Tab)) { // 天賦介面 Tab
            if (settingUI.activeSelf)
                return;
            if (ValueData.Instance.isUIopen)
            {
                PassiveskilltreeUI.SetActive(false);
                ValueUI.SetActive(false);
                ButtontipUI.SetActive(true);
                ChangeSkill_ID = new int[] { -1, 0 };
                ChangeWeapon_ID = -1;
                SkillFieldSelectUI.SetActive(false);
                SetWeaponSelectionActive(false);
                ValueData.Instance.isUIopen = false;
                if(!SkillStoreUI.activeSelf && !WeaponStoreUI.activeSelf)
                {
                    WeaponfieldUI.SetActive(false);
                    WeaponbagdUI.SetActive(false);
                }
                if(!GameOverUI.activeSelf)
                    Time.timeScale = 1;
            }
            else {
                ValueUI.SetActive(true);
                ButtontipUI.SetActive(false);
                PassiveskilltreeUI.SetActive(true);
                UpdatePassiveSkill();
                RefreshPassiveTreeScroll();
                UpdateValueUI();
                ValueData.Instance.isUIopen = true;
                WeaponfieldUI.SetActive(true);
                WeaponbagdUI.SetActive(true);
                Time.timeScale = 0;
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape)) // 暫停選單 Esc
        {
            if (!settingUI.activeSelf)
            {
                PlayerCtrl.Instance.openedTarget = null;
                settingUI.SetActive(true);
                RefreshStageDropdown();
                StartLanguageDropdown();
                showWeaponstore(false);
                showSkillstore(false);
                Time.timeScale = 0;
            }
            else
            {
                settingUI.SetActive(false);
                if (!ValueData.Instance.isUIopen && !GameOverUI.activeSelf)
                    Time.timeScale = 1;
            }
        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            callPassivetreeSelect();
        }

        //Tip彈窗
        if (TryGetHoveredTip(out TipInfo _tipinfo))
        {
            bool mergePreview = TryGetSkillMergePreview(_tipinfo, out SkillBase currentStats, out SkillBase upgradedStats);
            if (!ShouldRefreshTip(_tipinfo) && !mergePreview && !displayedSkillMergePreview)
                return;
            Tip.SetActive(true);
            
            if (_tipinfo.Type == TipType.Passiveskill)
            {
                Tip_Name.color = RarityColor_Normal;//天賦名稱一律白色
                Tip_tag.SetActive(false);
                Tip_line.SetActive(false);
                Tip_affix.SetActive(false);
                Tip_Level.SetActive(false);

                LocalizationManager.TryGetTranslation("Passiveskill/Passiveskill Name " + _tipinfo.Id, out string tipname);
                Tip_Name.text = tipname;
                LocalizationManager.TryGetTranslation("Passiveskill/Passiveskill Info " + _tipinfo.Id, out string tipinfo);
                Tip_Intro.text = tipinfo;
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

                Tip_Cd.text = _tipinfo.Cd.ToString("0.00") + " s";
                Tip_Cost.text = _tipinfo.Cost.ToString("0.0");
                Tip_Dmg.text = _tipinfo.Dmg.ToString("0");
                Tip_Size.text = _tipinfo.Size.ToString("0.0");
                Tip_Speed.text = _tipinfo.Speed.ToString("0.0");
                Tip_Crit.text = (_tipinfo.Crit * 100).ToString("0") + " %";
                Tip_CritDmg.text = (_tipinfo.CritDmg * 100).ToString("0") + " %";
                if (mergePreview)
                    ShowSkillMergeValues(currentStats, upgradedStats);

                LocalizationManager.TryGetTranslation("Skill/Skill Name " + _tipinfo.Id, out string tipname);
                Tip_Name.text = tipname;
                LocalizationManager.TryGetTranslation("Skill/Skill Info " + _tipinfo.Id, out string tipinfo);
                int tipLevel = mergePreview ? upgradedStats.Level : _tipinfo.Level;
                if (_tipinfo.Id == 10)
                {
                    var stats = ValueData.Instance.GetSkillLevelStats(10, tipLevel);
                    var previous = mergePreview ? ValueData.Instance.GetSkillLevelStats(10, _tipinfo.Level) : null;
                    tipinfo = string.Format(tipinfo ?? string.Empty,
                        FormatSkillMergeNumber((stats?.WaterballManaRatio ?? 0.5f) * 100f, previous?.WaterballManaRatio * 100f, "0.#"),
                        FormatSkillMergeNumber(stats?.WaterballDamagePerMana ?? 10f, previous?.WaterballDamagePerMana, "0.##"));
                }
                if (_tipinfo.Id == ValueData.SunfireSkillId)
                {
                    var stats = ValueData.Instance.GetSkillLevelStats(ValueData.SunfireSkillId, tipLevel);
                    var previous = mergePreview ? ValueData.Instance.GetSkillLevelStats(ValueData.SunfireSkillId, _tipinfo.Level) : null;
                    tipinfo = string.Format(tipinfo ?? string.Empty,
                        FormatSkillMergeNumber((stats?.SunfireLifeRatio ?? 0.5f) * 100f, previous?.SunfireLifeRatio * 100f, "0.##"),
                        FormatSkillMergeNumber(stats?.SunfireInterval ?? 0.5f, previous?.SunfireInterval, "0.###"),
                        FormatSkillMergeNumber(stats?.Size ?? 6f, previous?.Size, "0.##"),
                        FormatSkillMergeNumber(stats?.Cost ?? 0.5f, previous?.Cost, "0.##"));
                }
                Tip_Intro.text = tipinfo;

                //等級
                int childcount = Tip_Level.transform.childCount;
                for (int i = 0; i < childcount; i++)
                {
                    if (i < tipLevel)
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

                LocalizationManager.TryGetTranslation("Weapon/Weapon Name " + _tipinfo.Id, out string tipname);
                Tip_Name.text = tipname;
                LocalizationManager.TryGetTranslation("Weapon/Weapon Info " + _tipinfo.Id, out string tipinfo);
                Tip_Intro.text = ValueData.Instance.Weapon[_tipinfo.Id].FormatDescription(tipinfo);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(TiplayoutTransform);
            displayedTip = _tipinfo;
            displayedTipRevision = _tipinfo.Revision;
            displayedTipLanguage = LocalizationManager.CurrentLanguage;
            displayedSkillMergePreview = mergePreview;
        }
        else
        {
            displayedTip = null;
            displayedSkillMergePreview = false;
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
        if (Skill.Instance != null) Skill.Instance.StopAllSunfire();
        ValueData.Instance.immortal = true;
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

    private void UpdateSkillCooldowns(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        bool updated = false;
        for (int i = 0; i < ValueData.Instance.SkillField.Length; i++)
        {
            SkillFieldBase field = ValueData.Instance.SkillField[i];
            if (field.nowCD == 0f)
                continue;

            field.nowCD = Mathf.Max(0f, field.nowCD - deltaTime);
            updated = true;
        }

        if (updated)
            UpdateSkillCD();
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

    public Color GetSkillLevelColor(int level)
    {
        if (level >= 4) return skillLevel4Color;
        if (level == 3) return skillLevel3Color;
        if (level == 2) return skillLevel2Color;
        return skillLevel1Color;
    }

    private void SetSkillBackgroundColor(Image background, int level)
    {
        if (background == null) return;
        Color color = GetSkillLevelColor(level);
        // A highlighted drop target stays white, then restores the current level color.
        if (dragDestinationColors.ContainsKey(background))
            dragDestinationColors[background] = color;
        else
            background.color = color;
    }

    [ContextMenu("Refresh Skill Level Colors")]
    public void RefreshSkillLevelColors()
    {
        if (ValueData.Instance != null)
            for (int i = 0; i < SkillFieldIcon.Length; i++)
            {
                if (SkillFieldIcon[i] == null) continue;
                Transform background = SkillFieldIcon[i].transform.parent.Find("BG");
                if (background != null)
                    SetSkillBackgroundColor(background.GetComponent<Image>(),
                        ValueData.Instance.SkillField[i].ID == 0 ? 0 : ValueData.Instance.SkillField[i].Level);
            }
        RefreshSkillListColors(SkillStoreContent);
        RefreshSkillListColors(BoxContent);
    }

    private void RefreshSkillListColors(Transform content)
    {
        if (content == null) return;
        foreach (TipInfo item in content.GetComponentsInChildren<TipInfo>(true))
            if (item.Type == TipType.Skill)
                SetSkillBackgroundColor(item.GetComponent<Image>(), item.Level);
    }

    public void BeginDragHighlights(TipType type, int sourceField = -1)
    {
        EndDragHighlights();
        if (type == TipType.SkillField)
            type = TipType.Skill;
        else if (type == TipType.WeaponField)
            type = TipType.Weapon;
        if (type != TipType.Skill && type != TipType.Weapon)
            return;

        Image[] icons = type == TipType.Skill ? SkillFieldIcon : WeaponFieldIcon;
        for (int i = 0; i < icons.Length; i++)
        {
            if (i == sourceField || !IsPurchaseDestinationAvailable(type, i))
                continue;
            Transform background = icons[i].transform.parent.Find("BG");
            if (background == null || !background.TryGetComponent<Image>(out Image image))
                continue;
            dragDestinationColors[image] = image.color;
            image.color = dragDestinationColor;
        }
    }

    public void EndDragHighlights()
    {
        EndSkillMergePreview();
        foreach (KeyValuePair<Image, Color> entry in dragDestinationColors)
        {
            if (entry.Key != null)
                entry.Key.color = entry.Value;
        }
        dragDestinationColors.Clear();
    }

    public void CancelStoreItemSelection()
    {
        ChangeSkill_ID[0] = -1;
        ChangeSkill_ID[1] = 0;
        ChangeWeapon_ID = -1;
        SkillFieldSelectUI.SetActive(false);
        SetWeaponSelectionActive(false);
    }

    public bool CanPurchaseStoreItem(TipType type, int id, int level, int index, bool paid, Npc store)
    {
        if (store == null || index < 0 || index >= store.item.Count || store.item[index] != id || id <= 0)
            return false;
        if (type == TipType.Skill)
        {
            if (store != nowSkillstore || id >= ValueData.Instance.Skill.Length ||
                index >= store.itemlevel.Count || store.itemlevel[index] != level)
                return false;
            if (paid ? (!SkillStoreUI.activeInHierarchy || store.npcType != NpcType.Skillstore)
                     : (!BoxUI.activeInHierarchy || store.npcType != NpcType.Skill))
                return false;
            return !paid || ValueData.Instance.money >= ValueData.Instance.GetSkillBase(id, level).Price;
        }
        if (type == TipType.Weapon)
        {
            if (store != nowWeaponstore || id >= ValueData.Instance.Weapon.Length)
                return false;
            if (paid ? (!WeaponStoreUI.activeInHierarchy || store.npcType != NpcType.Weaponstore)
                     : (!BoxUI.activeInHierarchy || store.npcType != NpcType.Weapon))
                return false;
            return !paid || ValueData.Instance.money >= ValueData.Instance.Weapon[id].Price;
        }
        return false;
    }

    private bool IsPurchaseDestinationAvailable(TipType type, int field)
    {
        Image[] icons = type == TipType.Skill ? SkillFieldIcon : WeaponFieldIcon;
        return (type == TipType.Skill || type == TipType.Weapon) && field >= 0 && field < icons.Length &&
            icons[field] != null && icons[field].transform.parent.gameObject.activeInHierarchy &&
            (type != TipType.Weapon || ValueData.Instance.IsWeaponFieldUnlocked(field));
    }

    public bool TryGetPurchaseDropField(GameObject hit, TipType type, out int field)
    {
        field = -1;
        if (hit == null || (type != TipType.Skill && type != TipType.Weapon))
            return false;
        Image[] icons = type == TipType.Skill ? SkillFieldIcon : WeaponFieldIcon;
        for (int i = 0; i < icons.Length; i++)
        {
            if (!IsPurchaseDestinationAvailable(type, i))
                continue;
            Transform buttons = type == TipType.Skill ? SkillFieldSelectUI.transform :
                (i < WeaponfieldUI.transform.childCount ? WeaponFieldSelectUI.transform :
                    (WeaponBagSelectUI != null ? WeaponBagSelectUI.transform : null));
            int buttonIndex = type == TipType.Weapon && i >= WeaponfieldUI.transform.childCount ?
                i - WeaponfieldUI.transform.childCount : i;
            if (hit.transform.IsChildOf(icons[i].transform.parent) ||
                (buttons != null && buttonIndex < buttons.childCount &&
                 hit.transform.IsChildOf(buttons.GetChild(buttonIndex))))
            {
                field = i;
                return true;
            }
        }
        return false;
    }

    public bool PurchaseDraggedStoreItem(TipType type, int id, int level, int index, bool paid, Npc store, int field)
    {
        if (!CanPurchaseStoreItem(type, id, level, index, paid, store) ||
            !IsPurchaseDestinationAvailable(type, field))
            return false;
        CancelStoreItemSelection();
        if (type == TipType.Skill)
        {
            ChangeSkill(id, level, paid, index);
            SelectSkillChangeField(field);
        }
        else
        {
            ChangeWeapon(id, paid, index);
            SelectWeaponChangeField(field);
        }
        return true;
    }

    // 點擊商店的商品時
    public void ChangeSkill(int target , int level, bool isCostmoney = true, int itemListID = 0) {
        if (target < 0)
            return;
        if (ValueData.Instance.GetSkillBase(target, level).Price > ValueData.Instance.money && isCostmoney)
        {
            Debug.Log("金幣不足");
            return;
        }
        SetWeaponSelectionActive(false);
        ChangeWeapon_ID = -1;
        if (ChangeSkill_ID[0] == target && ChangeSkill_ID[1] == level) {
            ChangeSkill_ID[0] = -1;
            ChangeSkill_ID[1] = 0;
            SkillFieldSelectUI.SetActive(false);
        }
        else {
            ChangeSkill_ID[0] = target;
            ChangeSkill_ID[1] = level;
            nowSelectitem = itemListID;
            if (isCostmoney)
                isSpendmoney = true;
            else
                isSpendmoney = false;
            SkillFieldSelectUI.SetActive(true);
        }
    }
    // 點擊要放的欄位時
    public void SelectSkillChangeField(int Field) {
        if (ChangeSkill_ID[0] < 0 || Field < 0 || Field >= ValueData.Instance.SkillField.Length)
            return;
        int price = ValueData.Instance.GetSkillBase(ChangeSkill_ID[0], ChangeSkill_ID[1]).Price;
        if (isSpendmoney && price > ValueData.Instance.money)
            return;
       
        if (isSpendmoney && price > 0)
        {
            ValueData.Instance.GetMoney(-price);
        }
        if (nowSkillstore != null && ChangeSkill_ID[0] != 0 && !nowSkillstore.unlimited) // 如果當前為開啟技能商店，則從該商店移除該商品
        {
            nowSkillstore.item.RemoveAt(nowSelectitem);
            nowSkillstore.itemlevel.RemoveAt(nowSelectitem);
            nowSkillstore.doNpc(true);
            if (nowSkillstore.npcType == NpcType.Skill) // 如果是寶箱，且已無內容物，則刪除寶箱
            {
                if (nowSkillstore.item.Count == 0)
                {
                    GameObject box = nowSkillstore.gameObject;
                    nowSkillstore.doNpc(false);
                    if(box.TryGetComponent<Collider>(out Collider _box)) // 因為Destroy不會觸發OnTriggerExit，所以手動呼叫
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(box);
                }
            }
        }
        DoChangeSkill(Field);
    }

    public void DoChangeSkill(int Field)
    {
        SkillFieldSelectUI.SetActive(false);

        int oldlv = ValueData.Instance.SkillField[Field].Level;
        int newlv = ChangeSkill_ID[1];
        if (CanMergeSkills(ChangeSkill_ID[0], newlv, ValueData.Instance.SkillField[Field].ID, oldlv)) // 相同ID、等級的技能會升級
        {
            ChangeSkill_ID[1] += 1;
        }
        else
        {
            if (newlv < oldlv)//如果新技能等級<舊技能，擠掉欄位上的裝備
            {
                for (int i = oldlv; i > newlv; i--)
                {
                    if (ValueData.Instance.WeaponField[Field * 3 + (i - 1)].ID != 0)
                    {
                        ChangeWeapon_ID = 0;
                        SelectWeaponChangeField(Field * 3 + (i - 1));
                    }
                }
            }
            // 被擠掉的技能變成寶箱
            if (ValueData.Instance.SkillField[Field].ID != 0)
            {
                LevelCtrl.Instance.doItemskill(ValueData.Instance.SkillField[Field].ID, ValueData.Instance.SkillField[Field].Level);
            }
        }

        if (ValueData.Instance.SkillField[Field].ID != ChangeSkill_ID[0] && Skill.Instance != null)
            Skill.Instance.StopSunfire(Field);
        ValueData.Instance.SkillField[Field].ID = ValueData.Instance.Skill[ChangeSkill_ID[0]].ID;
        ValueData.Instance.SkillField[Field].Name = ValueData.Instance.Skill[ChangeSkill_ID[0]].Name;
        ValueData.Instance.SkillField[Field].Level = ChangeSkill_ID[1];
        if (ValueData.Instance.SkillField[Field].nowCD == 0)
        {
            ValueData.Instance.SkillField[Field].nowCD = ValueData.Instance.GetFieldSkillBase(Field).maxCD;
        }
        SkillFieldIcon[Field].sprite = ValueData.Instance.SkillIcon[ChangeSkill_ID[0]];
        SkillFieldIcon[Field].SetNativeSize();
        SetSkillBackgroundColor(SkillFieldIcon[Field].transform.parent.Find("BG").GetComponent<Image>(),
            ValueData.Instance.SkillField[Field].ID == 0 ? 0 : ValueData.Instance.SkillField[Field].Level);
        if (ValueData.Instance.Skill[ChangeSkill_ID[0]].ID == 0)
            SkillFieldIcon[Field].tag = "Untagged";
        else
            SkillFieldIcon[Field].tag = "UI";
        ValueData.Instance.SkillFieldValueUpdate();
        UpdateSkillCD();

        ChangeSkill_ID[0] = -1;
        ChangeSkill_ID[1] = 0;
    }

    // 點擊商店的商品時
    public void ChangeWeapon(int target , bool isCostmoney = true , int itemListID = 0){
        if (target < 0)
            return;
        if (ValueData.Instance.Weapon[target].Price > ValueData.Instance.money && isCostmoney)
        {
            Debug.Log("金幣不足");
            return;
        }
        SkillFieldSelectUI.SetActive(false);
        ChangeSkill_ID[0] = -1;
        ChangeSkill_ID[1] = 0;
        if (ChangeWeapon_ID == target){
            ChangeWeapon_ID = -1;
            SetWeaponSelectionActive(false);
        }
        else {
            ChangeWeapon_ID = target;
            nowSelectitem = itemListID;
            if (isCostmoney)
                isSpendmoney = true;
            else
                isSpendmoney = false;
            SetWeaponSelectionActive(true);
            // 顯示已有裝備欄位的選擇UI
            int nowweaponfield = WeaponfieldUI.transform.childCount;
            for (int i = 0; i < nowweaponfield; i++)
            {
                bool havefield = WeaponfieldUI.transform.GetChild(i).gameObject.activeSelf;
                GameObject thisfield = WeaponFieldSelectUI.transform.GetChild(i).gameObject;
                thisfield.SetActive(havefield);
            }
        }
    }
    private void SetWeaponSelectionActive(bool active)
    {
        WeaponFieldSelectUI.SetActive(active);
        if (WeaponBagSelectUI != null)
            WeaponBagSelectUI.SetActive(active);
        UpdateBackpackCapacityUI();
    }

    // 點擊要放的欄位時
    public void SelectWeaponChangeField(int Field)
    {
        if (ChangeWeapon_ID < 0 || !ValueData.Instance.IsWeaponFieldUnlocked(Field))
            return;
        if (isSpendmoney && ValueData.Instance.Weapon[ChangeWeapon_ID].Price > ValueData.Instance.money)
            return;
        if (isSpendmoney && ValueData.Instance.Weapon[ChangeWeapon_ID].Price > 0)
        {
            ValueData.Instance.GetMoney(-ValueData.Instance.Weapon[ChangeWeapon_ID].Price);
        }
        
        if(nowWeaponstore != null && ChangeWeapon_ID != 0 && !nowWeaponstore.unlimited) // 如果當前為開啟武器商店，則從該商店移除該商品
        {
            nowWeaponstore.item.RemoveAt(nowSelectitem);
            nowWeaponstore.doNpc(true);
            if (nowWeaponstore.npcType == NpcType.Weapon) // 如果是寶箱，且已無內容物，則刪除寶箱
            {
                if (nowWeaponstore.item.Count == 0)
                {
                    GameObject box = nowWeaponstore.gameObject;
                    nowWeaponstore.doNpc(false);
                    if (box.TryGetComponent<Collider>(out Collider _box)) // 因為Destroy不會觸發OnTriggerExit，所以手動呼叫
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(box);
                }
            }
            
        }
        DoChangeWeapon(Field);
    }

    public void DoChangeWeapon(int Field)
    {
        // Clearing remains available for ResetLoadout, including the locked backpack slots.
        if (Field < 0 || Field >= ValueData.Instance.WeaponField.Length || ChangeWeapon_ID < 0 ||
            ChangeWeapon_ID >= ValueData.Instance.Weapon.Length ||
            (ChangeWeapon_ID != 0 && !ValueData.Instance.IsWeaponFieldUnlocked(Field)))
            return;
        SetWeaponSelectionActive(false);
        // 被擠掉的裝備變成裝備寶箱
        if (ValueData.Instance.WeaponField[Field].ID != 0)
        {
            LevelCtrl.Instance.doItemweapon(ValueData.Instance.WeaponField[Field].ID);
        }
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
        WeaponFieldIcon[Field].SetNativeSize();
        WeaponFieldIcon[Field].tag = "UI";

        // 稀有度背景顏色
        Image _BG = WeaponFieldIcon[Field].transform.parent.Find("BG").GetComponent<Image>();
        if (ValueData.Instance.Weapon[ChangeWeapon_ID].Rarity == RarityType.Normal)
            _BG.color = weaponIconRarity_Normal;
        else if (ValueData.Instance.Weapon[ChangeWeapon_ID].Rarity == RarityType.Magic)
            _BG.color = weaponIconRarity_Magic;
        else if (ValueData.Instance.Weapon[ChangeWeapon_ID].Rarity == RarityType.Rare)
            _BG.color = weaponIconRarity_Rare;
        else if (ValueData.Instance.Weapon[ChangeWeapon_ID].Rarity == RarityType.Unique)
            _BG.color = weaponIconRarity_Unique;

        WeaponFieldIcon[Field].GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[Field].ID, ValueData.Instance.WeaponField[Field].Name, ValueData.Instance.WeaponField[Field].Cooldown, ValueData.Instance.WeaponField[Field].Cost, ValueData.Instance.WeaponField[Field].Damage, ValueData.Instance.WeaponField[Field].Crit, ValueData.Instance.WeaponField[Field].CritDmg, ValueData.Instance.WeaponField[Field].Size, ValueData.Instance.WeaponField[Field].Speed, 0, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[Field].ID]);
        //WeaponfieldUI.transform.GetChild(Field).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[Field].ID, ValueData.Instance.WeaponField[Field].Name, ValueData.Instance.WeaponField[Field].Cooldown, ValueData.Instance.WeaponField[Field].Cost, ValueData.Instance.WeaponField[Field].Damage, ValueData.Instance.WeaponField[Field].Crit, ValueData.Instance.WeaponField[Field].CritDmg, ValueData.Instance.WeaponField[Field].Size, ValueData.Instance.WeaponField[Field].Speed, 0, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[Field].ID]);
        ValueData.Instance.SkillFieldValueUpdate();
        ChangeWeapon_ID = -1;
    }

    public void UpdatePassiveSkill() {
        passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
        for (int i = 0; i < passiveskill.Length; i++) { //重製所有天賦，避免出錯
            if(passiveskill[i].gameObject.activeInHierarchy == true)
            {
                passiveskill[i].Btn.interactable = false;
                passiveskill[i].Img.color = BtnColor_Normal;
            }
        }
        for (int i = 0; i < passiveskill.Length; i++) {
            if (passiveskill[i].gameObject.activeInHierarchy == false)
                continue;
            if(passiveskill[i].top.Length == 0)
            {
                passiveskill[i].Btn.interactable = true; //開放初始天賦
            }
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
        Power_text.text = (ValueData.Instance.Power * 100).ToString("0");
        Movespeed_text.text = (ValueData.Instance.MoveSpeed * 100).ToString("0");
        SkillSpeed_text.text = (ValueData.Instance.SkillSpeed * 100).ToString("0");
        AttackSize_text.text = (ValueData.Instance.AttackSize * 100).ToString("0");
        Cooldown_text.text = (ValueData.Instance.Cooldown * 100).ToString("0");
        Costdown_text.text = (ValueData.Instance.Cost * 100).ToString("0");
        Crit_text.text = (ValueData.Instance.Crit * 100).ToString("0");
        CritDmg_text.text = (ValueData.Instance.CritDmg * 100).ToString("0");
        Damagereduction_text.text = (ValueData.Instance.Damagereduction * 100).ToString("0");
        RestoreAP_text.text = ValueData.Instance.RestoreAP.ToString("0.0");
        Rage_text.text = ValueData.Instance.maxRage.ToString();
    }

    //開啟技能商店
    public void showSkillstore(bool Switch, List<int> itemID = null , List<int> itemlevel = null) 
    {
        if (Switch)
        {
            SkillStoreUI.SetActive(true);
            WeaponStoreUI.SetActive(false);
            BoxUI.SetActive(false);
            SetWeaponSelectionActive(false);
            UpdateSkillStore(itemID, itemlevel);
            //ChangeSkill_ID[0] = -1;
            //ChangeSkill_ID[1] = 0;
            WeaponfieldUI.SetActive(true);
            WeaponbagdUI.SetActive(true);
            ButtontipUI.SetActive(false);
        }
        else
        {
            SkillStoreUI.SetActive(false);
            SkillFieldSelectUI.SetActive(false);
            //ChangeSkill_ID[0] = -1;
            //ChangeSkill_ID[1] = 0;
            if (!ValueData.Instance.isUIopen)
            {
                WeaponfieldUI.SetActive(false);
                WeaponbagdUI.SetActive(false);
                ButtontipUI.SetActive(true);
            }
                
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
            SkillBase skill = ValueData.Instance.GetSkillBase(itemID[i], itemlevel[i]);
            SetSkillBackgroundColor(newButton.GetComponent<Image>(), itemlevel[i]);
            newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Skill, skill.ID, skill.Name, skill.maxCD, skill.Cost, skill.Damage, skill.Crit, skill.CritDmg, skill.Size, skill.Speed, itemlevel[i], ValueData.Instance.SkillIntro[skill.ID]);
            newButton.GetComponent<TipInfo>().UpdatePrice(skill.Price);
            if (ValueData.Instance.SkillIcon.Length > skill.ID)
            {
                Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = ValueData.Instance.SkillIcon[skill.ID];
                icon.SetNativeSize();
            }
            int lv = itemlevel[i];
            int x = i;
            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeSkill(skill.ID, lv, true, x));
            newButton.AddComponent<UIStoreDrag>().Configure(TipType.Skill, skill.ID, lv, x, true);
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
            SkillStoreUI.SetActive(false);
            BoxUI.SetActive(false);
            SetWeaponSelectionActive(false);
            UpdateWeaponStore(itemID);
            //ChangeWeapon_ID = -1;
            WeaponfieldUI.SetActive(true);
            WeaponbagdUI.SetActive(true);
            ButtontipUI.SetActive(false);
        }
        else
        {
            WeaponStoreUI.SetActive(false);
            SetWeaponSelectionActive(false);
            //ChangeWeapon_ID = -1;
            if(!ValueData.Instance.isUIopen)
            {
                WeaponfieldUI.SetActive(false);
                WeaponbagdUI.SetActive(false);
                ButtontipUI.SetActive(true);
            }
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
                if (weapon.Rarity == RarityType.Normal)
                    newButton.GetComponent<Image>().color = weaponIconRarity_Normal;
                else if(weapon.Rarity == RarityType.Magic)
                    newButton.GetComponent<Image>().color = weaponIconRarity_Magic;
                else if (weapon.Rarity == RarityType.Rare)
                    newButton.GetComponent<Image>().color = weaponIconRarity_Rare;
                else if (weapon.Rarity == RarityType.Unique)
                    newButton.GetComponent<Image>().color = weaponIconRarity_Unique;
            }
            int x = i;
            newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeWeapon(weapon.ID, true,  x));
            newButton.AddComponent<UIStoreDrag>().Configure(TipType.Weapon, weapon.ID, 0, x, true);
        }
    }
    public void UpdateBox(List<int> itemID, List<int> itemlevel = null)
    {
        if (itemID.Count <= 0)
            return;
        //清除舊內容
        foreach (Transform child in BoxContent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < itemID.Count; i++)
        {
            if (itemlevel == null) // 物品類型為武器
            {
                GameObject newButton = Instantiate(StoreweaponbuttonPrefab, BoxContent);
                WeaponBase weapon = ValueData.Instance.Weapon[itemID[i]];
                newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, weapon.ID, weapon.Name, weapon.Cooldown, weapon.Cost, weapon.Damage, weapon.Crit, weapon.CritDmg, weapon.Size, weapon.Speed, 0, ValueData.Instance.WeaponIntro[weapon.ID]);
                newButton.GetComponent<TipInfo>().UpdatePrice(0);
                if (ValueData.Instance.WeaponIcon.Length > weapon.ID)
                {
                    Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                    icon.sprite = ValueData.Instance.WeaponIcon[weapon.ID];
                    icon.SetNativeSize();
                    if (weapon.Rarity == RarityType.Normal)
                        newButton.GetComponent<Image>().color = weaponIconRarity_Normal;
                    else if (weapon.Rarity == RarityType.Magic)
                        newButton.GetComponent<Image>().color = weaponIconRarity_Magic;
                    else if (weapon.Rarity == RarityType.Rare)
                        newButton.GetComponent<Image>().color = weaponIconRarity_Rare;
                    else if (weapon.Rarity == RarityType.Unique)
                        newButton.GetComponent<Image>().color = weaponIconRarity_Unique;
                }
                int x = i;
                newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeWeapon(weapon.ID, false, x));
                newButton.AddComponent<UIStoreDrag>().Configure(TipType.Weapon, weapon.ID, 0, x, false);
            }
            else  // 物品類型為技能
            {
                GameObject newButton = Instantiate(StoreskillbuttonPrefab, BoxContent);
                SkillBase skill = ValueData.Instance.GetSkillBase(itemID[i], itemlevel[i]);
                SetSkillBackgroundColor(newButton.GetComponent<Image>(), itemlevel[i]);
                newButton.GetComponent<TipInfo>().UpdateInfo(TipType.Skill, skill.ID, skill.Name, skill.maxCD, skill.Cost, skill.Damage, skill.Crit, skill.CritDmg, skill.Size, skill.Speed, itemlevel[i], ValueData.Instance.SkillIntro[skill.ID]);
                newButton.GetComponent<TipInfo>().UpdatePrice(0);
                if (ValueData.Instance.SkillIcon.Length > skill.ID)
                {
                    Image icon = newButton.transform.Find("Icon").GetComponent<Image>();
                    icon.sprite = ValueData.Instance.SkillIcon[skill.ID];
                    icon.SetNativeSize();
                }
                int lv = itemlevel[i];
                int x = i;
                newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeSkill(skill.ID, lv, false, x));
                newButton.AddComponent<UIStoreDrag>().Configure(TipType.Skill, skill.ID, lv, x, false);
            }

        }
    }

    //滑鼠射線
    private bool ShouldRefreshTip(TipInfo info)
    {
        return !Tip.activeSelf || displayedTip != info || displayedTipRevision != info.Revision ||
            displayedTipLanguage != LocalizationManager.CurrentLanguage;
    }

    private bool TryGetHoveredTip(out TipInfo info)
    {
        info = null;
        if (!IsPointerOverUI(out GameObject hit)) return false;
        // Drop overlays and field backgrounds must resolve to the destination skill too.
        if (mergePreviewSkillId > 0 && TryGetPurchaseDropField(hit, TipType.Skill, out int field))
        {
            info = SkillFieldIcon[field].GetComponent<TipInfo>();
            return info != null && info.CompareTag("UI");
        }
        // Price labels and other child graphics belong to the same item tooltip.
        info = hit.GetComponentInParent<TipInfo>();
        return info != null && info.CompareTag("UI") &&
            (info.Type == TipType.Skill || info.Type == TipType.Weapon || info.Type == TipType.Passiveskill);
    }

    private int mergePreviewSkillId;
    private int mergePreviewLevel;
    private int mergePreviewSourceField = -1;

    public static bool CanMergeSkills(int sourceId, int sourceLevel, int targetId, int targetLevel)
    {
        return sourceId > 0 && sourceId == targetId && sourceLevel == targetLevel &&
            sourceLevel >= 1 && sourceLevel < SkillLevelTable.MaxLevel;
    }

    public void BeginSkillMergePreview(int skillId, int level, int sourceField = -1)
    {
        mergePreviewSkillId = skillId;
        mergePreviewLevel = level;
        mergePreviewSourceField = sourceField;
        displayedTip = null;
    }

    public void EndSkillMergePreview()
    {
        if (mergePreviewSkillId == 0) return;
        mergePreviewSkillId = 0;
        mergePreviewSourceField = -1;
        displayedTip = null;
    }

    private bool TryGetSkillMergePreview(TipInfo info, out SkillBase current, out SkillBase upgraded)
    {
        current = upgraded = null;
        if (mergePreviewSkillId <= 0 || info.Type != TipType.Skill) return false;
        var data = ValueData.Instance;
        if (mergePreviewSourceField >= 0 &&
            (data.SkillField[mergePreviewSourceField].ID != mergePreviewSkillId ||
             data.SkillField[mergePreviewSourceField].Level != mergePreviewLevel)) return false;
        for (int i = 0; i < SkillFieldIcon.Length; i++)
        {
            if (i == mergePreviewSourceField || SkillFieldIcon[i].GetComponent<TipInfo>() != info ||
                !IsPurchaseDestinationAvailable(TipType.Skill, i)) continue;
            var field = data.SkillField[i];
            if (!CanMergeSkills(mergePreviewSkillId, mergePreviewLevel, field.ID, field.Level)) return false;
            current = data.GetSkillFieldPreview(i, field.Level);
            upgraded = data.GetSkillFieldPreview(i, field.Level + 1);
            return true;
        }
        return false;
    }

    private void ShowSkillMergeValues(SkillBase current, SkillBase upgraded)
    {
        ShowSkillMergeValue(Tip_Cd, current.maxCD, upgraded.maxCD, "0.00", " s");
        ShowSkillMergeValue(Tip_Cost, current.Cost, upgraded.Cost, "0.0");
        ShowSkillMergeValue(Tip_Dmg, current.Damage, upgraded.Damage, "0");
        ShowSkillMergeValue(Tip_Size, current.Size, upgraded.Size, "0.0");
        ShowSkillMergeValue(Tip_Speed, current.Speed, upgraded.Speed, "0.0");
        ShowSkillMergeValue(Tip_Crit, current.Crit * 100f, upgraded.Crit * 100f, "0", " %");
        ShowSkillMergeValue(Tip_CritDmg, current.CritDmg * 100f, upgraded.CritDmg * 100f, "0", " %");
    }

    private void ShowSkillMergeValue(TextMeshProUGUI valueUI, float current, float upgraded, string format, string unit = "")
    {
        valueUI.text = upgraded.ToString(format) + unit;
        valueUI.color = Mathf.Approximately(current, upgraded) ? valueColor_normal : skillMergePreviewColor;
    }

    private string FormatSkillMergeNumber(float value, float? previous, string format)
    {
        string text = value.ToString(format);
        return previous.HasValue && !Mathf.Approximately(previous.Value, value)
            ? "<color=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(skillMergePreviewColor) + ">" + text + "</color>"
            : text;
    }

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
        Vector3 _lerp = Vector3.Lerp(_position, Camera.main.transform.position , 0.1f);
        TextMeshPro damagetext = Instantiate(DamagetextPrefab, _lerp, Camera.main.transform.rotation, DamagetextParent.transform);
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

    private void RefreshStoreAffordability(Transform content)
    {
        if (content == null) return;
        foreach (UIStoreDrag item in content.GetComponentsInChildren<UIStoreDrag>(true))
            item.RefreshAffordability();
    }

    public void UpdateMoneyUI() {
        MoneyValue.text = ValueData.Instance.money.ToString();
        RefreshStoreAffordability(SkillStoreContent);
        RefreshStoreAffordability(WeaponStoreContent);
    }

    public void resetlevel()
    {
        LevelCtrl.Instance.NextLevel(LevelCtrl.Instance.CurrentLevelIndex);
        settingUI.SetActive(false);
        Time.timeScale = 1;
    }

    public void randomweaponstoreEvent()
    {
        if(ValueData.Instance.money < 5)
            return;
        if (!nowWeaponstore)
            return;
        if (!nowWeaponstore.startRandom || nowWeaponstore.npcType != NpcType.Weaponstore || !WeaponStoreUI.activeInHierarchy)
            return;
        if (!nowWeaponstore.RandomItem()) return;
        ValueData.Instance.GetMoney(-5);
        nowWeaponstore.doNpc(true);
    }
    public void randomskillstoreEvent()
    {
        if (ValueData.Instance.money < 5)
            return;
        if (!nowSkillstore)
            return;
        if (!nowSkillstore.startRandom || nowSkillstore.npcType != NpcType.Skillstore || !SkillStoreUI.activeInHierarchy)
            return;
        if (!nowSkillstore.RandomItem()) return;
        ValueData.Instance.GetMoney(-5);
        nowSkillstore.doNpc(true);
    }
    public void Newplayer()
    {
        Time.timeScale = 1;
        LevelCtrl.Instance.ResetRun();
        Skill.Instance.ResetRun();
        PlayerCtrl.Instance.ResetRun();
        ResetRunUI();
        ValueData.Instance.ResetRun();
        ResetLoadout();
        UpdateMoneyUI();
        UpdatePassiveSkill();
        LevelCtrl.Instance.NextLevel(0);
    }

    private void ResetRunUI()
    {
        nowSkillstore = null;
        nowWeaponstore = null;
        nowSelectitem = 0;
        ChangeSkill_ID[0] = -1;
        ChangeSkill_ID[1] = 0;
        ChangeWeapon_ID = -1;
        isSpendmoney = false;
        UpgradeBtn = new int[3];
        foreach (GameObject tree in passivetree)
            tree.SetActive(false);
        settingUI.SetActive(false);
        GameOverUI.SetActive(false);
        Upgrade.SetActive(false);
        SkillStoreUI.SetActive(false);
        WeaponStoreUI.SetActive(false);
        BoxUI.SetActive(false);
        SkillFieldSelectUI.SetActive(false);
        SetWeaponSelectionActive(false);
        PassiveskilltreeUI.SetActive(false);
        ValueUI.SetActive(false);
        WeaponfieldUI.SetActive(false);
        WeaponbagdUI.SetActive(false);
        passivetreeSelectUI.SetActive(false);
        Tip.SetActive(false);
        GarbageCan.SetActive(false);
        ButtontipUI.SetActive(true);
        if (vignette != null)
            vignette.color.value = vignetteColor;
    }

    private void ResetLoadout()
    {
        for (int i = 0; i < ValueData.Instance.WeaponField.Length; i++)
        {
            ValueData.Instance.WeaponField[i].ID = 0;
            ChangeWeapon_ID = 0;
            DoChangeWeapon(i);
        }
        for (int i = 0; i < ValueData.Instance.SkillField.Length; i++)
        {
            SkillFieldBase field = ValueData.Instance.SkillField[i];
            field.ID = 0;
            field.Level = 0;
            field.nowCD = 0;
            ChangeSkill_ID[0] = 0;
            ChangeSkill_ID[1] = 0;
            DoChangeSkill(i);
        }
        ValueData.Instance.GetAp(0);
    }



    // 開啟地上的裝備寶箱時
    public void showBox(bool Switch, List<int> itemID = null, List<int> itemLv = null)
    {
        if (Switch)
        {
            BoxUI.SetActive(true);
            WeaponStoreUI.SetActive(false);
            SkillStoreUI.SetActive(false);
            //SetWeaponSelectionActive(false);
            UpdateBox(itemID, itemLv);
            WeaponfieldUI.SetActive(true);
            WeaponbagdUI.SetActive(true);
            ButtontipUI.SetActive(false);
        }
        else
        {
            BoxUI.SetActive(false);
            SetWeaponSelectionActive(false);
            SkillFieldSelectUI.SetActive(false);
            if (!ValueData.Instance.isUIopen)
            {
                WeaponfieldUI.SetActive(false);
                WeaponbagdUI.SetActive(false);
                ButtontipUI.SetActive(true);
            }
        }

    }

    // 語系選擇
    public void SelectLenguage()
    {
        int index = LanguageDropdown.value;
        string selectedOption = LanguageDropdown.options[index].text;
        string targetLanguage;

        if (selectedOption == "繁體中文")
            targetLanguage = "Chinese (Taiwan)";
        else if (selectedOption == "English")
            targetLanguage = "English";
        else if (selectedOption == "简体中文")
            targetLanguage = "Chinese (Simplified)";
        else
            return;
        
        if (LocalizationManager.HasLanguage(targetLanguage))
            LocalizationManager.CurrentLanguage = targetLanguage;

        // 據點名稱UI
        if(LevelCtrl.Instance.nowLevel == 0)
        {
            LocalizationManager.TryGetTranslation("UI/Home", out string classname);
            UICtrl.Instance.classUI.text = classname;
        }
        
    }

    // 語系下拉式選單初始化
    private void StartLanguageDropdown()
    {
        if (LocalizationManager.CurrentLanguage == "English")
            LanguageDropdown.value = 0;
        else if (LocalizationManager.CurrentLanguage == "Chinese (Taiwan)")
            LanguageDropdown.value = 1;
        else if (LocalizationManager.CurrentLanguage == "Chinese (Simplified)")
            LanguageDropdown.value = 2;
    }

    // 語系選擇
    public void SelectClass()
    {
        int index = ClassDropdown.value;
        if (index <= 0 || index >= stageDropdownIndices.Count || LevelCtrl.Instance == null) return;
        int _class = stageDropdownIndices[index];

        settingUI.SetActive(false);
        Time.timeScale = 1;
        LevelCtrl.Instance.NextLevel(_class);
    }

    public void callPassivetreeSelect()
    {
        TryCallPassivetreeSelect();
    }

    public const int HomePassiveTreeId = 5;
    private readonly int[] passiveTreeOptions = { -1, -1, -1 };
    private readonly bool[] passiveTreeRerollUsed = new bool[3];
    private int fixedPassiveTreeOfferId = -1;

    public bool CanOfferPassiveTree(int treeId)
    {
        return treeId >= 0 && treeId < ValueData.Instance.PassiveTree.Length &&
            treeId < passivetreePrefab.Length && treeId < passivetree.Length &&
            !ValueData.Instance.PassiveTree[treeId] && passivetreePrefab[treeId] != null &&
            passivetree[treeId] != null;
    }

    public bool TryCallPassivetreeSelect()
    {
        return TryCallPassivetreeSelect(-1);
    }

    public bool TryCallPassivetreeSelect(int fixedTreeId)
    {
        if (passivetreeSelectUI.activeSelf)
            return false;
        // 將未解鎖的天賦樹放進treepool
        List<int> treepool = new List<int>();
        for(int i = 0 ; i< ValueData.Instance.PassiveTree.Length ; i++)
        {
            if (CanOfferPassiveTree(i) && (fixedTreeId >= 0 ? i == fixedTreeId : i != HomePassiveTreeId))
            {
                treepool.Add(i);
            }
        }
        if(treepool.Count == 0)
        {
            Debug.Log("沒有能解鎖的天賦樹了");
            return false;
        }
        
        Time.timeScale = 0;
        passivetreeSelectUI.SetActive(true);
        fixedPassiveTreeOfferId = fixedTreeId;
        for (int n = 0 ; n < 3 ; n ++)
        {
            passiveTreeOptions[n] = -1;
            passiveTreeRerollUsed[n] = false;
            UnityEngine.UI.Button refreshButton = GetPassiveTreeRefreshButton(n);
            if (refreshButton != null)
            {
                int optionIndex = n;
                refreshButton.onClick.RemoveAllListeners();
                refreshButton.onClick.AddListener(() => RerollPassiveTreeOption(optionIndex));
            }
        }

        // 生成選項
        for(int x =0; x < 3 ; x ++)
        {
            if(treepool.Count > 0 && (fixedTreeId < 0 || x == 1))
            {
                int rng = UnityEngine.Random.Range(0, treepool.Count);
                SetPassiveTreeOption(x, treepool[rng]);
                treepool.RemoveAt(rng);
            }
            else
                SetPassiveTreeOption(x, -1);
        }
        UpdatePassiveTreeRefreshButtons();
        return true;
    }

    private void SetPassiveTreeOption(int optionIndex, int treeId)
    {
        Transform option = passivetreeSelectUI.transform.Find("Button " + optionIndex);
        // Refresh controls are siblings, so only tree previews are cleared here.
        foreach (Transform child in option)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        passiveTreeOptions[optionIndex] = treeId;
        UnityEngine.UI.Button button = option.GetComponent<UnityEngine.UI.Button>();
        button.onClick.RemoveAllListeners();
        option.gameObject.SetActive(treeId >= 0);
        if (treeId < 0)
            return;

        button.onClick.AddListener(() => selectPassivetree(treeId));
        GameObject tree = Instantiate(passivetreePrefab[treeId], option);
        tree.transform.localPosition = Vector3.zero;
    }

    private UnityEngine.UI.Button GetPassiveTreeRefreshButton(int optionIndex)
    {
        return passiveTreeRefreshButtons != null && optionIndex < passiveTreeRefreshButtons.Length
            ? passiveTreeRefreshButtons[optionIndex] : null;
    }

    private List<int> GetPassiveTreeRerollPool()
    {
        List<int> pool = new List<int>();
        if (fixedPassiveTreeOfferId >= 0)
            return pool;

        for (int treeId = 0; treeId < ValueData.Instance.PassiveTree.Length; treeId++)
        {
            if (treeId != HomePassiveTreeId && CanOfferPassiveTree(treeId)
                && Array.IndexOf(passiveTreeOptions, treeId) < 0)
                pool.Add(treeId);
        }
        return pool;
    }

    public void RerollPassiveTreeOption(int optionIndex)
    {
        if (!passivetreeSelectUI.activeSelf || optionIndex < 0 || optionIndex >= 3
            || passiveTreeOptions[optionIndex] < 0 || passiveTreeRerollUsed[optionIndex])
            return;

        List<int> pool = GetPassiveTreeRerollPool();
        if (pool.Count == 0)
        {
            UpdatePassiveTreeRefreshButtons();
            return;
        }

        passiveTreeRerollUsed[optionIndex] = true;
        SetPassiveTreeOption(optionIndex, pool[UnityEngine.Random.Range(0, pool.Count)]);
        UpdatePassiveTreeRefreshButtons();
    }

    private void UpdatePassiveTreeRefreshButtons()
    {
        bool hasReplacement = GetPassiveTreeRerollPool().Count > 0;
        for (int i = 0; i < 3; i++)
        {
            UnityEngine.UI.Button refresh = GetPassiveTreeRefreshButton(i);
            if (refresh != null)
            {
                refresh.gameObject.SetActive(passiveTreeOptions[i] >= 0 && !passiveTreeRerollUsed[i]);
                refresh.interactable =
                    passiveTreeOptions[i] >= 0 && !passiveTreeRerollUsed[i] && hasReplacement;
            }
        }
    }

    private bool focusNewestPassiveTree;

    private void RefreshPassiveTreeScroll()
    {
        // Tree rewards can be selected while the Tab panel is hidden.
        if (!PassiveskilltreeUI.activeInHierarchy)
            return;
        ScrollRect scroll = PassiveskilltreeUI.GetComponent<ScrollRect>();
        if (scroll == null || scroll.content == null)
            return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        if (focusNewestPassiveTree)
        {
            scroll.StopMovement();
            scroll.horizontalNormalizedPosition = 1f;
            focusNewestPassiveTree = false;
        }
    }

    public void selectPassivetree(int treeID)
    {
        if(ValueData.Instance.PassiveTree.Length <= treeID)
        {
            Debug.Log("選項ID超出天賦樹數量");
        }
        else if(ValueData.Instance.PassiveTree[treeID] == true)
        {
            Debug.Log("重複解鎖天賦樹");
        }
        else
        {
            ValueData.Instance.PassiveTree[treeID] = true;
            passivetree[treeID].SetActive(true);
            passivetree[treeID].transform.SetAsLastSibling();
            focusNewestPassiveTree = true;
            RefreshPassiveTreeScroll();
        }
        Time.timeScale = 1;
        passivetreeSelectUI.SetActive(false);
    }

}
