using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ValueData : MonoBehaviour
{
    public GameObject Player;
    private static ValueData instance;
    public bool canBehurt;//可被攻擊，用於受傷無敵幀
    public bool immortal; // 無敵狀態
    public bool isUIopen;//開關 tab UI
    public CinemachineVirtualCamera virtualCamera;//鏡頭
    public GameObject moneyPrefab;
    private Coroutine _restoreAP; // 自然回魔
    private Coroutine _reloadAP; // 天賦28
    private Coroutine _lostRage; // 盛怒自然衰退
    private Coroutine _rageCount; // 盛怒衰退倒數

    //基底數值
    public int money;//身上持有的金幣數量
    public int passiveskillPoint = 0;//天賦點數
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_maxRage = 0;
    public float base_MoveSpeed = 3f;
    public float base_Power = 5;
    public float base_SkillSpeed = 1;
    public float base_EnemyTimer = 1;
    public float base_AttackSize = 1;
    public float base_Cooldown = 0;
    public float base_Cost = 0;
    public float base_Crit = 0;
    public float base_CritDmg = 0;
    public float base_RestoreAP = 1f;//AP每秒自然恢復
    public float base_Damagereduction;//傷害減免
    public float base_Vision;
    public float base_BulletSpeed;

    //天賦數值
    public float add_maxAp;
    public float add_maxHp;
    public float add_maxRage;
    public float add_MoveSpeed;
    public float add_Power;
    public float add_SkillSpeed;
    public float add_EnemyTimer;
    public float add_AttackSize;
    public float add_Cooldown;
    public float add_Cost;
    public float add_Crit;
    public float add_CritDmg;
    public float add_RestoreAP;
    public float add_Damagereduction;
    public float add_Vision;
    public float add_BulletSpeed;
    public float add_RagePower; // 天賦0給予的傷害加乘
    public float add_RageCritdmg; // 天賦6給予的暴傷加乘
    public float add_RageCooldown; // 天賦10給予的冷卻
    public float add_RageMovespeed; // 天賦11給予的跑速
    public float add_ReloadCrit; // 天賦19給予的額外暴率
    public float add_ReloadMovespeed; // 天賦18給予的跑速
    public float add_MaxapRestore; // 天賦22 額外最大魔力的魔力恢復
    public float add_ManaPower; // 天賦23給予的額外傷害
    public float add_ManaCrit; // 天賦31給予的額外暴率

    //局外數值(預留)


    //當前總數值
    public bool[] PassiveSkills; //當前各天賦點的加點
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float Rage; // 盛怒值
    public float maxRage;
    public float EXP;
    public float maxEXP;
    public float Power;//基礎傷害倍率
    public float SkillSpeed;//技能移動速度
    public float MoveSpeed;//移動速度
    public float EnemyTimer;//敵人生成速度%
    public float AttackSize;//技能大小%
    public float Cooldown;//冷卻倍率%
    public float Cost;//魔耗倍率%
    public float Crit;//暴率
    public float CritDmg;//暴傷
    public float RestoreAP;
    public float Damagereduction;//傷害減免%
    public float Vision;//視野範圍(FOV)
    public float RageTime; // 盛怒當前倒數時間

    public Sprite[] SkillIcon;//技能icon
    public Sprite[] WeaponIcon;//武器icon

    //技能總表
    [NonSerialized]
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,0,"-",0,0,0,0,0,0,0f,1),//無
        new SkillBase(1,20,"劈砍",1f,10,1f,1,0,0.15f,2f,1),
        new SkillBase(2,20,"衝刺",2.4f,0,1f,1,0,0,2f,1),//size=位移距離
        new SkillBase(3,0,"音符",1f,10,1f,1,0,0,2f,1),
        new SkillBase(4,20,"閃現",0.5f,0f,1f,1,3f,0f,2f,1),
        new SkillBase(5,40,"新月斬",1.8f,6,0.8f,1,0f,0.1f,2f,1),
        new SkillBase(6,0,"弦月斬",1.8f,12,1.1f,1,0f,0.1f,2f,1),
        new SkillBase(7,0,"明月斬",1.8f,20,1.4f,1,0f,0.1f,2f,1),
        new SkillBase(8,20,"The喪鐘",4f,100f,1f,1,2,0f,2f,1),
        new SkillBase(9,20,"飛箭",0.12f,3,1f,1,0.3f,0f,2f,1),
        new SkillBase(10,40,"水曝",0.5f,0f,1f,1,0f,0f,2f,1),
    };
    //技能介紹
    [NonSerialized]
    public string[] SkillIntro = new string[] {
        "-",
        "對前方半圓形範圍內所有敵人造成傷害",
        "衝刺一段距離，並恢復50%失去的魔力<BR>(速度會影響衝刺距離)",
        "",
        "閃現至滑鼠位置，沒有距離限制",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "會以 新月斬→弦月斬→明月斬 順序輪替",
        "生成一個持續6秒的喪鐘，你對喪鐘造成的傷害會被放大 20% 後，被喪鐘以圓形造成範圍傷害",
        "朝滑鼠方向發射一枚飛彈，命中敵人後消失",
        "在滑鼠位置生成一個水球，一段時間後爆炸，消耗一半當前魔力並造成(消耗量×10)傷害",
    };
    //技能標籤
    [NonSerialized]
    public SkillTagType[][] SkillTag = new SkillTagType[][]
    {
        new SkillTagType[]{ } ,
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能1
        new SkillTagType[]{ SkillTagType.Movement } , //技能2
        new SkillTagType[]{ SkillTagType.Spell } , //技能3
        new SkillTagType[]{ SkillTagType.Movement, SkillTagType.Spell } , //技能4
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能5
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能6
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //技能7
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range } , //技能8
        new SkillTagType[]{ SkillTagType.Projectile, SkillTagType.Physical } , //技能9
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range, SkillTagType.Cold } , //技能10
    };

    //已裝備技能欄位
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//滑鼠L
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//滑鼠R
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//空白鍵
    };

    //裝備總表
    [NonSerialized]
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,RarityType.Normal,0,"空手", 0, 0, 0, 0, 0, 0, 0),//Dmg、CD、Size、Speed、Cost皆是倍率，1f=100%
        new WeaponBase(1,RarityType.Normal,20,"鐵劍", 0.2f, -0.15f, 0, -0.2f, 0, 0, 0),
        new WeaponBase(2,RarityType.Normal,20,"鐵弓", 0, 0, 0 , 0.25f, -0.15f, 0, 0),
        new WeaponBase(3,RarityType.Normal,20,"鐵斧", 0, 0.2f, 0.35f, -0.2f, 0.3f, 0.15f, 0),
        new WeaponBase(4,RarityType.Magic,20,"守財犬", 0, -0.1f, 0, 0, 0, 0, 0),
        new WeaponBase(5,RarityType.Rare,60,"無盡", 0.4f, 0.2f, -0.3f, -0.3f, 1f, 0.2f, 0.5f),
        new WeaponBase(6,RarityType.Rare,60,"風暴", 0.5f, 0, -0.2f, -0.2f, 1f, 0, 0),
        new WeaponBase(7,RarityType.Rare,60,"賽博義肢", 0, -0.1f, -0.5f, 1f, 0.2f, 0, 0),
        new WeaponBase(8,RarityType.Rare,60,"漩渦", -0.2f, 1f, 1f, 0, 0, 0, 0),
        new WeaponBase(9,RarityType.Magic,20,"招財貓", 0, -0.1f, 0, 0, 0, 0, 0),
        new WeaponBase(10,RarityType.Rare,60,"彈弓", 0, 0, -0.2f, 0.2f, -0.1f, 0, 0),
        new WeaponBase(11,RarityType.Rare,60,"分裂", 0, 0, 0, 0.2f, -0.1f, 0, 0),
        new WeaponBase(12,RarityType.Rare,60,"狙擊", 0, 0, -0.2f, 0.4f, 0, 0, 0),
        new WeaponBase(13,RarityType.Rare,60,"暴擊冷卻", 0, -0.1f, 0, 0, 0.25f, 0.2f, 0),
        new WeaponBase(14,RarityType.Rare,60,"虛空妖刃", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(15,RarityType.Normal,20,"布鞋", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(16,RarityType.Normal,20,"魔力鞋", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(17,RarityType.Normal,20,"初新者匕首", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(18,RarityType.Rare,60,"紫晶戒指", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(19,RarityType.Normal,20,"學徒法書", 0, 0, 0, 0, 0, 0, 0),
    };
    //裝備介紹
    [NonSerialized]
    public string[] WeaponIntro = new string[] {
        "-",
        "1",
        "2",
        "3",
        "身上每1金幣提供1%傷害增幅",
        "「大力點，一下搞定」",
        "技能額外重複2次",
        "使用位移技能時觸發L欄位上的非位移技能",
        "冰冷技能命中複數目標時，每個額外目標使傷害提升20%",
        "擊殺敵人掉落的金幣為0~3倍",
        "投射物命中牆壁時會反彈",
        "投射物命中敵人後，額外分裂出兩個",
        "投射物傷害依距離提升",
        "暴擊時降低其他技能冷卻 1 秒",
        "14",
        "15",
        "16",
        "17",
        "18",
        "19",
    };

    //已裝備裝備
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//滑鼠L
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//滑鼠R
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//空白鍵
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
    };

    //技能商店池
    [NonSerialized]
    public List<int> skillstorePool = new List<int>()
    {
        1,2,4,8,9,10
    };
    //裝備商店池
    [NonSerialized]
    public List<int> weaponstorePool = new List<int>()
    {
        1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
    };

    //每次加減天賦時呼叫，更新所有數值
    //還需要遍歷天賦點
    public void PlayerValueUpdate() {
        //重製
        add_maxAp = 0;
        add_maxHp = 0;
        add_maxRage = 0;
        add_MoveSpeed = 0;
        add_Power= 0;
        add_SkillSpeed = 0;
        add_EnemyTimer = 0;
        add_AttackSize = 0;
        add_Cooldown = 0;
        add_Cost = 0;
        add_Crit = 0;
        add_CritDmg = 0;
        add_RestoreAP = 0;
        add_Damagereduction = 0;
        add_Vision = 0;
        add_BulletSpeed = 0;
        //天賦數值
        for (int i = 0; i < PassiveSkills.Length; i++) {
            if (PassiveSkills[i])
                PassiveSkillValueUpdate(i);
        }
        //加總
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        if (PassiveSkills[28]) 
        {
            maxAP *= 1.5f;
            maxHP *= 0.5f;
        }
        maxRage = base_maxRage + add_maxRage;
        if (PassiveSkills[23]) // 天賦23
            add_ManaPower = Mathf.FloorToInt(maxAP) * 0.05f;
        else
            add_ManaPower = 0f;
        Power = base_Power + add_Power + add_RagePower + add_ManaPower;
        MoveSpeed = 1 + add_MoveSpeed + add_RageMovespeed + add_ReloadMovespeed;
        SkillSpeed = base_SkillSpeed + add_SkillSpeed;
        EnemyTimer = base_EnemyTimer + add_EnemyTimer;
        AttackSize = base_AttackSize + add_AttackSize;
        Cooldown = base_Cooldown + add_Cooldown + add_RageCooldown;
        Cost = (base_Cost + add_Cost);
        if (PassiveSkills[31])
            add_ManaCrit = Mathf.FloorToInt(maxAP) * 0.03f;
        Crit = base_Crit + add_Crit + add_ReloadCrit + add_ManaCrit;
        CritDmg = base_CritDmg + add_CritDmg + add_RageCritdmg;
        if (PassiveSkills[22]) // 天賦22
            add_MaxapRestore = maxAP / 10;
        else
            add_MaxapRestore = 0;
        RestoreAP = base_RestoreAP + add_RestoreAP + add_MaxapRestore;
        Damagereduction = base_Damagereduction + add_Damagereduction;
        Vision = base_Vision + add_Vision;
        //更新value UI
        UICtrl.Instance.UpdateValueUI();
        GetRage(0);
        virtualCamera.m_Lens.FieldOfView = Vision;
        //天賦0
        if (!PassiveSkills[0])
        {
            UICtrl.Instance.UI_Rage.SetActive(false);
            if (_rageCount != null)
                StopCoroutine(_rageCount);
        }
        //天賦14
        if (PassiveSkills[14])
        {
            if (_restoreAP != null) 
            {
                StopCoroutine(_restoreAP);
                _restoreAP = null;
            }
        }
        else
        {
            if (_restoreAP == null)
                _restoreAP = StartCoroutine(restoreAP());
        }
        GetHp(0);
        GetAp(0);
    }

    //更換武器、技能時呼叫，呼叫前請確保有更新過PlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * (1 + Cooldown) * (1 + WeaponField[id * 3].Cooldown + WeaponField[id * 3 + 1].Cooldown + WeaponField[id * 3 + 2].Cooldown);
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * (1 + Power) * (1 + WeaponField[id * 3].Damage + WeaponField[id * 3 + 1].Damage + WeaponField[id * 3 + 2].Damage);
            float add_CostSize = 1;
            if (PassiveSkills[32])
                add_CostSize = (1 + Cost) * (1 + WeaponField[id * 3].Cost + WeaponField[id * 3 + 1].Cost + WeaponField[id * 3 + 2].Cost);
            SkillField[id].Size = Skill[SkillField[id].ID].Size * (1 + AttackSize) * (1 + WeaponField[id * 3].Size + WeaponField[id * 3 + 1].Size + WeaponField[id * 3 + 2].Size) * add_CostSize;
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * (1 + SkillSpeed) * (1 + WeaponField[id * 3].Speed + WeaponField[id * 3 + 1].Speed + WeaponField[id * 3 + 2].Speed);
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * (1 + Cost) * (1 + WeaponField[id * 3].Cost + WeaponField[id * 3 + 1].Cost + WeaponField[id * 3 + 2].Cost);
            SkillField[id].Crit = Skill[SkillField[id].ID].Crit + Crit + WeaponField[id * 3].Crit + WeaponField[id * 3 + 1].Crit + WeaponField[id * 3 + 2].Crit;
            SkillField[id].CritDmg = Skill[SkillField[id].ID].CritDmg + CritDmg + WeaponField[id * 3].CritDmg + WeaponField[id * 3 + 1].CritDmg + WeaponField[id * 3 + 2].CritDmg;
            UICtrl.Instance.SkillfieldUI.transform.GetChild(id).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Skill, SkillField[id].ID, SkillField[id].Name, SkillField[id].maxCD, SkillField[id].Cost, SkillField[id].Damage, SkillField[id].Crit, SkillField[id].CritDmg, SkillField[id].Size, SkillField[id].Speed, SkillField[id].Level, SkillIntro[SkillField[id].ID]);
            
            // 關閉該技能所有裝備欄位]
            for (int i = 0; i < 3; i++)
            {
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(id * 3 + i).gameObject.SetActive(false);
            }
            int Lv = SkillField[id].Level;
            if (Lv >= 0 && Lv <= 3) // 依等級開啟該技能裝備欄位，最多3級
            {
                for (int i = 0; i < Lv; i++)
                {
                    UICtrl.Instance.WeaponfieldUI.transform.GetChild(id * 3 + i).gameObject.SetActive(true);
                }
            }
            else
                Debug.Log("技能等級不在預設範圍內");
        }
    }

    //單例實體
    public static ValueData Instance
    {
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
    private void Start()
    {
        _restoreAP = StartCoroutine(restoreAP());
        Reload(false);
    }

    //天賦數值計算
    public void PassiveSkillValueUpdate(int id) { 
        switch (id)
        {
            case 0:
                add_maxRage += 20;
                if (UICtrl.Instance.UI_Rage.activeSelf)
                    return;
                UICtrl.Instance.UI_Rage.SetActive(true);
                if (_rageCount != null)
                    StopCoroutine(_rageCount);
                _rageCount = StartCoroutine(rageCount());
                GetRage(0);
                break;
            case 1:
                add_Cooldown -= 0.08f;
                break;
            case 2:
                add_Cooldown -= 0.08f;
                break;
            case 3:
                add_maxRage -= 15;
                break;
            case 4:
                add_CritDmg += 0.1f;
                break;
            case 5:
                add_CritDmg += 0.1f;
                break;
            case 6:
                
                break;
            case 7:
                add_maxRage += 5;
                break;
            case 8:
                add_maxRage += 5;
                break;
            case 9:
                
                break;
            case 10:
                
                break;
            case 11:

                break;
            case 12:
                add_Vision += 5f;
                add_MoveSpeed += 0.05f;
                break;
            case 13:
                add_Vision += 5f;
                add_MoveSpeed += 0.05f;
                break;
            case 14:
                
                break;
            case 15:
                add_Power += 0.1f;
                break;
            case 16:
                add_Power += 0.1f;
                break;
            case 17:
                add_Power += 0.1f;
                break;
            case 18:
                
                break;
            case 19:
                
                break;
            case 20:
                
                break;
            case 21:

                break;
            case 22:
                
                break;
            case 23:
                add_Cost += 0.5f;
                break;
            case 24:
                add_maxAp += 1;
                break;
            case 25:
                add_maxAp += 1;
                break;
            case 26:
                add_maxAp += 1;
                break;
            case 27:
                
                break;
            case 28:
                
                break;
            case 29:
                add_maxAp += 1;
                break;
            case 30:
                add_maxAp += 1;
                break;
            case 31:
                
                break;
            case 32:
                
                break;
        }
    }

    public void GetMoney(int value) 
    { 
        if(value != 0)
        {
            money += value;
            UICtrl.Instance.UpdateMoneyUI();
        }
        //裝備4的特殊能力
        for(int i=0;i< WeaponField.Length;i++)
        {
            if (WeaponField[i].ID == 4)
            {
                WeaponField[i].Damage = 1 + (money * 0.01f);
                SkillFieldValueUpdate();
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(i).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[i].ID, ValueData.Instance.WeaponField[i].Name, ValueData.Instance.WeaponField[i].Cooldown, ValueData.Instance.WeaponField[i].Cost, ValueData.Instance.WeaponField[i].Damage, ValueData.Instance.WeaponField[i].Crit, ValueData.Instance.WeaponField[i].CritDmg, ValueData.Instance.WeaponField[i].Size, ValueData.Instance.WeaponField[i].Speed, 0, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[i].ID]);
            }
        }
    }

    //降冷卻通用
    public void doCooldown(SkillFieldBase field, float reduce) 
    {
        if (field.nowCD == 0)
            return;
        else if (field.nowCD <= reduce)
            field.nowCD = 0;
        else
            field.nowCD -= reduce;
        UICtrl.Instance.UpdateSkillCD();
    }

    //加減生命通用
    public void GetHp(float value, bool useBehurtTimer = false)
    {
        if (value >= 0) // 回血
        {
            if (HP < maxHP - value)
                HP += value;
            else
                HP = maxHP;
        }
        else // 扣血
        {
            if (!canBehurt || immortal)
                return;
            value *= (1 - Damagereduction);
            /*if (PassiveSkills[10] && !PlayerCtrl.Instance.isReload) // MOM
            {
                if (AP >= -value)
                {
                    GetAp(value);
                    value = 0;
                }
                else
                {
                    value += AP;
                    GetAp(-AP);
                }
            }*/
            if (HP > -value)
            {
                if (value == 0)
                {
                    StartCoroutine(PlayerCtrl.Instance.BehurtTimer(false)); // 不扣血時不會觸發特效(傷害完全被擋掉)，僅觸發無敵幀
                }
                else 
                {
                    HP += value;
                    if (useBehurtTimer == true)
                        StartCoroutine(PlayerCtrl.Instance.BehurtTimer(true)); //扣血及特效
                }
            }
            else
            {
                HP = 0;
                UICtrl.Instance.gameover();
            }
        }
    }
    //加減魔力通用
    public void GetAp(float value)
    {
        if(value >= 0) // 回魔
        {
            if (maxAP > AP + value)
                AP += value;
            else
                AP = maxAP;
        }
        else // 耗魔
        {
            if (AP > -value)
                AP += value;
            else
                AP = 0;

            if(PassiveSkills[14]) // 天賦14
            {
                if(AP < 0.1f)
                    Reload(true);
            }
        }
    }

    //自動回魔
    IEnumerator restoreAP()
    {
        while(true)
        {
            float value = RestoreAP / 50;
            if (AP >= maxAP - value)
            {
                AP = maxAP;
            }
            else
            {
                AP += value;
            }
            yield return new WaitForSeconds(0.02f);
        }
    }

    // 開關reload協程
    public void Reload(bool _bool)
    {
        if (_bool)
        {
            if (_reloadAP == null)
            {
                _reloadAP = StartCoroutine(reloadAP());
            }
        }
        else
            _reloadAP = null;
    }
    //天賦14
    IEnumerator reloadAP()
    {
        PlayerCtrl.Instance.canAttack = false;
        PlayerCtrl.Instance.isReload = true;
        if (PassiveSkills[18])
        {
            add_ReloadMovespeed = 0.2f;
            PlayerValueUpdate();
        }
        
        while (AP < maxAP)
        {
            float value;
            if (PassiveSkills[20]) // 天賦20
                value = RestoreAP / 50 * 4;
            else
                value = RestoreAP / 50 * 2.5f;
            if (AP >= maxAP - value)
            {
                AP = maxAP;
            }
            else
            {
                AP += value;
            }
            yield return new WaitForSeconds(0.02f);
        }
        PlayerCtrl.Instance.canAttack = true;
        PlayerCtrl.Instance.isReload = false;
        if (PassiveSkills[19])
            add_ReloadCrit = 1f;
        if (PassiveSkills[18])
            add_ReloadMovespeed = 0;
        PlayerValueUpdate();
        SkillFieldValueUpdate();
        Reload(false);
    }

    //加減盛怒通用
    public void GetRage(float value)
    {
        if (value > 0) // 加
        {
            if (maxRage > Rage + value)
                Rage += value;
            else
                Rage = maxRage;

            RageTime = 3; // 重製盛怒衰退倒數
            if(_lostRage != null)
            {
                StopCoroutine(_lostRage);
                _lostRage = null;
            }
        }
        else if(value < 0) // 減
        {
            if (Rage > -value)
                Rage += value;
            else
                Rage = 0;
        }
        float valueRage = Rage / maxRage;
        UICtrl.Instance.Value_Rage.fillAmount = valueRage;
        UICtrl.Instance.maxrageUI.text = maxRage.ToString("0");
        UICtrl.Instance.nowrageUI.text = Rage.ToString("0");

        float mRage = Mathf.Floor(Rage);
        add_RagePower = mRage * 0.05f;
        if (PassiveSkills[6])
            add_RageCritdmg = mRage * 0.04f;
        if (PassiveSkills[10])
            add_RageCooldown = mRage * -0.02f;
        if(PassiveSkills[11])
            add_RageMovespeed = mRage * 0.03f;
        if (value != 0) // 避免死迴圈
        {
            PlayerValueUpdate();
            SkillFieldValueUpdate();
        }
        
    }
    //盛怒倒數
    IEnumerator rageCount()
    { 
        while(true)
        {
            if (RageTime > 0)
            {
                RageTime -= 0.02f;
                yield return new WaitForSeconds(0.02f);
            }
            else
            {
                //RageTime = 0;
                if (_lostRage == null && Rage > 0)
                {
                    _lostRage = StartCoroutine(lostRage());
                }
                    
                yield return new WaitForFixedUpdate();
            }
        }
        
    }
    //盛怒衰退
    IEnumerator lostRage()
    {
        float value = 0.02f;
        while (true)
        {
            if (Rage > 0)
            {
                GetRage(-value*5);
                yield return new WaitForSeconds(value);
            }
            else
            {
                _lostRage = null;
                yield break;
            }
        }
    }

    // 判斷該技能欄位是否擁有指定裝備
    public bool isHaveweaponid(int field, int id)
    {
        if(WeaponField[field * 3].ID == id || WeaponField[field * 3 + 1].ID == id || WeaponField[field * 3 + 2].ID == id)
            return true;
        else
            return false;
    }

}

