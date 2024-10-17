using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public string Name { get; set; }      // �ޯ�W��
    public float Cooldown { get; set; }   // �ޯ�N�o�ɶ�
    public int Damage { get; set; }       // �ޯ�ˮ`
    public float Size { get; set; }       // �ޯ�d��j�p

    public Skill(string name, float cooldown, int damage, float size)
    {
        Name = name;
        Cooldown = cooldown;
        Damage = damage;
        Size = size;
    }

    // ������k�A����ޯ�i�H��{�ۤv���欰
    public virtual void UseSkill()
    {
        Debug.Log(Name + " is used!");
    }
}

public class Skill_A : Skill
{
    public Skill_A() : base("Skill_A", 2.5f, 10, 1.0f)
    {
        // �i�H�b�o�̦ۭqSkill_A����L�ݩ�
    }

    // ���g������ UseSkill ��k
    public override void UseSkill()
    {
        Debug.Log(Name + " is activated with " + Damage + " damage and a size of " + Size);
        // �o�̥i�H�K�[���骺�ޯ�ĪG
    }
}
