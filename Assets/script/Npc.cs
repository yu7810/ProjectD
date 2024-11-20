using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcType npcType;
    public List<int> item = new List<int> { };

    public void doNpc(bool Switch) {
        switch (npcType) 
        {
            case NpcType.Talk:
                Debug.Log(gameObject.name + " ¦b»¡¸Ü");
                return;
            case NpcType.Weaponstore:
                UICtrl.Instance.showWeaponstore(Switch, item);
                return;
            case NpcType.Skillstore:
                UICtrl.Instance.showSkillstore(Switch, item);
                return;
        }
    }
}

public enum NpcType{ 
    Talk,
    Weaponstore,
    Skillstore
}