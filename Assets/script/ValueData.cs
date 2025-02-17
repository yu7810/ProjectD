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
    public bool canBehurt;//�i�Q�����A�Ω���˵L�ĴV
    public bool immortal; // �L�Ī��A
    public bool isUIopen;//�}�� tab UI
    public CinemachineVirtualCamera virtualCamera;//���Y
    public GameObject moneyPrefab;
    private Coroutine _restoreAP; // �۵M�^�]
    private Coroutine _reloadAP; // �ѽ�28
    private Coroutine _lostRage; // ����۵M�I�h
    private Coroutine _rageCount; // ����I�h�˼�

    //�򩳼ƭ�
    public int money;//���W�����������ƶq
    public int passiveskillPoint = 0;//�ѽ��I��
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
    public float base_RestoreAP = 1f;//AP�C��۵M��_
    public float base_Damagereduction;//�ˮ`��K
    public float base_Vision;
    public float base_BulletSpeed;

    //�ѽ�ƭ�
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
    public float add_RagePower; // �ѽ�0�������ˮ`�[��
    public float add_RageCritdmg; // �ѽ�6�������ɶ˥[��
    public float add_RageCooldown; // �ѽ�10�������N�o
    public float add_RageMovespeed; // �ѽ�11�������]�t
    public float add_ReloadCrit; // �ѽ�19�������B�~�ɲv
    public float add_ReloadMovespeed; // �ѽ�18�������]�t
    public float add_MaxapRestore; // �ѽ�22 �B�~�̤j�]�O���]�O��_
    public float add_ManaPower; // �ѽ�23�������B�~�ˮ`
    public float add_ManaCrit; // �ѽ�31�������B�~�ɲv

    //���~�ƭ�(�w�d)


    //��e�`�ƭ�
    public bool[] PassiveSkills; //��e�U�ѽ��I���[�I
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float Rage; // �����
    public float maxRage;
    public float EXP;
    public float maxEXP;
    public float Power;//��¦�ˮ`���v
    public float SkillSpeed;//�ޯಾ�ʳt��
    public float MoveSpeed;//���ʳt��
    public float EnemyTimer;//�ĤH�ͦ��t��%
    public float AttackSize;//�ޯ�j�p%
    public float Cooldown;//�N�o���v%
    public float Cost;//�]�ӭ��v%
    public float Crit;//�ɲv
    public float CritDmg;//�ɶ�
    public float RestoreAP;
    public float Damagereduction;//�ˮ`��K%
    public float Vision;//�����d��(FOV)
    public float RageTime; // �����e�˼Ʈɶ�

    public Sprite[] SkillIcon;//�ޯ�icon
    public Sprite[] WeaponIcon;//�Z��icon

    //�ޯ��`��
    [NonSerialized]
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,0,"-",0,0,0,0,0,0,0f,1),//�L
        new SkillBase(1,20,"�A��",1f,10,1f,1,0,0.15f,2f,1),
        new SkillBase(2,20,"�Ĩ�",2.4f,0,1f,1,0,0,2f,1),//size=�첾�Z��
        new SkillBase(3,0,"����",1f,10,1f,1,0,0,2f,1),
        new SkillBase(4,20,"�{�{",0.5f,0f,1f,1,3f,0f,2f,1),
        new SkillBase(5,40,"�s���",1.8f,6,0.8f,1,0f,0.1f,2f,1),
        new SkillBase(6,0,"�����",1.8f,12,1.1f,1,0f,0.1f,2f,1),
        new SkillBase(7,0,"�����",1.8f,20,1.4f,1,0f,0.1f,2f,1),
        new SkillBase(8,20,"The����",4f,100f,1f,1,2,0f,2f,1),
        new SkillBase(9,20,"���b",0.12f,3,1f,1,0.3f,0f,2f,1),
        new SkillBase(10,40,"���n",0.5f,0f,1f,1,0f,0f,2f,1),
    };
    //�ޯश��
    [NonSerialized]
    public string[] SkillIntro = new string[] {
        "-",
        "��e��b��νd�򤺩Ҧ��ĤH�y���ˮ`",
        "�Ĩ�@�q�Z���A�ë�_50%���h���]�O<BR>(�t�׷|�v�T�Ĩ�Z��)",
        "",
        "�{�{�ܷƹ���m�A�S���Z������",
        "�|�H �s��١�����١������ ���ǽ���",
        "�|�H �s��١�����١������ ���ǽ���",
        "�|�H �s��١�����١������ ���ǽ���",
        "�ͦ��@�ӫ���6�������A�A������y�����ˮ`�|�Q��j 20% ��A�Q�����H��γy���d��ˮ`",
        "�·ƹ���V�o�g�@�T���u�A�R���ĤH�����",
        "�b�ƹ���m�ͦ��@�Ӥ��y�A�@�q�ɶ����z���A���Ӥ@�b��e�]�O�óy��(���Ӷq��10)�ˮ`",
    };
    //�ޯ����
    [NonSerialized]
    public SkillTagType[][] SkillTag = new SkillTagType[][]
    {
        new SkillTagType[]{ } ,
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //�ޯ�1
        new SkillTagType[]{ SkillTagType.Movement } , //�ޯ�2
        new SkillTagType[]{ SkillTagType.Spell } , //�ޯ�3
        new SkillTagType[]{ SkillTagType.Movement, SkillTagType.Spell } , //�ޯ�4
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //�ޯ�5
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //�ޯ�6
        new SkillTagType[]{ SkillTagType.Attack, SkillTagType.Physical, SkillTagType.Range } , //�ޯ�7
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range } , //�ޯ�8
        new SkillTagType[]{ SkillTagType.Projectile, SkillTagType.Physical } , //�ޯ�9
        new SkillTagType[]{ SkillTagType.Spell, SkillTagType.Range, SkillTagType.Cold } , //�ޯ�10
    };

    //�w�˳Ƨޯ����
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//�ƹ�L
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//�ƹ�R
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0,0,1),//�ť���
    };

    //�˳��`��
    [NonSerialized]
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,RarityType.Normal,0,"�Ť�", 0, 0, 0, 0, 0, 0, 0),//Dmg�BCD�BSize�BSpeed�BCost�ҬO���v�A1f=100%
        new WeaponBase(1,RarityType.Normal,20,"�K�C", 0.2f, -0.15f, 0, -0.2f, 0, 0, 0),
        new WeaponBase(2,RarityType.Normal,20,"�K�}", 0, 0, 0 , 0.25f, -0.15f, 0, 0),
        new WeaponBase(3,RarityType.Normal,20,"�K��", 0, 0.2f, 0.35f, -0.2f, 0.3f, 0.15f, 0),
        new WeaponBase(4,RarityType.Magic,20,"�u�]��", 0, -0.1f, 0, 0, 0, 0, 0),
        new WeaponBase(5,RarityType.Rare,60,"�L��", 0.4f, 0.2f, -0.3f, -0.3f, 1f, 0.2f, 0.5f),
        new WeaponBase(6,RarityType.Rare,60,"����", 0.5f, 0, -0.2f, -0.2f, 1f, 0, 0),
        new WeaponBase(7,RarityType.Rare,60,"�ɳոq��", 0, -0.1f, -0.5f, 1f, 0.2f, 0, 0),
        new WeaponBase(8,RarityType.Rare,60,"�x��", -0.2f, 1f, 1f, 0, 0, 0, 0),
        new WeaponBase(9,RarityType.Magic,20,"�۰]��", 0, -0.1f, 0, 0, 0, 0, 0),
        new WeaponBase(10,RarityType.Rare,60,"�u�}", 0, 0, -0.2f, 0.2f, -0.1f, 0, 0),
        new WeaponBase(11,RarityType.Rare,60,"����", 0, 0, 0, 0.2f, -0.1f, 0, 0),
        new WeaponBase(12,RarityType.Rare,60,"����", 0, 0, -0.2f, 0.4f, 0, 0, 0),
        new WeaponBase(13,RarityType.Rare,60,"�����N�o", 0, -0.1f, 0, 0, 0.25f, 0.2f, 0),
        new WeaponBase(14,RarityType.Rare,60,"��ŧ��b", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(15,RarityType.Normal,20,"���c", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(16,RarityType.Normal,20,"�]�O�c", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(17,RarityType.Normal,20,"��s�̤P��", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(18,RarityType.Rare,60,"�����٫�", 0, 0, 0, 0, 0, 0, 0),
        new WeaponBase(19,RarityType.Normal,20,"�Ǯ{�k��", 0, 0, 0, 0, 0, 0, 0),
    };
    //�˳Ƥ���
    [NonSerialized]
    public string[] WeaponIntro = new string[] {
        "-",
        "1",
        "2",
        "3",
        "���W�C1��������1%�ˮ`�W�T",
        "�u�j�O�I�A�@�U�d�w�v",
        "�ޯ��B�~����2��",
        "�ϥΦ첾�ޯ��Ĳ�oL���W���D�첾�ޯ�",
        "�B�N�ޯ�R���ƼƥؼЮɡA�C���B�~�ؼШ϶ˮ`����20%",
        "�����ĤH������������0~3��",
        "��g���R������ɷ|�ϼu",
        "��g���R���ĤH��A�B�~�����X���",
        "��g���ˮ`�̶Z������",
        "�����ɭ��C��L�ޯ�N�o 1 ��",
        "14",
        "15",
        "16",
        "17",
        "18",
        "19",
    };

    //�w�˳Ƹ˳�
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//�ƹ�L
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//�ƹ�R
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),//�ť���
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0,0),
    };

    //�ޯ�ө���
    [NonSerialized]
    public List<int> skillstorePool = new List<int>()
    {
        1,2,4,8,9,10
    };
    //�˳ưө���
    [NonSerialized]
    public List<int> weaponstorePool = new List<int>()
    {
        1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
    };

    //�C���[��ѽ�ɩI�s�A��s�Ҧ��ƭ�
    //�ٻݭn�M���ѽ��I
    public void PlayerValueUpdate() {
        //���s
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
        //�ѽ�ƭ�
        for (int i = 0; i < PassiveSkills.Length; i++) {
            if (PassiveSkills[i])
                PassiveSkillValueUpdate(i);
        }
        //�[�`
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        if (PassiveSkills[28]) 
        {
            maxAP *= 1.5f;
            maxHP *= 0.5f;
        }
        maxRage = base_maxRage + add_maxRage;
        if (PassiveSkills[23]) // �ѽ�23
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
        if (PassiveSkills[22]) // �ѽ�22
            add_MaxapRestore = maxAP / 10;
        else
            add_MaxapRestore = 0;
        RestoreAP = base_RestoreAP + add_RestoreAP + add_MaxapRestore;
        Damagereduction = base_Damagereduction + add_Damagereduction;
        Vision = base_Vision + add_Vision;
        //��svalue UI
        UICtrl.Instance.UpdateValueUI();
        GetRage(0);
        virtualCamera.m_Lens.FieldOfView = Vision;
        //�ѽ�0
        if (!PassiveSkills[0])
        {
            UICtrl.Instance.UI_Rage.SetActive(false);
            if (_rageCount != null)
                StopCoroutine(_rageCount);
        }
        //�ѽ�14
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

    //�󴫪Z���B�ޯ�ɩI�s�A�I�s�e�нT�O����s�LPlayerValue
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
            
            // �����ӧޯ�Ҧ��˳����]
            for (int i = 0; i < 3; i++)
            {
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(id * 3 + i).gameObject.SetActive(false);
            }
            int Lv = SkillField[id].Level;
            if (Lv >= 0 && Lv <= 3) // �̵��Ŷ}�Ҹӧޯ�˳����A�̦h3��
            {
                for (int i = 0; i < Lv; i++)
                {
                    UICtrl.Instance.WeaponfieldUI.transform.GetChild(id * 3 + i).gameObject.SetActive(true);
                }
            }
            else
                Debug.Log("�ޯ൥�Ť��b�w�]�d��");
        }
    }

    //��ҹ���
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

    //�ѽ�ƭȭp��
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
        //�˳�4���S���O
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

    //���N�o�q��
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

    //�[��ͩR�q��
    public void GetHp(float value, bool useBehurtTimer = false)
    {
        if (value >= 0) // �^��
        {
            if (HP < maxHP - value)
                HP += value;
            else
                HP = maxHP;
        }
        else // ����
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
                    StartCoroutine(PlayerCtrl.Instance.BehurtTimer(false)); // ������ɤ��|Ĳ�o�S��(�ˮ`�����Q�ױ�)�A��Ĳ�o�L�ĴV
                }
                else 
                {
                    HP += value;
                    if (useBehurtTimer == true)
                        StartCoroutine(PlayerCtrl.Instance.BehurtTimer(true)); //����ίS��
                }
            }
            else
            {
                HP = 0;
                UICtrl.Instance.gameover();
            }
        }
    }
    //�[���]�O�q��
    public void GetAp(float value)
    {
        if(value >= 0) // �^�]
        {
            if (maxAP > AP + value)
                AP += value;
            else
                AP = maxAP;
        }
        else // ���]
        {
            if (AP > -value)
                AP += value;
            else
                AP = 0;

            if(PassiveSkills[14]) // �ѽ�14
            {
                if(AP < 0.1f)
                    Reload(true);
            }
        }
    }

    //�۰ʦ^�]
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

    // �}��reload��{
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
    //�ѽ�14
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
            if (PassiveSkills[20]) // �ѽ�20
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

    //�[���q��
    public void GetRage(float value)
    {
        if (value > 0) // �[
        {
            if (maxRage > Rage + value)
                Rage += value;
            else
                Rage = maxRage;

            RageTime = 3; // ���s����I�h�˼�
            if(_lostRage != null)
            {
                StopCoroutine(_lostRage);
                _lostRage = null;
            }
        }
        else if(value < 0) // ��
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
        if (value != 0) // �קK���j��
        {
            PlayerValueUpdate();
            SkillFieldValueUpdate();
        }
        
    }
    //����˼�
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
    //����I�h
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

    // �P�_�ӧޯ����O�_�֦����w�˳�
    public bool isHaveweaponid(int field, int id)
    {
        if(WeaponField[field * 3].ID == id || WeaponField[field * 3 + 1].ID == id || WeaponField[field * 3 + 2].ID == id)
            return true;
        else
            return false;
    }

}

//�}���׬[�c
public enum RarityType
{
    Normal,
    Magic,
    Rare,
    Unique
}

//�ޯ����
public enum SkillTagType
{
    Attack,//����
    Spell,//�k�N
    Movement,//�첾
    Range,//�d��
    Projectile,//��g��
    Physical,//���z
    Fire,//��
    Cold,//�B
    Lightning,//�q
    Chaos,//�V�P

}

//�ޯ�[�c
public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }    // �W��
    public float maxCD { get; set; }    // �N�o�ɶ�
    public float Damage { get; set; }   // �ˮ`
    public float Size { get; set; }     // �ޯ�d��j�p
    public float Speed { get; set; }    // �ޯ୸��t��
    public float Cost { get; set; }     // �]��
    public float Crit { get; set; }     // �����v
    public float CritDmg { get; set; }
    public int Price { get; set; }      // ����
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

//�ޯ����[�c
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

//�Z���[�c
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

//�Z�����[�c
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
