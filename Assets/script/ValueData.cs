using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    public GameObject Player;
    private static ValueData instance;
    public bool canBehurt;//�i�Q�����A�Ω���˵L�ĴV
    public bool isUIopen;//�}��UI

    //�򩳼ƭ�
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
    public float Power;//��¦�ˮ`���v
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
        new WeaponBase(0,"-", 1f, 1f, 1f, 1f, 1f, 0),//Dmg�BCD�BSize�BSpeed�BCost�ҬO���v�A1f=100%
        new WeaponBase(1,"�C", 1.6f, 0.7f, 1f, 1f, 1f, 0.1f),
        new WeaponBase(2,"�}", 1.5f, 1f, 1f , 1.5f, 0.6f, 0.25f),
        new WeaponBase(3,"��", 2.4f, 1.3f, 1.5f, 1f, 1f, 0.1f),
    };
    //�˳Ƥ���
    public string[] WeaponIntro = new string[] {
        "-",
        "�˳�1������",
        "�˳�2������",
        "�˳�3������",
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
        Cooldown = base_Cooldown + add_Cooldown;
        CostDown = base_CostDown + add_CostDown;
        Crit = base_Crit + add_Crit;
        CritDmg = base_CritDmg + add_CritDmg;
        //��svalue UI
        UICtrl.Instance.UpdateValueUI();
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
            UICtrl.Instance.SkillfieldUI.transform.GetChild(id).transform.Find("Icon").GetComponent<TipInfo>().UpdateInfo(1, SkillField[id].Name, SkillField[id].maxCD, SkillField[id].Cost, SkillField[id].Damage, SkillField[id].Crit, SkillField[id].Size, SkillField[id].Speed, SkillIntro[SkillField[id].ID]);
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
                add_Power += 0.1f;
                break;
        }
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
