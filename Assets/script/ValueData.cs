using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueData : MonoBehaviour
{
    //�򩳼ƭ�
    public float base_maxAp = 10;
    public float base_maxHp = 30;
    public float base_MoveSpeed = 3f;
    public float base_Attack = 5;

    //�����ƭ�
    public float add_maxAp;
    public float add_maxHp;
    public float add_MoveSpeed;
    public float add_Attack;

    //���~�ƭ�(�w�d)


    //��e�ƭ�
    public float AP;
    public float maxAP;
    public float HP;
    public float maxHP;
    public float EXP;
    public float maxEXP;
    public float Attack;
    public float MoveSpeed;
    public float EnemyTimer;

    //�ޯ�icon
    public Sprite[] SkillIcon;

    //�ޯ��`��
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase(0,"-",1f,1,1,1,1,0),//�L
        new SkillBase(1,"��¦����",0,1.1f,10,1f,1,0),//��¦����
        new SkillBase(2,"��¦�{��",0,3f,0,11f,1,0),//��¦�{�� size=�첾�Z��
        new SkillBase(3,"����",0,3f,10,1f,1,0),
    };

    //�w�˳Ƨޯ����
    public SkillBase[] SkillField = new SkillBase[] {
        new SkillBase(1,"��¦����",0,1f,10,1f,1,0),//�ƹ�L
        new SkillBase(0,"-",1f,1,1,1,1,0),//�ƹ�R
        new SkillBase(0,"-",1f,1,1,1,1,0),//�ť���
    };

    //�C���[��ѽ�ɩI�s�A��s�Ҧ��ƭ�
    //�ٻݭn�M���ѽ��I
    public void ValueUpdate() {
        maxAP = base_maxAp + add_maxAp;
        maxHP = base_maxHp + add_maxHp;
        Attack = base_Attack + add_Attack;
        MoveSpeed = base_MoveSpeed + MoveSpeed;

    }

}

public class SkillBase
{
    public int ID { get; set; }
    public string Name { get; set; }      // �ޯ�W��
    public float nowCD { get; set; }   // �ޯ��e�N�o (�ޯ�����)
    public float maxCD { get; set; }   // �ޯ�N�o�ɶ�
    public float DamageAdd { get; set; }       // �ޯ�ˮ`
    public float Size { get; set; }       // �ޯ�d��j�p
    public float Speed { get; set; }       // �ޯ�t��
    public float Cost { get; set; }       // �ޯ��]��

    public SkillBase(int id, string name, float nowcd, float maxcd, float damageadd, float size, float speed, float cost)
    {
        ID = id;
        Name = name;
        nowCD = nowcd;
        maxCD = maxcd;
        DamageAdd = damageadd;
        Size = size;
        Speed = speed;
        Cost = cost;
    }

}