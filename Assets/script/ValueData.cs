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
    public bool isUIopen;//�}��UI
    public CinemachineVirtualCamera virtualCamera;//���Y
    public GameObject moneyPrefab;

    //�򩳼ƭ�
    public int money;//���W�����������ƶq
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Power = 5;
    public float base_SkillSpeed = 1;
    public float base_EnemyTimer = 1;
    public float base_AttackSize = 1;
    public float base_Cooldown = 1;
    public float base_CostDown = 1;
    public float base_Crit = 0;
    public float base_CritDmg = 2f;
    public float base_RestoreAP = 1f;//AP�C��۵M��_
    public float base_Damagereduction;//�ˮ`��K
    public float base_Vision;
    public float base_BulletSpeed;

    //�ѽ�ƭ�
    public int passiveskillPoint = 0;//�ѽ��I��
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Power;
    public float add_SkillSpeed;
    public float add_EnemyTimer;
    public float add_AttackSize;
    public float add_Cooldown;
    public float add_CostDown;
    public float add_Crit;
    public float add_CritDmg;
    public float add_RestoreAP;
    public float add_Damagereduction;
    public float add_Vision;
    public float add_BulletSpeed;

    //���~�ƭ�(�w�d)


    //��e�`�ƭ�
    public bool[] PassiveSkills; //��e�U�ѽ��I���[�I
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Power;//��¦�ˮ`���v
    public float SkillSpeed;//�ޯಾ�ʳt��
    public float MoveSpeed;//���ʳt��
    public float EnemyTimer;//�ĤH�ͦ��t��%
    public float AttackSize;//�ޯ�j�p%
    public float Cooldown;//�N�o���v%
    public float CostDown;//�]�ӭ��v%
    public float Crit;//�ɲv
    public float CritDmg;//�ɶ�
    public float RestoreAP;
    public float Damagereduction;//�ˮ`��K%
    public float Vision;//�����d��(FOV)
    public float BulletSpeed;//��g������t��%

    public Sprite[] SkillIcon;//�ޯ�icon
    public Sprite[] WeaponIcon;//�Z��icon

    //�ޯ��`��
    [NonSerialized]
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,0,"-",0,0,0,0,0,0),//�L
        new SkillBase(1,0,"�A��",2f,10,1f,1,0.2f,0.25f),
        new SkillBase(2,10,"�Ĩ�",5f,0,1f,1,0,0),//size=�첾�Z��
        new SkillBase(3,0,"����",1f,10,1f,1,0,0),
        new SkillBase(4,10,"�{�{",3.4f,0f,1f,1,1,0f),
        new SkillBase(5,20,"�s���",2f,10,0.8f,1,1,0.1f),
        new SkillBase(6,0,"�����",2f,20,1.1f,1,2,0.1f),
        new SkillBase(7,0,"�����",2f,40,1.4f,1,2,0.1f),
        new SkillBase(8,20,"The����",20f,0,1f,1,0,0f),
        new SkillBase(9,0,"���b",0.3f,3,1f,1,0.6f,0f),
        new SkillBase(10,30,"���n",1f,0f,1f,1,0f,0f),
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
        "�ͦ��@�ӫ���6�������A�A������y�����ˮ`�|�Q��j3����A�Q�����H��γy���d��ˮ`",
        "�·ƹ���V�o�g�@�T���u�A�R���ĤH�����",
        "�b�ƹ���m�ͦ��@�Ӥ��y�A�@�q�ɶ����z���A���Ӥ@�b��e�]�O�óy��(���Ӷq��5)�ˮ`",
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
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//�ƹ�L
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//�ƹ�R
        new SkillFieldBase(0,"-",0f,1,1,1,1,0,0),//�ť���
    };

    //�˳��`��
    [NonSerialized]
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,RarityType.Normal,0,"�Ť�", 1f, 1f, 1f, 1f, 1f, 0),//Dmg�BCD�BSize�BSpeed�BCost�ҬO���v�A1f=100%
        new WeaponBase(1,RarityType.Normal,15,"�K�C", 1.5f, 0.85f, 1f, 1f, 1f, 0.1f),
        new WeaponBase(2,RarityType.Normal,15,"�K�}", 1.2f, 1f, 1f , 1.4f, 0.85f, 0f),
        new WeaponBase(3,RarityType.Normal,15,"�K��", 1.2f, 1.5f, 1.5f, 0.8f, 1.2f, 0.2f),
        new WeaponBase(4,RarityType.Magic,15,"�E�_", 1, 1f, 1f, 1f, 1f, 0.05f),
        new WeaponBase(5,RarityType.Rare,30,"�}��", 0.5f, 1f, 1f, 0.8f, 0.8f, 0.25f),
        new WeaponBase(6,RarityType.Rare,30,"�v�v", 2f, 1.8f, 0.75f, 1f, 1.6f, 0.15f),
        new WeaponBase(7,RarityType.Rare,30,"�ɳոq��", 0.5f, 0.5f, 0.5f, 2f, 0.5f, 0f),
        new WeaponBase(8,RarityType.Rare,0,"�Ѯ�", 1f, 1f, 2f, 1f, 1f, 0),
        new WeaponBase(9,RarityType.Magic,15,"�۰]", 1, 1f, 1f, 1f, 1f, 0f),
    };
    //�˳Ƥ���
    [NonSerialized]
    public string[] WeaponIntro = new string[] {
        "-",
        "",
        "",
        "",
        "���W�C1��������1%�ˮ`�W�T",
        "�ޯ�����ɱN�N�o����0.3s",
        "�ޯ୫��2��",
        "�ϥΦ첾�ޯ��Ĳ�oL�W���D�첾�ޯ�",
        "�B�N�ޯ�P�ɩR���ƼƥؼЮɡA�C�ӥؼШ϶ˮ`����20%",
        "�����ĤH������������0~3��",
    };

    //�w�˳Ƹ˳�
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//�ƹ�L
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//�ƹ�R
        new WeaponFieldBase(0,"-",1,1f,1f,1f,1f,0),//�ť���
    };

    //�ޯ�ө���
    public List<int> skillstorePool = new List<int>()
    {
        2,4,5,8,10
    };
    //�˳ưө���
    public List<int> weaponstorePool = new List<int>()
    {
        1,2,3,4,5,6,7,8,9
    };

    //�C���[��ѽ�ɩI�s�A��s�Ҧ��ƭ�
    //�ٻݭn�M���ѽ��I
    public void PlayerValueUpdate() {
        //���s
        add_maxAp = 0;
        add_maxHp = 0;
        add_MoveSpeed = 0;
        add_Power= 0;
        add_SkillSpeed = 0;
        add_EnemyTimer = 0;
        add_AttackSize = 0;
        add_Cooldown = 0;
        add_CostDown = 0;
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
        Power = base_Power + add_Power;
        MoveSpeed = 1 + add_MoveSpeed;
        SkillSpeed = base_SkillSpeed + add_SkillSpeed;
        EnemyTimer = base_EnemyTimer + add_EnemyTimer;
        AttackSize = base_AttackSize + add_AttackSize;
        Cooldown = 1 - (base_Cooldown * add_Cooldown);
        CostDown = 1 - (base_CostDown * add_CostDown);
        Crit = base_Crit + add_Crit;
        CritDmg = base_CritDmg + add_CritDmg;
        RestoreAP = base_RestoreAP + add_RestoreAP;
        Damagereduction = base_Damagereduction + add_Damagereduction;
        Vision = base_Vision + add_Vision;
        BulletSpeed = base_BulletSpeed + add_BulletSpeed;
        //��svalue UI
        UICtrl.Instance.UpdateValueUI();
        virtualCamera.m_Lens.FieldOfView = Vision;
    }

    //�󴫪Z���B�ޯ�ɩI�s�A�I�s�e�нT�O����s�LPlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * Cooldown * WeaponField[id].Cooldown;
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * Power * WeaponField[id].Damage;
            SkillField[id].Size = Skill[SkillField[id].ID].Size * AttackSize * WeaponField[id].Size;
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * SkillSpeed * WeaponField[id].Speed;
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * CostDown * WeaponField[id].Costdown;
            SkillField[id].Crit = Skill[SkillField[id].ID].Crit + Crit + WeaponField[id].Crit;
            UICtrl.Instance.SkillfieldUI.transform.GetChild(id).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Skill, SkillField[id].ID, SkillField[id].Name, SkillField[id].maxCD, SkillField[id].Cost, SkillField[id].Damage, SkillField[id].Crit, SkillField[id].Size, SkillField[id].Speed, SkillIntro[SkillField[id].ID]);
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

    //�ѽ�ƭȭp��
    public void PassiveSkillValueUpdate(int id) { 
        switch (id)
        {
            case 0:
                add_Power += 0.1f;
                break;
            case 1:
                add_Power += 0.1f;
                break;
            case 2:
                add_Power += 0.1f;
                break;
            case 3:
                add_Cooldown += 0.05f;
                break;
            case 4:
                add_Cooldown += 0.05f;
                break;
            case 5:
                add_Cooldown += 0.05f;
                break;
            case 6:
                add_Cooldown += 0.05f;
                break;
            case 7:
                add_maxHp += 5f;
                break;
            case 8:
                add_maxHp += 5f;
                break;
            case 9:
                add_maxHp += 5f;
                break;
            case 10:
                add_maxHp += 5f;
                break;
            case 12:
                add_Crit += 0.1f;
                break;
            case 13:
                add_Crit += 0.1f;
                break;
            case 14:
                add_Crit += 0.1f;
                break;
            case 15:
                add_Crit += 0.1f;
                break;
            case 16:
                add_Crit += 0.15f;
                break;
            case 18:
                add_Vision += 5f;
                break;
            case 19:
                add_Vision += 5f;
                break;
            case 20:
                add_Vision += 5f;
                break;
            case 22:
                add_BulletSpeed += 0.2f;
                break;
            case 23:
                add_BulletSpeed += 0.2f;
                break;
            case 24:
                add_BulletSpeed += 0.2f;
                break;
            case 26:
                add_maxAp += 3f;
                break;
            case 27:
                add_maxAp += 3f;
                break;
            case 28:
                add_maxAp += 3f;
                break;
            case 29:
                add_maxAp += 3f;
                break;
            case 30:
                add_RestoreAP += 0.5f;
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
                UICtrl.Instance.WeaponfieldUI.transform.GetChild(i).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(TipType.Weapon, ValueData.Instance.WeaponField[i].ID, ValueData.Instance.WeaponField[i].Name, ValueData.Instance.WeaponField[i].Cooldown, ValueData.Instance.WeaponField[i].Costdown, ValueData.Instance.WeaponField[i].Damage, ValueData.Instance.WeaponField[i].Crit, ValueData.Instance.WeaponField[i].Size, ValueData.Instance.WeaponField[i].Speed, ValueData.Instance.WeaponIntro[ValueData.Instance.WeaponField[i].ID]);
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
        if (value == 0)
            return;
        else if (value > 0) 
        {
            if (HP < maxHP - value)
                HP += value;
            else
                HP = maxHP;
        }
        else
        {
            value *= (1 - Damagereduction);
            if (HP > -value)
            {
                HP += value;
                if (useBehurtTimer == true)
                    StartCoroutine(PlayerCtrl.Instance.BehurtTimer());
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
        if (value == 0)
            return;
        else if(value > 0)
        {
            if (maxAP > AP + value)
                AP += value;
            else
                AP = maxAP;
        }
        else
        {
            if (AP > value)
                AP += value;
            else
                AP = 0;
        }
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
    public int Price { get; set; }      // ����

    public SkillBase(int id, int price,string name, float maxcd, float damage, float size, float speed, float cost, float crit)
    {
        ID = id;
        Name = name;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
        Price = price;
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

    public SkillFieldBase(int id, string name, float nowcd, float maxcd, float damage, float size, float speed, float cost, float crit)
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
    public float Costdown { get; set; }
    public float Crit { get; set; }
    public int Price { get; set; }
    public RarityType Rarity { get; set; }

    public WeaponBase(int id, RarityType rarity, int price, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
        Crit = crit;
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
    public float Costdown { get; set; }
    public float Crit { get; set; }

    public WeaponFieldBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
        Crit = crit;
    }
}
