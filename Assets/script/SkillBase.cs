using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase
{
    public UICtrl UIctrl;

    public string Name { get; set; }      // 技能名稱
    public float Cooldown { get; set; }   // 技能冷卻時間
    public int Damage { get; set; }       // 技能傷害
    public float Size { get; set; }       // 技能範圍大小

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