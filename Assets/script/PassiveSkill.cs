using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] link; //下層天賦點，此天賦點為true時開放所有link
    public int ID; //該天賦的ID，需與Hierarchy裡附物件的子物件順序相符(不算disable的)，從0開始算
    public Toggle Btn;
    ValueData valuedata;
    UICtrl uictrl;
    bool isHandlingEvent = false; // 防止toggle重複觸發

    private void Start()
    {
        Btn = this.GetComponent<Toggle>();
        valuedata = GameObject.Find("ValueData").GetComponent<ValueData>();
        uictrl = GameObject.Find("UICtrl").GetComponent<UICtrl>();
    }

    public void OnToggle() { //toggle事件，當該天賦點被點擊時
        if (isHandlingEvent)
            return;
        isHandlingEvent = true;
        if (!Btn.isOn) //後悔天賦時
        {
            for (int i = 0; i < link.Length; i++)
            {
                if (valuedata.PassiveSkills[link[i].ID]) //下層天賦有任何是已點擊的狀態，則不能後悔此天賦點
                {
                    Btn.isOn = true;
                    return;
                }
            }
            valuedata.PassiveSkills[ID] = false; //後悔成功
            for (int i = 0; i < link.Length; i++)
            {
                link[i].Btn.interactable = false;
                Debug.Log(link[i].ID + "關閉");
            }
        }
        else { //取得天賦時
            valuedata.PassiveSkills[ID] = true;
        }
        uictrl.UpdatePassiveSkill();
        isHandlingEvent = false;
    }

}
