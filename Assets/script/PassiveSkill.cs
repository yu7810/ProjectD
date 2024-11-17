using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] top; //�W�h�ѽ��I
    public PassiveSkill[] down;//�U�h�ѽ��I
    public int ID; //�Ӥѽ᪺ID�A�ݻPHierarchy�̪����󪺤l���󶶧Ǭ۲�(����disable��)�A�q0�}�l��
    public Button Btn;
    public Image Img;

    private void Awake()
    {
        Btn = this.GetComponent<Button>();
        Img = this.GetComponent<Image>();
    }

    public void OnBtn() { //toggle�ƥ�A��Ӥѽ��I�Q�I����
        if (ValueData.Instance.PassiveSkills[ID]) //�ᮬ�ѽ��
        {
            for (int i = 0; i < down.Length; i++)
            {
                if (down[i].top.Length <= 1 && ValueData.Instance.PassiveSkills[down[i].ID])//�Y����U�h���W�h�u���ڡA�h����ᮬ��
                {
                    return;
                }
                bool canRemove = false;
                for (int x = 0; x< down[i].top.Length; x++) {
                    bool a = ValueData.Instance.PassiveSkills[down[i].top[x].ID];//�ڪ��U�h�����N�W�h�ѽ�O�_���I
                    if (down[i].top[x].ID != ID && a) //�Y�U�h�����N�W�h���F�ڥH�~������w�I�ѽ�
                    {
                        canRemove = true;
                    }
                }
                if (!canRemove && ValueData.Instance.PassiveSkills[down[i].ID])
                    return;
            }
            ValueData.Instance.passiveskillPoint += 1;
            ValueData.Instance.PassiveSkills[ID] = false; //�ᮬ���\
            for (int i = 0; i < down.Length; i++) //�����U�h�ѽ᪺���s
            {
                down[i].Btn.interactable = false;
            }
        }
        else { //���o�ѽ��
            if (ValueData.Instance.passiveskillPoint <= 0)
                return;
            ValueData.Instance.passiveskillPoint -= 1;
            ValueData.Instance.PassiveSkills[ID] = true;
        }
        UICtrl.Instance.UpdatePassiveSkill();
        ValueData.Instance.PlayerValueUpdate();
        ValueData.Instance.SkillFieldValueUpdate();
    }

}