//稀有度架構
public enum RarityType
{
    Normal,
    Magic,
    Rare,
    Unique
}

//技能標籤
public enum SkillTagType
{
    Attack,//攻擊
    Spell,//法術
    Movement,//位移
    Range,//範圍
    Projectile,//投射物
    Physical,//物理
    Fire,//火
    Cold,//冰
    Lightning,//電
    Chaos,//混沌

}

//技能架構
public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }    // 名稱
    public float maxCD { get; set; }    // 冷卻時間
    public float Damage { get; set; }   // 傷害
    public float Size { get; set; }     // 技能範圍大小
    public float Speed { get; set; }    // 技能飛行速度
    public float Cost { get; set; }     // 魔耗
    public float Crit { get; set; }     // 暴擊率
    public float CritDmg { get; set; }
    public int Price { get; set; }      // 價格
    public int Level { get; set; }

    public SkillBase(int id, int price,string name, float maxcd, float damage, float size, float speed, float cost, float crit, float critdmg, int level)
    {
        ID = id;
        Name = name;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        CritDmg = critdmg;
        Price = price;
        Level = level;
    }
}

//技能欄位架構
public class SkillFieldBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float nowCD { get; set; }
    public float maxCD { get; set; }
    public float Damage { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Cost { get; set; }
    public float Crit { get; set; }
    public float CritDmg { get; set; }
    public int Level { get; set; }

    public SkillFieldBase(int id, string name, float nowcd, float maxcd, float damage, float size, float speed, float cost, float crit, float critdmg, int level)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        CritDmg = critdmg;
        Level = level;
    }
}

//武器架構
public class WeaponBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public float Cooldown { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Cost { get; set; }
    public float Crit { get; set; }
    public float CritDmg { get; set; }
    public int Price { get; set; }
    public RarityType Rarity { get; set; }

    public WeaponBase(int id, RarityType rarity, int price, string name, float damage, float cooldown, float size, float speed, float cost, float crit, float critdmg)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        CritDmg = critdmg;
        Price = price;
        Rarity = rarity;
    }
}

//武器欄位架構
public class WeaponFieldBase
{
    public int ID { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public float Cooldown { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Cost { get; set; }
    public float Crit { get; set; }
    public float CritDmg { get; set; }

    public WeaponFieldBase(int id, string name, float damage, float cooldown, float size, float speed, float cost, float crit, float critdmg)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        CritDmg = critdmg;
    }
}
