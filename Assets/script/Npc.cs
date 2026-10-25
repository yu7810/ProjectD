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
    public RectTransform NameUIBackground;

    [Header("通關獎勵")] 
    public int money;
    public int passivepoint;
    public int fixedPassiveTreeId = -1;
    private bool rewardConsumed;

    [Header("商店 / 道具")]
    public bool startRandom; // 使否在生成時隨機商品
    public bool unlimited; // 商品不會被消耗
    public List<int> item = new List<int> { };
    public List<int> itemlevel = new List<int> { };
    [Min(0f)] public float normalRarityChance = 60f;
    [Min(0f)] public float magicRarityChance = 30f;
    [Min(0f)] public float rareRarityChance = 10f;
    [Min(0f)] public float uniqueRarityChance = 0f;
    [Min(0f)] public float skillLevel1Chance = 60f;
    [Min(0f)] public float skillLevel2Chance = 30f;
    [Min(0f)] public float skillLevel3Chance = 10f;
    private bool randomStockReady;
    private static ShopPoolTable commonShopPool;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetShopPoolCache() => commonShopPool = null;

    [Header("門")]
    public PrizeBase Prize;//獎勵類型
    public int Level;//關卡編號

    private IEnumerator Start()
    {
        if (startRandom && !randomStockReady)
        {
            yield return null; // All catalog Awake methods must finish first.
            if (!randomStockReady) RandomItem();
        }

        if (npcType != NpcType.Passivetree || fixedPassiveTreeId < 0)
            yield break;

        // Scene rewards wait for ValueData.Start to initialize unlocks.
        yield return null;
        if (!UICtrl.Instance.CanOfferPassiveTree(fixedPassiveTreeId))
        {
            if (gameObject.TryGetComponent<Collider>(out Collider treeCollider))
                PlayerCtrl.Instance.OnTriggerExit(treeCollider);
            Destroy(gameObject);
            yield break;
        }
        showName();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLocalizeEvent += RefreshLocalizedName;
        randomStockReady = false;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizeEvent -= RefreshLocalizedName;
    }

    private void RefreshLocalizedName()
    {
        if (NameUI != null && NameUI.gameObject.activeInHierarchy)
            showName();
    }

    public void doNpc(bool Switch) {
        if (Switch && startRandom && !randomStockReady &&
            (npcType == NpcType.Skillstore || npcType == NpcType.Weaponstore) && !RandomItem())
            return;
        switch (npcType) 
        {
            case NpcType.Door:
                if (Switch) 
                {
                    LevelCtrl.Instance.TryEnterPortal(this);
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
                    if (gameObject.TryGetComponent<Collider>(out Collider _box)) // 因為Destroy不會觸發OnTriggerExit，所以手動呼叫
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(gameObject);
                }
                break;
            case NpcType.Passivepoint:
                if (Switch && !rewardConsumed)
                {
                    rewardConsumed = true;
                    ValueData.Instance.passiveskillPoint += passivepoint;
                    UICtrl.Instance.passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
                    if (gameObject.TryGetComponent<Collider>(out Collider _box)) // 因為Destroy不會觸發OnTriggerExit，所以手動呼叫
                        PlayerCtrl.Instance.OnTriggerExit(_box);
                    Destroy(gameObject);
                }
                break;
            case NpcType.Passivetree:
                if (Switch && !rewardConsumed && UICtrl.Instance.TryCallPassivetreeSelect(fixedPassiveTreeId))
                {
                    rewardConsumed = true;
                    if (gameObject.TryGetComponent<Collider>(out Collider treeCollider))
                        PlayerCtrl.Instance.OnTriggerExit(treeCollider);
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

    public bool RandomItem()
    {
        if (!startRandom || (npcType != NpcType.Skillstore && npcType != NpcType.Weaponstore)) return false;
        var data = ValueData.Instance;
        if (data == null) return false;
        try
        {
            if (commonShopPool == null)
            {
                var csv = Resources.Load<TextAsset>("ShopPool");
                if (csv == null) throw new System.FormatException("Missing Resources/ShopPool.csv.");
                commonShopPool = ShopPoolTable.Parse(csv.text);
            }
            bool skill = npcType == NpcType.Skillstore;
            double[] chances = skill
                ? new double[] { skillLevel1Chance, skillLevel2Chance, skillLevel3Chance }
                : new double[] { normalRarityChance, magicRarityChance, rareRarityChance, uniqueRarityChance };
            var products = commonShopPool.Sample(skill ? ShopProductType.Skill : ShopProductType.Weapon,
                chances, id => (int)data.Weapon[id].Rarity,
                () => (double)Random.Range(0, int.MaxValue) / int.MaxValue);
            if (products.Count == 0)
            {
                Debug.LogWarning("Shop pool has no products matching this merchant's chances; stock unchanged.", this);
                return false;
            }
            var ids = new List<int>();
            var levels = new List<int>();
            foreach (var product in products)
            {
                ids.Add(product.ItemID);
                levels.Add(product.Level);
            }
            item = ids;
            itemlevel = levels;
            randomStockReady = true;
            return true;
        }
        catch (System.FormatException error)
        {
            Debug.LogError("Shop pool was not applied; stock unchanged: " + error.Message, this);
            return false;
        }
    }

    public void SetNameVisible(bool visible)
    {
        if (NameUI != null)
            NameUI.gameObject.SetActive(visible);
        if (NameUIBackground != null)
            NameUIBackground.gameObject.SetActive(visible);
    }

    private void UpdateNameBackground()
    {
        if (NameUIBackground == null || NameUI == null)
            return;

        Vector2 textSize = NameUI.GetPreferredValues(NameUI.text,
            NameUI.rectTransform.rect.width, Mathf.Infinity);
        NameUIBackground.sizeDelta = new Vector2(
            Mathf.Min(NameUI.rectTransform.rect.width, textSize.x) + 16f,
            textSize.y + 8f);
        NameUIBackground.gameObject.SetActive(NameUI.gameObject.activeSelf);
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
        else if (npcType == NpcType.Passivetree)
            LocalizationManager.TryGetTranslation("NPC/Item Passivetree Name", out _name);
        else if (npcType == NpcType.Weapon)
            LocalizationManager.TryGetTranslation("Weapon/Weapon Name " + item[0], out _name);
        else if (npcType == NpcType.Skill)
        {
            LocalizationManager.TryGetTranslation("Skill/Skill Name " + item[0], out _name);
            _name = "L" + itemlevel[0] + " " + _name;
        }

        NameUI.text = _name;
        UpdateNameBackground();
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
    Passivetree,
}