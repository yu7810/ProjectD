using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition; // ��l�y��
    private Transform startParent; // ��l������
    public TipType Type;
    public int ID;

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
        if (Type == TipType.WeaponField && ValueData.Instance.WeaponField[ID].ID <= 0) // �ť���줣��즲
            eventData.pointerDrag = null;
        else if (Type == TipType.SkillField && ValueData.Instance.SkillField[ID].ID <= 0) // �ť���줣��즲
            eventData.pointerDrag = null;
        else
        {
            canvasGroup.alpha = 0.7f;  // �즲�ɥb�z��
            canvasGroup.blocksRaycasts = false; // ����즲�L�{���B�ר�L UI ����
            transform.SetParent(UICtrl.Instance.UIDrogParent);
            UICtrl.Instance.GarbageCan.SetActive(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(startParent);
        if(Type == TipType.SkillField && startParent.childCount > 2) // �ޯ���쪺icon�n�bBG��CD����
            transform.SetSiblingIndex(1);
        rectTransform.anchoredPosition = startPosition; // �^�k��l��m

        // �ˬd�ƹ��U�誺 UI ����
        GameObject hoveredObject = eventData.pointerEnter;
        if (hoveredObject != null)
        {
            if(hoveredObject.TryGetComponent<UIDrag>(out UIDrag target))
            {
                if(Type == TipType.WeaponField && target.Type == TipType.WeaponField) // �Z�����
                {
                    int targetWeapinID = ValueData.Instance.WeaponField[target.ID].ID;
                    int myWeapinID = ValueData.Instance.WeaponField[ID].ID;
                    // ������誺
                    UICtrl.Instance.ChangeWeapon_ID = myWeapinID;
                    ValueData.Instance.WeaponField[target.ID].ID = 0; // �קK�ͦ��_�c
                    UICtrl.Instance.DoChangeWeapon(target.ID);
                    // �A���ۤv��
                    UICtrl.Instance.ChangeWeapon_ID = targetWeapinID;
                    ValueData.Instance.WeaponField[ID].ID = 0; // �קK�ͦ��_�c
                    UICtrl.Instance.DoChangeWeapon(ID);
                }
                else if (Type == TipType.SkillField && target.Type == TipType.SkillField) // �ޯ����
                {
                    int targetSkillID = ValueData.Instance.SkillField[target.ID].ID;
                    int targetSkillILv = ValueData.Instance.SkillField[target.ID].Level;
                    int mySkillID = ValueData.Instance.SkillField[ID].ID;
                    int mySkillILv = ValueData.Instance.SkillField[ID].Level;
                    // �ۦP���ޯ�ɯ�
                    if(targetSkillID == mySkillID && targetSkillILv == mySkillILv)
                    {
                        // ������誺
                        UICtrl.Instance.ChangeSkill_ID[0] = mySkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = mySkillILv;
                        UICtrl.Instance.DoChangeSkill(target.ID);
                        // �A���ۤv��
                        UICtrl.Instance.ChangeSkill_ID[0] = 0;
                        UICtrl.Instance.ChangeSkill_ID[1] = 0;
                        ValueData.Instance.SkillField[ID].ID = 0; // �קK�ͦ��_�c
                        UICtrl.Instance.DoChangeSkill(ID);
                    }
                    else // ���P���ޯ���
                    {
                        // ������誺
                        UICtrl.Instance.ChangeSkill_ID[0] = mySkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = mySkillILv;
                        ValueData.Instance.SkillField[target.ID].ID = 0; // �קK�ͦ��_�c
                        UICtrl.Instance.DoChangeSkill(target.ID);
                        // �A���ۤv��
                        UICtrl.Instance.ChangeSkill_ID[0] = targetSkillID;
                        UICtrl.Instance.ChangeSkill_ID[1] = targetSkillILv;
                        ValueData.Instance.SkillField[ID].ID = 0; // �קK�ͦ��_�c
                        UICtrl.Instance.DoChangeSkill(ID);
                    }
                }
                else if (target.Type == TipType.GarbageCan) // ���
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
                Debug.Log("�ؼШS��UIDrag");
        }
        else
            Debug.Log("�S����ؼ�");

        UICtrl.Instance.GarbageCan.SetActive(false);
    }
}
