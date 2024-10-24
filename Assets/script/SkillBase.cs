using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase
{
    public UICtrl UIctrl;

    public string Name { get; set; }      // �ޯ�W��
    public float Cooldown { get; set; }   // �ޯ�N�o�ɶ�
    public int Damage { get; set; }       // �ޯ�ˮ`
    public float Size { get; set; }       // �ޯ�d��j�p

    public SkillBase(string name, float cooldown, int damage, float size)
    {
        Name = name;
        Cooldown = cooldown;
        Damage = damage;
        Size = size;
    }

}

public class SkillData
{
    public SkillBase[] Skill = new SkillBase[] {
        new SkillBase("A",3f,10,1),

    };
}