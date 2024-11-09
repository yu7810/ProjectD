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

    //�ѽ�ƭ�
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Power;

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


    //�ޯ�icon
    public Sprite[] SkillIcon;
    //�Z��icon
    public Sprite[] WeaponIcon;

    //�ޯ��`��
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,"-",1,1,1,1,0),//�L
        new SkillBase(1,"��¦����",1f,10,1f,1,1),//��¦����
        new SkillBase(2,"��¦�{��",3f,0,11f,1,0),//��¦�{�� size=�첾�Z��
        new SkillBase(3,"����",3f,10,1f,1,0),
    };

    //�w�˳Ƨޯ����
    public SkillFieldBase[] SkillField = new SkillFieldBase[] {
        new SkillFieldBase(0,"-",1f,1,1,1,1,0),//�ƹ�L
        new SkillFieldBase(0,"-",1f,1,1,1,1,0),//�ƹ�R
        new SkillFieldBase(0,"-",1f,1,1,1,1,0),//�ť���
    };

    //�˳��`��
    public WeaponBase[] Weapon = new WeaponBase[] {
        new WeaponBase(0,"-",0,1f,1f,1f,1f),//Dmg�BCD�BSize�BSpeed�BCost�ҬO���v�A1f=100%
        new WeaponBase(1,"�C",0,1f,1f,1f,1f),
        new WeaponBase(2,"�}",0,1f,1f,1f,1f),
        new WeaponBase(3,"��",0,1f,1f,1f,1f),
    };

    //�w�˳Ƹ˳�
    public WeaponFieldBase[] WeaponField = new WeaponFieldBase[] {
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f),//�ƹ�L
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f),//�ƹ�R
        new WeaponFieldBase(0,"-",0,1f,1f,1f,1f),//�ť���
    };

    //�C���[��ѽ�ɩI�s�A��s�Ҧ��ƭ�
    //�ٻݭn�M���ѽ��I
    public void PlayerValueUpdate() {
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Power = base_Power + add_Power;
        MoveSpeed = base_MoveSpeed + add_MoveSpeed;

    }

    //�󴫪Z���B�ޯ�ɩI�s�A�I�s�e�нT�O����s�LPlayerValue
    public void SkillFieldValueUpdate() {
        for (int id = 0; id < 3; id++) {
            SkillField[id].maxCD = Skill[SkillField[id].ID].maxCD * Cooldown * Weapon[WeaponField[id].ID].Cooldown;
            SkillField[id].Damage = Skill[SkillField[id].ID].Damage * Power * Weapon[WeaponField[id].ID].Damage;
            SkillField[id].Size = Skill[SkillField[id].ID].Size * AttackSize * Weapon[WeaponField[id].ID].Size;
            SkillField[id].Speed = Skill[SkillField[id].ID].Speed * SkillSpeed * Weapon[WeaponField[id].ID].Speed;
            SkillField[id].Cost = Skill[SkillField[id].ID].Cost * CostDown * Weapon[WeaponField[id].ID].Costdown;
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
    public string Name { get; set; }      // �ޯ�W��
    public float maxCD { get; set; }   // �ޯ�N�o�ɶ�
    public float Damage { get; set; }       // �ޯ�ˮ`
    public float Size { get; set; }       // �ޯ�d��j�p
    public float Speed { get; set; }       // �ޯ୸��t��
    public float Cost { get; set; }       // �ޯ��]��

    public SkillBase(int id, string name, float maxcd, float damage, float size, float speed, float cost)
    {
        ID = id;
        Name = name;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
    }
}

//�ޯ����[�c
public class SkillFieldBase
{
    public int ID { get; set; }
    public string Name { get; set; }      // �ޯ�W��
    public float nowCD { get; set; }   // �ޯ��e�N�o (�ޯ�����)
    public float maxCD { get; set; }   // �ޯ�N�o�ɶ�
    public float Damage { get; set; }       // �ޯ�ˮ`
    public float Size { get; set; }       // �ޯ�d��j�p
    public float Speed { get; set; }       // �ޯ୸��t��
    public float Cost { get; set; }       // �ޯ��]��

    public SkillFieldBase(int id, string name, float nowcd, float maxcd, float damage, float size, float speed, float cost)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        Damage = damage;
        Size = size;
        Speed = speed;
        Cost = cost;
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

    public WeaponBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
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

    public WeaponFieldBase(int id, string name, float damage, float cooldown, float size, float speed, float costdown)
    {
        ID = id;
        Name = name;
        Damage = damage;
        Cooldown = cooldown;
        Size = size;
        Speed = speed;
        Costdown = costdown;
    }
}