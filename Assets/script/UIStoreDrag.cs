using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Store entries keep their place in the layout while an icon follows the pointer.
public class UIStoreDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TipType type;
    private int itemId;
    private int level;
    private int itemIndex;
    private bool costsMoney;
    private Npc store;
    private bool dragging;
    private bool previousCanAttack;
    private CanvasGroup group;
    private float previousAlpha;
    private bool previousBlocksRaycasts;
    private Image dragIcon;
    private readonly List<RaycastResult> hits = new List<RaycastResult>();

    public void Configure(TipType itemType, int id, int itemLevel, int index, bool paid)
    {
        type = itemType;
        itemId = id;
        level = itemLevel;
        itemIndex = index;
        costsMoney = paid;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        eventData.eligibleForClick = false;
        UICtrl ui = UICtrl.Instance;
        store = type == TipType.Skill ? ui.nowSkillstore : ui.nowWeaponstore;
        if (!ui.CanPurchaseStoreItem(type, itemId, level, itemIndex, costsMoney, store))
        {
            eventData.pointerDrag = null;
            return;
        }

        ui.CancelStoreItemSelection();
        if (type == TipType.Skill)
            ui.ChangeSkill(itemId, level, costsMoney, itemIndex);
        else
            ui.ChangeWeapon(itemId, costsMoney, itemIndex);

        ui.BeginDragHighlights(type);
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        previousAlpha = group.alpha;
        previousBlocksRaycasts = group.blocksRaycasts;
        group.alpha = 0.5f;
        group.blocksRaycasts = false;
        previousCanAttack = PlayerCtrl.Instance.canAttack;
        PlayerCtrl.Instance.canAttack = false;
        dragging = true;

        Image sourceIcon = transform.Find("Icon").GetComponent<Image>();
        GameObject ghost = new GameObject("Dragged store item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dragIcon = ghost.GetComponent<Image>();
        dragIcon.transform.SetParent(ui.UIDrogParent, false);
        dragIcon.sprite = sourceIcon.sprite;
        dragIcon.color = new Color(1f, 1f, 1f, 0.8f);
        dragIcon.raycastTarget = false;
        dragIcon.preserveAspect = true;
        dragIcon.rectTransform.sizeDelta = sourceIcon.rectTransform.rect.size;
        Vector3 scale = sourceIcon.transform.lossyScale;
        Vector3 parentScale = ui.UIDrogParent.lossyScale;
        dragIcon.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, 1f);
        MoveIcon(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
            MoveIcon(eventData);
    }

    private void MoveIcon(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            UICtrl.Instance.UIDrogParent as RectTransform, eventData.position,
            eventData.pressEventCamera, out Vector3 position))
            dragIcon.rectTransform.position = position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;

        UICtrl ui = UICtrl.Instance;
        hits.Clear();
        EventSystem.current.RaycastAll(eventData, hits);
        int field = -1;
        bool valid = hits.Count > 0 && ui.TryGetPurchaseDropField(hits[0].gameObject, type, out field);
        FinishDrag();
        if (valid)
            ui.PurchaseDraggedStoreItem(type, itemId, level, itemIndex, costsMoney, store, field);
    }

    private void FinishDrag()
    {
        dragging = false;
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
        if (group != null)
        {
            group.alpha = previousAlpha;
            group.blocksRaycasts = previousBlocksRaycasts;
        }
        if (PlayerCtrl.Instance != null)
            PlayerCtrl.Instance.canAttack = previousCanAttack;
        if (UICtrl.Instance != null)
        {
            UICtrl.Instance.EndDragHighlights();
            UICtrl.Instance.CancelStoreItemSelection();
        }
    }

    private void OnDisable()
    {
        if (dragging)
            FinishDrag();
    }
}
