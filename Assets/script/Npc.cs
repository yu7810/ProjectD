using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcType npcType;
    public string Name;
    public bool startRandom;
    public List<int> item = new List<int> { };

    private void OnEnable()
    {
        if (startRandom)
            RandomItem();
    }

    public void doNpc(bool Switch) {
        switch (npcType) 
        {
            case NpcType.Talk:
                Debug.Log(gameObject.name + " �b����");
                return;
            case NpcType.Weaponstore:
                UICtrl.Instance.showWeaponstore(Switch, item);
                return;
            case NpcType.Skillstore:
                UICtrl.Instance.showSkillstore(Switch, item);
                return;
        }
    }

    public void RandomItem()
    {
        List<int> _pool = new List<int>();//�Ҧ��ӫ~��
        List<int> _item = new List<int>();//�s����X���ӫ~

        if (npcType == NpcType.Skillstore)
        {
            _pool.AddRange(ValueData.Instance.skillstorePool);
        }
        else if (npcType == NpcType.Weaponstore)
        {
            _pool.AddRange(ValueData.Instance.weaponstorePool);
        }
        else
            return;
        
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, _pool.Count);
            _item.Add(_pool[randomIndex]);
            _pool.RemoveAt(randomIndex);
        }

        item.Clear();
        item.AddRange(_item);
    }

}

public enum NpcType{ 
    Talk,
    Weaponstore,
    Skillstore
}