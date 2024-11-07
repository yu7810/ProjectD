using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] link; //�U�h�ѽ��I�A���ѽ��I��true�ɶ}��Ҧ�link
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
            for (int i = 0; i < link.Length; i++)
            {
                if (ValueData.Instance.PassiveSkills[link[i].ID]) //�U�h�ѽᦳ����O�w�I�������A�A�h����ᮬ���ѽ��I
                    return;
            }
            ValueData.Instance.PassiveSkills[ID] = false; //�ᮬ���\
            for (int i = 0; i < link.Length; i++) //�������p�ѽ᪺���s
            {
                link[i].Btn.interactable = false;
            }
        }
        else { //���o�ѽ��
            ValueData.Instance.PassiveSkills[ID] = true;
        }
        UICtrl.Instance.UpdatePassiveSkill();
        ValueData.Instance.PlayerValueUpdate();
        ValueData.Instance.SkillFieldValueUpdate();
    }

}
