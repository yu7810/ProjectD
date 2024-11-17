using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveSkill : MonoBehaviour
{
    public PassiveSkill[] top; //上層天賦點
    public PassiveSkill[] down;//下層天賦點
    public int ID; //該天賦的ID，需與Hierarchy裡附物件的子物件順序相符(不算disable的)，從0開始算
    public Button Btn;
    public Image Img;

    private void Awake()
    {
        Btn = this.GetComponent<Button>();
        Img = this.GetComponent<Image>();
    }

    public void OnBtn() { //toggle事件，當該天賦點被點擊時
        if (ValueData.Instance.PassiveSkills[ID]) //後悔天賦時
        {
            for (int i = 0; i < down.Length; i++)
            {
                if (down[i].top.Length <= 1 && ValueData.Instance.PassiveSkills[down[i].ID])//若任何下層的上層只有我，則不能後悔我
                {
                    return;
                }
                bool canRemove = false;
                for (int x = 0; x< down[i].top.Length; x++) {
                    bool a = ValueData.Instance.PassiveSkills[down[i].top[x].ID];//我的下層的任意上層天賦是否有點
                    if (down[i].top[x].ID != ID && a) //若下層的任意上層除了我以外有任何已點天賦
                    {
                        canRemove = true;
                    }
                }
                if (!canRemove && ValueData.Instance.PassiveSkills[down[i].ID])
                    return;
            }
            ValueData.Instance.passiveskillPoint += 1;
            ValueData.Instance.PassiveSkills[ID] = false; //後悔成功
            for (int i = 0; i < down.Length; i++) //關閉下層天賦的按鈕
            {
                down[i].Btn.interactable = false;
            }
        }
        else { //取得天賦時
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
