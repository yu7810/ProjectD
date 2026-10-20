using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition; // 初始座標
    private Transform startParent; // 初始父物件
    public TipType Type;
    public int ID;
    private bool dragging;

    private void Start()
    {
        canvas = UICtrl.Instance.canvas;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Type == TipType.WeaponField && ValueData.Instance.WeaponField[ID].ID <= 0) // 空白欄位不能拖曳
            eventData.pointerDrag = null;
        else if (Type == TipType.SkillField && ValueData.Instance.SkillField[ID].ID <= 0) // 空白欄位不能拖曳
            eventData.pointerDrag = null;
        else
        {
            dragging = true;
            UICtrl.Instance.CancelStoreItemSelection();
            UICtrl.Instance.BeginDragHighlights(Type, ID);
            if (Type == TipType.SkillField)
                UICtrl.Instance.BeginSkillMergePreview(ValueData.Instance.SkillField[ID].ID,
                    ValueData.Instance.SkillField[ID].Level, ID);
            PlayerCtrl.Instance.canAttack = false;
            canvasGroup.alpha = 0.7f;  // 拖曳時半透明
            canvasGroup.blocksRaycasts = false; // 防止拖曳過程中遮擋其他 UI 互動
            transform.SetParent(UICtrl.Instance.UIDrogParent);
            UICtrl.Instance.GarbageCan.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (!dragging)
            return;
        dragging = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        if (startParent != null)
        {
            transform.SetParent(startParent);
            if (Type == TipType.SkillField && startParent.childCount > 2)
                transform.SetSiblingIndex(1);
            rectTransform.anchoredPosition = startPosition;
        }
        if (UICtrl.Instance != null)
        {
            UICtrl.Instance.EndDragHighlights();
            UICtrl.Instance.GarbageCan.SetActive(false);
        }
        if (PlayerCtrl.Instance != null)
            PlayerCtrl.Instance.canAttack = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;
        dragging = false;
        UICtrl.Instance.EndDragHighlights();
        PlayerCtrl.Instance.canAttack = true;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(startParent);
        if(Type == TipType.SkillField && startParent.childCount > 2) // 技能欄位的icon要在BG跟CD中間
            transform.SetSiblingIndex(1);
        rectTransform.anchoredPosition = startPosition; // 回歸初始位置

        // 檢查滑鼠下方的 UI 物件
        GameObject hoveredObject = eventData.pointerEnter;
        if (Type == TipType.SkillField && hoveredObject != null &&
            UICtrl.Instance.TryGetPurchaseDropField(hoveredObject, TipType.Skill, out int skillField))
            hoveredObject = UICtrl.Instance.SkillFieldIcon[skillField].gameObject;
        if (hoveredObject != null)
        {
            if(hoveredObject.TryGetComponent<UIDrag>(out UIDrag target) && target != this)
            {
                if(Type == TipType.WeaponField && target.Type == TipType.WeaponField) // 武器欄位
                {
                    int targetWeapinID = ValueData.Instance.WeaponField[target.ID].ID;
                    int myWeapinID = ValueData.Instance.WeaponField[ID].ID;
                    // 先換對方的
                    UICtrl.Instance.ChangeWeapon_ID = myWeapinID;
                    ValueData.Instance.WeaponField[target.ID].ID = 0; // 避免生成寶箱
                    UICtrl.Instance.DoChangeWeapon(target.ID);
                    // 再換自己的
                    UICtrl.Instance.ChangeWeapon_ID = targetWeapinID;
                    ValueData.Instance.WeaponField[ID].ID = 0; // 避免生成寶箱
                    UICtrl.Instance.DoChangeWeapon(ID);
                }
                else if (Type == TipType.SkillField && target.Type == TipType.SkillField) // 技能欄位
                {
                    int targetSkillID = ValueData.Instance.SkillField[target.ID].ID;
                    int targetSkillILv = ValueData.Instance.SkillField[target.ID].Level;
                    int mySkillID = ValueData.Instance.SkillField[ID].ID;
                    int mySkillILv = ValueData.Instance.SkillField[ID].Level;
                    // 相同的技能升級
                    if (UICtrl.CanMergeSkills(mySkillID, mySkillILv, targetSkillID, targetSkillILv))
                    {
                        // 先換對方的
                        UICtrl.Instance.ChangeSkill_ID[0] = mySkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = mySkillILv;
                        UICtrl.Instance.DoChangeSkill(target.ID);
                        // 再換自己的
                        UICtrl.Instance.ChangeSkill_ID[0] = 0;
                        UICtrl.Instance.ChangeSkill_ID[1] = 0;
                        ValueData.Instance.SkillField[ID].ID = 0; // 避免生成寶箱
                        UICtrl.Instance.DoChangeSkill(ID);
                    }
                    else // 不同的技能對調
                    {
                        // 先換對方的
                        UICtrl.Instance.ChangeSkill_ID[0] = mySkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = mySkillILv;
                        ValueData.Instance.SkillField[target.ID].ID = 0; // 避免生成寶箱
                        UICtrl.Instance.DoChangeSkill(target.ID);
                        // 再換自己的
                        UICtrl.Instance.ChangeSkill_ID[0] = targetSkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = targetSkillILv;
                        ValueData.Instance.SkillField[ID].ID = 0; // 避免生成寶箱
                        UICtrl.Instance.DoChangeSkill(ID);
                    }
                }
                else if (target.Type == TipType.GarbageCan) // 丟棄
                {
                    if(Type == TipType.WeaponField)
                    {
                        UICtrl.Instance.ChangeWeapon_ID = 0;
                        UICtrl.Instance.DoChangeWeapon(ID);
                    }
                    else if (Type == TipType.SkillField)
                    {
                        UICtrl.Instance.ChangeSkill_ID[0] = 0;
                        UICtrl.Instance.ChangeSkill_ID[1] = 0;
                        UICtrl.Instance.DoChangeSkill(ID);
                    }
                }
            }
            else
                Debug.Log("目標沒有UIDrag");
        }
        else
            Debug.Log("沒指到目標");

        UICtrl.Instance.GarbageCan.SetActive(false);
    }
}
