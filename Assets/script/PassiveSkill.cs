using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] link; //下層天賦點，此天賦點為true時開放所有link
    public int ID; //該天賦的ID，需與Hierarchy裡附物件的子物件順序相符(不算disable的)，從0開始算
    public Button Btn;
    public Image Img;
    ValueData valuedata;

    private void Awake()
    {
        Btn = this.GetComponent<Button>();
        Img = this.GetComponent<Image>();
        valuedata = GameObject.Find("ValueData").GetComponent<ValueData>();
    }

    public void OnBtn() { //toggle事件，當該天賦點被點擊時
        if (valuedata.PassiveSkills[ID]) //後悔天賦時
        {
            for (int i = 0; i < link.Length; i++)
            {
                if (valuedata.PassiveSkills[link[i].ID]) //下層天賦有任何是已點擊的狀態，則不能後悔此天賦點
                    return;
            }
            valuedata.PassiveSkills[ID] = false; //後悔成功
            for (int i = 0; i < link.Length; i++) //關閉關聯天賦的按鈕
            {
                link[i].Btn.interactable = false;
            }
        }
        else { //取得天賦時
            valuedata.PassiveSkills[ID] = true;
        }
        UICtrl.Instance.UpdatePassiveSkill();
        valuedata.ValueUpdate();
    }

}
