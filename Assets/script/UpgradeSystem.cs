using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UpgradeSystem : MonoBehaviour
{
    public GameObject _UICtrl;

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

    [SerializeField] UpgradeDataAsset _dataAsset;

    public List<UpgradeData> UpgradeList => _dataAsset.UpgradeList;

    //強化池，放ID
    public static int[] UpgradePool = new int[] { 1,2,3,4 };

    private void Start()
    {
        TestBtn();
    }

    public void TestBtn()
    {
        int[] i = UpgradeBtn();
        Debug.Log(i[0] + " & " + i[1]);
    }

    //升級選項
    public int[] UpgradeBtn() {

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

        return _value;
    }

    //升級
    public void Upgrade(int ID) {
        if (ID == 1)
            Skill_A_attackUp();
        UpgradeList[ID].Lv += 1;
        if (UpgradeList[ID].Lv >= UpgradeList[ID].maxLv) { 
            //從卡池移除
        }
    }

    void Skill_A_attackUp() {
        UICtrl.Skill_A_DmgAdd += 0.2f;
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