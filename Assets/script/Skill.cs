using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public string Name { get; set; }      // 技能名稱
    public float Cooldown { get; set; }   // 技能冷卻時間
    public int Damage { get; set; }       // 技能傷害
    public float Size { get; set; }       // 技能範圍大小

    public Skill(string name, float cooldown, int damage, float size)
    {
        Name = name;
        Cooldown = cooldown;
        Damage = damage;
        Size = size;
    }

    // 虛擬方法，具體技能可以實現自己的行為
    public virtual void UseSkill()
    {
        Debug.Log(Name + " is used!");
    }
}

public class Skill_A : Skill
{
    public Skill_A() : base("Skill_A", 2.5f, 10, 1.0f)
    {
        // 可以在這裡自訂Skill_A的其他屬性
    }

    // 重寫父類的 UseSkill 方法
    public override void UseSkill()
    {
        Debug.Log(Name + " is activated with " + Damage + " damage and a size of " + Size);
        // 這裡可以添加具體的技能效果
    }
}
