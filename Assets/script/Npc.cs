using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using I2.Loc;
public class Npc : MonoBehaviour
{
    public NpcType npcType;
    public string Name;
    public TextMeshProUGUI NameUI;

    [Header("�q�����y")] 
    public int money;
    public int passivepoint;

    [Header("�ө� / �D��")]
    public bool startRandom; // �ϧ_�b�ͦ����H���ӫ~
    public bool unlimited; // �ӫ~���|�Q����
    public List<int> item = new List<int> { };
    public List<int> itemlevel = new List<int> { };

    [Header("��")]
    public PrizeBase Prize;//���y����
    public int Level;//���d�s��

    private void OnEnable()
    {
        if (startRandom)
            RandomItem();
    }

    public void doNpc(bool Switch) {
        switch (npcType) 
        {
            case NpcType.Door:
                if (Switch) 
                {
                    LevelCtrl.Instance.nowPrize = Prize;
                    int nextLevel = Level;
                    LevelCtrl.Instance.NextLevel(nextLevel);
                }
                break;
            case NpcType.Weaponstore:
                UICtrl.Instance.showWeaponstore(Switch, item);
                if (Switch)
                {
                    UICtrl.Instance.nowWeaponstore = this.gameObject.GetComponent<Npc>();
                    UICtrl.Instance.WeaponStoreReloadBtn.SetActive(startRandom);
                }
                else if (!Switch)
                    UICtrl.Instance.nowWeaponstore = null;
                break;
            case NpcType.Skillstore:
                UICtrl.Instance.showSkillstore(Switch, item, itemlevel);
                if (Switch)
                {
                    UICtrl.Instance.nowSkillstore = this.gameObject.GetComponent<Npc>();
                    UICtrl.Instance.SkillStoreReloadBtn.SetActive(startRandom);
                }
                else if (!Switch)
                    UICtrl.Instance.nowSkillstore = null;
                    break;
            case NpcType.Money:
                if (Switch)
                {
                    LevelCtrl.Instance.DropMoney(money, transform.position, 1f);
                    if (gameObject.TryGetComponent<Collider>(out Collider _box)) // �]��Destroy���|Ĳ�oOnTriggerExit�A�ҥH��ʩI�s
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(gameObject);
                }
                break;
            case NpcType.Passivepoint:
                if (Switch)
                {
                    ValueData.Instance.passiveskillPoint += passivepoint;
                    UICtrl.Instance.passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
                    if (gameObject.TryGetComponent<Collider>(out Collider _box)) // �]��Destroy���|Ĳ�oOnTriggerExit�A�ҥH��ʩI�s
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(gameObject);
                }
                break;
            case NpcType.Weapon:
                UICtrl.Instance.showBox(Switch, item);
                if (Switch)
                    UICtrl.Instance.nowWeaponstore = this.gameObject.GetComponent<Npc>();
                else if (!Switch)
                    UICtrl.Instance.nowWeaponstore = null;
                break;
            case NpcType.Skill:
                UICtrl.Instance.showBox(Switch, item, itemlevel);
                if (Switch)
                    UICtrl.Instance.nowSkillstore = this.gameObject.GetComponent<Npc>();
                else if (!Switch)
                    UICtrl.Instance.nowSkillstore = null;
                break;
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

        itemlevel.Clear();
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, _pool.Count);
            _item.Add(_pool[randomIndex]);
            _pool.RemoveAt(randomIndex);
            itemlevel.Add(Random.Range(1, LevelCtrl.Instance.nowclass + 1));
        }

        item.Clear();
        item.AddRange(_item);
    }

    public void showName()
    {
        string _name = "";
        if (npcType == NpcType.Door)
            LocalizationManager.TryGetTranslation(Name, out _name);
        else if (npcType == NpcType.Weaponstore)
            LocalizationManager.TryGetTranslation("NPC/Weaponstore Name", out _name);
        else if (npcType == NpcType.Skillstore)
            LocalizationManager.TryGetTranslation("NPC/Skillstore Name", out _name);
        else if (npcType == NpcType.Money)
        {
            LocalizationManager.TryGetTranslation("NPC/Exitdoor Money", out _name);
            _name = money + " " + _name;
        }
        else if (npcType == NpcType.Passivepoint)
        {
            LocalizationManager.TryGetTranslation("NPC/Item Passivepoint Name", out _name);
            _name = passivepoint + " " + _name;
        }
        else if (npcType == NpcType.Weapon)
            LocalizationManager.TryGetTranslation("Weapon/Weapon Name " + item[0], out _name);
        else if (npcType == NpcType.Skill)
        {
            LocalizationManager.TryGetTranslation("Skill/Skill Name " + item[0], out _name);
            _name = "L" + itemlevel[0] + " " + _name;
        }

        NameUI.text = _name;
    }

}

public enum NpcType{ 
    Door,
    Weaponstore,
    Skillstore,
    Money,
    Passivepoint,
    Weapon,
    Skill,
}