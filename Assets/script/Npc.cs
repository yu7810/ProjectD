using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public NpcType npcType;
    public string Name;
    public bool startRandom; // 使否在生成時隨機商品
    public int money;
    public int passivepoint;
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
                Debug.Log(gameObject.name + " 在說話");
                break;
            case NpcType.Weaponstore:
                UICtrl.Instance.showWeaponstore(Switch, item);
                UICtrl.Instance.nowWeaponstore = this.gameObject.GetComponent<Npc>();
                break;
            case NpcType.Skillstore:
                UICtrl.Instance.showSkillstore(Switch, item);
                UICtrl.Instance.nowSkillstore = this.gameObject.GetComponent<Npc>();
                break;
            case NpcType.Money:
                if (Switch)
                {
                    LevelCtrl.Instance.DropMoney(money, transform.position, 1f);
                    Destroy(gameObject);
                }
                break;
            case NpcType.Passiveskill:
                if (Switch)
                {
                    ValueData.Instance.passiveskillPoint += passivepoint;
                    UICtrl.Instance.passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
                    Destroy(gameObject);
                }
                break;
        }
    }

    public void RandomItem()
    {
        List<int> _pool = new List<int>();//所有商品池
        List<int> _item = new List<int>();//存放取出的商品

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
    Skillstore,
    Money,
    Passiveskill,
}