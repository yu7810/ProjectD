using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] link; //�U�h�ѽ��I�A���ѽ��I��true�ɶ}��Ҧ�link
    public int ID; //�Ӥѽ᪺ID�A�ݻPHierarchy�̪����󪺤l���󶶧Ǭ۲�(����disable��)�A�q0�}�l��
    public Toggle Btn;
    ValueData valuedata;
    UICtrl uictrl;
    bool isHandlingEvent = false; // ����toggle����Ĳ�o

    private void Start()
    {
        Btn = this.GetComponent<Toggle>();
        valuedata = GameObject.Find("ValueData").GetComponent<ValueData>();
        uictrl = GameObject.Find("UICtrl").GetComponent<UICtrl>();
    }

    public void OnToggle() { //toggle�ƥ�A��Ӥѽ��I�Q�I����
        if (isHandlingEvent)
            return;
        isHandlingEvent = true;
        if (!Btn.isOn) //�ᮬ�ѽ��
        {
            for (int i = 0; i < link.Length; i++)
            {
                if (valuedata.PassiveSkills[link[i].ID]) //�U�h�ѽᦳ����O�w�I�������A�A�h����ᮬ���ѽ��I
                {
                    Btn.isOn = true;
                    return;
                }
            }
            valuedata.PassiveSkills[ID] = false; //�ᮬ���\
            for (int i = 0; i < link.Length; i++)
            {
                link[i].Btn.interactable = false;
                Debug.Log(link[i].ID + "����");
            }
        }
        else { //���o�ѽ��
            valuedata.PassiveSkills[ID] = true;
        }
        uictrl.UpdatePassiveSkill();
        isHandlingEvent = false;
    }

}
