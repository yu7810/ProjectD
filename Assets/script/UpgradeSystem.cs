using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class UpgradeSystem : MonoBehaviour
{
    public GameObject _UICtrl;
    public GameObject Skill_B_1;//放SkillB
    public GameObject Skill_B_2;
    public GameObject Skill_B_3;
    public GameObject Skill_B_4;

#if false
    //強化列表
    public static UpgradeData[] UpgradeList = new UpgradeData[] {
        new UpgradeData(0,"Skill_A",1,1,0), //預設的基礎攻擊
        new UpgradeData(1,"Skill_A_attackUp",0,3,10), //Skill_A攻擊力強化
        new UpgradeData(2,"Skill_A_speedUp",0,3,10),
        new UpgradeData(3,"Skill_A_sizeUp",0,3,10),
        new UpgradeData(4,"Skill_A_colorUp",0,1,10),//just for test
    };
#endif

    [SerializeField] UpgradeDataAsset _dataAsset;//原始資料
    //[SerializeField] UpgradeDataAsset _dataAsset_new;//該局技能池，權重、等級會變動

    public List<UpgradeData> UpgradeList;
    public List<UpgradeData> _UpgradeData => _dataAsset.UpgradeList;

    //強化池，放ID
    public static int[] UpgradePool = new int[10];

    private void Start(){
        for (int i = 0; i < _UpgradeData.Count; i++) {
            //兩種做法 by阿勘
#if fals
            var data = new UpgradeData(_UpgradeData[i]);
            UpgradeList.Add(data);
            UpgradePool[i] = data.ID;
#else
            UpgradeList.Add(_UpgradeData[i].Clone());
            UpgradePool[i] = _UpgradeData[i].Clone().ID;
#endif
        }
    }

    //升級選項
    public int[] UpgradeBtn() {
        int L = 0;
        for (int i = 0; i < UpgradePool.Length; i++) {
            if (UpgradeList[UpgradePool[i]].Weights > 0)
                L++;
        }
        //升級池為空
        if (L <= 0)
            return new int[0];

        int[] _value = new int[] { 0,0,0};
        int FullWeight = 0;
        for (int i = 0; i< UpgradePool.Length; i++) {
            FullWeight += UpgradeList[UpgradePool[i]].Weights;
        }

        //第一個選項
        int Arrow = Random.Range(1, FullWeight+1);
        int CountWeight = 0;//用於壘算權重直至找到對應目標ID
        for (int i = 0; CountWeight < Arrow; i ++) {
            CountWeight += UpgradeList[UpgradePool[i]].Weights;
            _value[0] = UpgradeList[UpgradePool[i]].ID;
        }
        if (L == 1)
            return _value;

        while (true) {
            //第二個選項
            CountWeight = 0;
            Arrow = Random.Range(1, FullWeight + 1);

            for (int i = 0; CountWeight < Arrow; i++){
                CountWeight += UpgradeList[UpgradePool[i]].Weights;
                _value[1] = UpgradeList[UpgradePool[i]].ID;
            }

            if (_value[1] != _value[0])
                break;
        };
        if (L == 2)
            return _value;

        while (true)
        {
            //第三個選項
            CountWeight = 0;
            Arrow = Random.Range(1, FullWeight + 1);

            for (int i = 0; CountWeight < Arrow; i++)
            {
                CountWeight += UpgradeList[UpgradePool[i]].Weights;
                _value[2] = UpgradeList[UpgradePool[i]].ID;
            }

            if (_value[2] != _value[0] && _value[2] != _value[1])
                break;
        };

        return _value;
    }

    //升級
    public void Upgrade(int ID) {
        if (ID == 1)
            Skill_A_atkUp();
        else if (ID == 2)
            Skill_A_speedUp();
        else if (ID == 3)
            Skill_A_sizeUp();
        else if (ID == 5)
            Skill_B();
        else if (ID == 6)
            Skill_B_speedUp();
        else if (ID == 7)
            Skill_B_attackUp();

        UpgradeList[ID].Lv += 1;
        if (UpgradeList[ID].Lv >= UpgradeList[ID].maxLv) {
            //從卡池移除
            UpgradeList[ID].Weights = 0;
        }
    }

    //=================================各技能的升級事件=================================
    public void Skill_A_atkUp() {
        UICtrl.Skill_A_DmgAdd += 0.5f;
    }
    public void Skill_A_speedUp(){
        UICtrl.Skill_A_AttackSpeed += 0.5f;
    }
    public void Skill_A_sizeUp(){
        UICtrl.Skill_A_Size += 0.3f;
    }
    public void Skill_B() {
        if (UpgradeList[5].Lv == 0) {
            Skill_B_1.SetActive(true);
            UpgradeList[6].Weights = 10;
            UpgradeList[7].Weights = 10;
        }
        else if (UpgradeList[5].Lv == 1)
            Skill_B_2.SetActive(true);
        else if (UpgradeList[5].Lv == 2)
            Skill_B_3.SetActive(true);
        else if (UpgradeList[5].Lv == 3)
            Skill_B_4.SetActive(true);
    }
    public void Skill_B_speedUp() {
        UICtrl.Skill_B_AttackSpeed += 0.5f;
        Skill_B_1.GetComponent<SkillRotation>().globalSpeed = 100 * UICtrl.Skill_B_AttackSpeed;
    }
    public void Skill_B_attackUp(){
        UICtrl.Skill_B_DmgAdd += 0.5f;
    }

}

#if false
public class UpgradeData : UpgradeBase
{
    public UpgradeData(int ID, string Name,int Lv,int maxLv,int Weights) { 
        this._Name = Name;
        this._Id = ID;
        this._Lv = Lv;
        this._maxLv = maxLv;
        this._Weights = Weights;
    }

    public string Name { 
        get { return this._Name; }
    }
    public int ID { 
        get { return this._Id; }
    }
    public int Lv { 
        get { return this._Lv; }
        set { this._Lv = value; }
    }
    public int maxLv { 
        get { return this._maxLv; }
    }
    public int Weights { 
        get { return this._Weights; }
        set { this._Weights = value; }
    }
}

public class UpgradeBase
{
    protected string _Name;
    protected int _Id;
    protected int _Lv;
    protected int _maxLv;
    protected int _Weights;//權重
}
#endif