using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class UICtrl
{
    [Header("Shop sell areas")]
    public GameObject SkillStoreSellArea;
    public GameObject WeaponStoreSellArea;
    public GameObject SkillStoreSalePriceUI;
    public GameObject WeaponStoreSalePriceUI;
    private UIDrag ownedItemDrag;
    private Npc dragSaleMerchant;
    private bool dragStartedInShop;
    private TipType saleItemType;
    private int saleItemField;
    private int saleItemId;
    private int saleItemLevel;
    private GameObject ownedItemDragHover;
    private PointerEventData ownedItemDragPointer;
    private Localize skillSalePriceLocalize;
    private Localize weaponSalePriceLocalize;
    private LocalizationParamsManager skillSaleParameters;
    private LocalizationParamsManager weaponSaleParameters;

    private Npc ActiveSaleMerchant()
    {
        if ((settingUI != null && settingUI.activeInHierarchy) ||
            (GameOverUI != null && GameOverUI.activeInHierarchy))
            return null;
        if (SkillStoreUI.activeInHierarchy && nowSkillstore != null &&
            nowSkillstore.gameObject.activeInHierarchy && nowSkillstore.npcType == NpcType.Skillstore)
            return nowSkillstore;
        if (WeaponStoreUI.activeInHierarchy && nowWeaponstore != null &&
            nowWeaponstore.gameObject.activeInHierarchy && nowWeaponstore.npcType == NpcType.Weaponstore)
            return nowWeaponstore;
        return null;
    }

    // Each shop owns a separate copy of the discard area's graphics and layout.
    // The original discard area keeps its own label and behavior, including in shops.
    public void RefreshOwnedItemDropArea()
    {
        if (ownedItemDrag != null && ownedItemDragPointer != null)
            ownedItemDragHover = SaleAreaAtPointer(ownedItemDragPointer);
        Npc merchant = ActiveSaleMerchant();
        RefreshSaleArea(SkillStoreSellArea, SkillStoreSalePriceUI, merchant != null && merchant == nowSkillstore,
            ref skillSalePriceLocalize, ref skillSaleParameters);
        RefreshSaleArea(WeaponStoreSellArea, WeaponStoreSalePriceUI, merchant != null && merchant == nowWeaponstore,
            ref weaponSalePriceLocalize, ref weaponSaleParameters);
        if (GarbageCan != null && GarbageCan.activeSelf != (ownedItemDrag != null))
            GarbageCan.SetActive(ownedItemDrag != null);
    }

    private void RefreshSaleArea(GameObject area, GameObject priceUI, bool visible, ref Localize label,
        ref LocalizationParamsManager parameters)
    {
        if (area == null) return;
        if (area.activeSelf != visible)
            area.SetActive(visible);
        if (priceUI == null) return;
        int price = 0;
        bool showPrice = visible && ownedItemDrag != null && dragStartedInShop &&
            ActiveSaleMerchant() == dragSaleMerchant && IsDropInside(ownedItemDragHover, area) &&
            TryGetDraggedSalePrice(out price);
        if (showPrice && label == null)
        {
            label = priceUI.GetComponentInChildren<Localize>(true);
            if (label != null)
            {
                parameters = label.GetComponent<LocalizationParamsManager>();
                if (parameters == null)
                    parameters = label.gameObject.AddComponent<LocalizationParamsManager>();
            }
        }
        if (showPrice && label != null)
        {
            string amount = price.ToString();
            if (parameters.GetParameterValue("PRICE") != amount)
                parameters.SetParameterValue("PRICE", amount);
            if (label.Term != "UI/Sell Price")
                label.SetTerm("UI/Sell Price");
        }
        if (priceUI.activeSelf != showPrice)
            priceUI.SetActive(showPrice);
    }

    public void UpdateOwnedItemDragHover(UIDrag drag, GameObject hit)
    {
        if (ownedItemDrag != drag) return;
        ownedItemDragHover = hit;
        RefreshOwnedItemDropArea();
    }

    public void UpdateOwnedItemDragPointer(UIDrag drag, PointerEventData pointer)
    {
        if (ownedItemDrag != drag) return;
        ownedItemDragPointer = pointer;
        RefreshOwnedItemDropArea();
    }

    private GameObject SaleAreaAtPointer(PointerEventData pointer)
    {
        Npc merchant = ActiveSaleMerchant();
        if (merchant == null || pointer == null) return null;
        GameObject area = merchant == nowSkillstore ? SkillStoreSellArea : WeaponStoreSellArea;
        if (area == null || !area.activeInHierarchy || !(area.transform is RectTransform rect))
            return null;
        // A full-screen overlay or a child graphic must not hide the visible sale target.
        // Use the same screen-space bounds for preview and completing the sale.
        return RectTransformUtility.RectangleContainsScreenPoint(rect, pointer.position,
            pointer.pressEventCamera) ? area : null;
    }

    public void BeginOwnedItemDrag(UIDrag drag)
    {
        ownedItemDrag = drag;
        ownedItemDragHover = null;
        ownedItemDragPointer = null;
        dragSaleMerchant = ActiveSaleMerchant();
        dragStartedInShop = dragSaleMerchant != null;
        saleItemType = drag.Type;
        saleItemField = drag.ID;
        ValueData data = ValueData.Instance;
        if (drag.Type == TipType.SkillField)
        {
            saleItemId = data.SkillField[drag.ID].ID;
            saleItemLevel = data.SkillField[drag.ID].Level;
        }
        else
        {
            saleItemId = data.WeaponField[drag.ID].ID;
            saleItemLevel = 0;
        }
        RefreshOwnedItemDropArea();
    }

    public void EndOwnedItemDrag(UIDrag drag)
    {
        if (ownedItemDrag != drag) return;
        ownedItemDrag = null;
        ownedItemDragHover = null;
        ownedItemDragPointer = null;
        dragSaleMerchant = null;
        dragStartedInShop = false;
        RefreshOwnedItemDropArea();
    }

    private bool TryGetDraggedSalePrice(out int price)
    {
        price = 0;
        ValueData data = ValueData.Instance;
        if (ownedItemDrag == null || data == null || ownedItemDrag.Type != saleItemType ||
            ownedItemDrag.ID != saleItemField || saleItemId <= 0)
            return false;
        int basePrice;
        if (saleItemType == TipType.SkillField)
        {
            if (saleItemField < 0 || saleItemField >= data.SkillField.Length ||
                saleItemId >= data.Skill.Length || saleItemLevel < 1 ||
                saleItemLevel > SkillLevelTable.MaxLevel ||
                data.SkillField[saleItemField].ID != saleItemId ||
                data.SkillField[saleItemField].Level != saleItemLevel)
                return false;
            basePrice = data.GetSkillBase(saleItemId, saleItemLevel).Price;
        }
        else if (saleItemType == TipType.WeaponField)
        {
            if (!data.IsWeaponFieldUnlocked(saleItemField) || saleItemId >= data.Weapon.Length ||
                data.WeaponField[saleItemField].ID != saleItemId)
                return false;
            basePrice = data.Weapon[saleItemId].Price;
        }
        else return false;
        if (basePrice < 0) return false;
        price = basePrice / 2;
        return true;
    }

    // Handle descendants too, so releasing over the label is the same as the background.
    // A changed/closed merchant cancels a sale instead of turning it into a discard.
    public bool HandleOwnedItemDrop(UIDrag drag, GameObject hit)
    {
        if (ownedItemDrag == drag && ownedItemDragPointer != null)
        {
            GameObject saleArea = SaleAreaAtPointer(ownedItemDragPointer);
            if (saleArea != null)
                hit = saleArea;
            else if (IsDropInside(hit, SkillStoreSellArea) || IsDropInside(hit, WeaponStoreSellArea))
                hit = null;
        }
        bool skillSale = IsDropInside(hit, SkillStoreSellArea);
        bool weaponSale = IsDropInside(hit, WeaponStoreSellArea);
        bool discard = IsDropInside(hit, GarbageCan);
        if (!skillSale && !weaponSale && !discard) return false;
        if (ownedItemDrag != drag) return true;
        if (skillSale || weaponSale)
        {
            Npc merchant = skillSale ? nowSkillstore : nowWeaponstore;
            if (dragStartedInShop && dragSaleMerchant != null && merchant == dragSaleMerchant &&
                ActiveSaleMerchant() == dragSaleMerchant &&
                TryGetDraggedSalePrice(out int price))
                SellDraggedItem(price);
        }
        else if (discard)
        {
            if (drag.Type == TipType.WeaponField)
            {
                ChangeWeapon_ID = 0;
                DoChangeWeapon(drag.ID);
            }
            else if (drag.Type == TipType.SkillField)
            {
                isSpendmoney = false;
                ChangeSkill_ID[0] = 0;
                ChangeSkill_ID[1] = 0;
                DoChangeSkill(drag.ID);
            }
        }
        return true;
    }

    private static bool IsDropInside(GameObject hit, GameObject area)
    {
        return hit != null && area != null && area.activeInHierarchy &&
            hit.transform.IsChildOf(area.transform);
    }

    private void SellDraggedItem(int price)
    {
        ValueData data = ValueData.Instance;
        if (data.money > int.MaxValue - price) return;
        CancelStoreItemSelection();
        isSpendmoney = false;
        if (saleItemType == TipType.SkillField)
        {
            if (Skill.Instance != null) Skill.Instance.StopSunfire(saleItemField);
            // Drop all attached equipment via the ordinary replacement path, without purchasing.
            for (int i = 0; i < 3; i++)
            {
                int equipmentField = saleItemField * 3 + i;
                if (data.WeaponField[equipmentField].ID == 0) continue;
                ChangeWeapon_ID = 0;
                DoChangeWeapon(equipmentField);
            }
            // Clearing the ID first prevents the sold skill from becoming a ground chest.
            data.SkillField[saleItemField].ID = 0;
            data.SkillField[saleItemField].nowCD = 0;
            ChangeSkill_ID[0] = 0;
            ChangeSkill_ID[1] = 0;
            DoChangeSkill(saleItemField);
        }
        else
        {
            data.WeaponField[saleItemField].ID = 0;
            ChangeWeapon_ID = 0;
            DoChangeWeapon(saleItemField);
        }
        // Consume this drag before crediting gold; repeated end/drop events cannot sell twice.
        ownedItemDrag = null;
        ownedItemDragHover = null;
        ownedItemDragPointer = null;
        dragSaleMerchant = null;
        dragStartedInShop = false;
        data.GetMoney(price);
        RefreshOwnedItemDropArea();
    }
}
