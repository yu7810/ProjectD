using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    private static ValueData instance;
    public bool canBehurt;//�i�Q�����A�Ω���˵L�ĴV

    //�򩳼ƭ�
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Power = 5;
    public float base_SkillSpeed = 0;
    public float base_EnemyTimer = 1;
    public float base_AttackSize = 1;
    public float base_Cooldown = 0;
    public float base_CostDown = 0;
    public float base_Crit = 0;
    public float base_CritDmg = 2f;

    //�ѽ�ƭ�
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

    //���~�ƭ�(�w�d)


    //��e�`�ƭ�
    public bool[] PassiveSkills; //��e�U�ѽ��I���[�I
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Power;//�����O
    public float SkillSpeed;//�ޯಾ�ʳt��
    public float MoveSpeed;//���ʳt��
    public float EnemyTimer;//�ĤH�ͦ��t��%
    public float AttackSize;//�ޯ�j�p%
    public float Cooldown;//�N�o���v%
    public float CostDown;//�]�ӭ��v%
    public float Crit;//�ɲv
    public float CritDmg;//�ɶ�


    //�ޯ�icon
    public Sprite[] SkillIcon;
    //�Z��icon
    public Sprite[] WeaponIcon;

    //�ޯ��`��
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,"-",1,1,1,1,0,0),//�L
        new SkillBase(1,"��¦����",1f,10,1f,1,1,0.1f),//��¦����
        new SkillBase(2,"��¦�{��",3f,0,11f,1,0,0),//��¦�{�� size=�첾�Z��
        new SkillBase(3,"����",3f,10,1f,1,0,0),
    };

    //�ޯश��
    public string[] SkillIntro = new string[] {
        "-",
        "�ޯ�1������",
        "�ޯ�2������",
        "�ޯ�3������",
    };

    //�w�˳Ƨޯ����
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//�ƹ�L
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//�ƹ�R
        new SkillFieldBase(0,"-",1f,1,1,1,1,0,0),//�ť���
    };

    //�˳��`��
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,"-",0,1f,1f,1f,1f,0),//Dmg�BCD�BSize�BSpeed�BCost�ҬO���v�A1f=100%
        new WeaponBase(1,"�C",0,1f,1f,1f,1f,0),
        new WeaponBase(2,"�}",0,1f,1f,1f,1f,0),
        new WeaponBase(3,"��",0,1f,1f,1f,1f,0),
    };

    //�w�˳Ƹ˳�
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//�ƹ�L
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//�ƹ�R
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f,0),//�ť���
    };

    //�C���[��ѽ�ɩI�s�A��s�Ҧ��ƭ�
    //�ٻݭn�M���ѽ��I
    public void PlayerValueUpdate() {
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Power = base_Power + add_Power;
        MoveSpeed = base_MoveSpeed + add_MoveSpeed;
        SkillSpeed = base_SkillSpeed + add_SkillSpeed;
        MoveSpeed = base_MoveSpeed + add_MoveSpeed;
        EnemyTimer = base_EnemyTimer + add_EnemyTimer;
        AttackSize = base_AttackSize + add_AttackSize;
        Cooldown = base_Cooldown + add_Cooldown;
        CostDown = base_CostDown + add_CostDown;
        Crit = base_Crit + add_Crit;
        CritDmg = base_CritDmg + add_CritDmg;
    }

    //�󴫪Z���B�ޯ�ɩI�s�A�I�s�e�нT�O����s�LPlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * Cooldown * Weapon[WeaponField[id].ID].Cooldown;
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * Power * Weapon[WeaponField[id].ID].Damage;
            SkillField[id].Size = Skill[SkillField[id].ID].Size * AttackSize * Weapon[WeaponField[id].ID].Size;
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * SkillSpeed * Weapon[WeaponField[id].ID].Speed;
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * CostDown * Weapon[WeaponField[id].ID].Costdown;
            SkillField[id].Crit = Skill[SkillField[id].ID].Crit + Crit + Weapon[WeaponField[id].ID].Crit;
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

    public SkillBase(int id, string name, float maxcd, float damage, float size, float speed, float cost, float crit)
    {
        ID = id;
        Name = name;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
        Crit = crit;
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

    public WeaponBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown, float crit)
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
