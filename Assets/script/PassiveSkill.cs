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
                bool canRemove = false;
                if (down[i].top.Length == 1 && ValueData.Instance.PassiveSkills[down[i].ID])//�Y����U�h���W�h�u���ڡA�h����ᮬ��
                {
                    Debug.Log(down[i].ID + "���W�h�u����");
                    return;
                }
                else if (down.Length == 1 && down[i].top.Length == 0) // ��ڪ��U�h�u��1�ӥB���O��l�I
                    canRemove = true;
                for (int x = 0; x< down[i].top.Length; x++) {
                    bool a = ValueData.Instance.PassiveSkills[down[i].top[x].ID];//�ڪ��U�h�����N�W�h�ѽ�O�_���I
                    if(a)
                        Debug.Log(down[i].top[x].ID + "�����N�W�h�ѽᦳ�I");
                    if (down[i].top[x].ID != ID && a && top.Length>0) //�Y�U�h�����N�W�h���F�ڥH�~������w�I�ѽ�A�B�ڤ�����l�I
                    {
                        canRemove = true;
                        Debug.Log("can remove");
                    }
                }
                if (!canRemove && down[i].top.Length == 0) // �Y�U�h����l�I
                    canRemove = true;
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
