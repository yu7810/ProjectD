using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowclass = 0; // 當前章節編號
    public int nowLevel = 0; // 當前關卡編號
    public PrizeBase nowPrize; // 當前關卡通關後會獲得的獎勵
    public GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy; // 剩餘敵人數量

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;
    public GameObject itemMoney;
    public GameObject itemPassivepoint;
    public GameObject itemWeapon;
    public GameObject itemSkill;

    //關卡池
    public int[][] Level = new int[][] // [等級][關卡ID]
    {
        new int[]{ 0 },
        new int[]{ 1,},
        new int[]{ 2,},
        new int[]{ 3,},
        new int[]{ 4,},
        new int[]{ 5,},
        new int[]{ 6,},
        new int[]{ 7,},
    };

    //確認剩餘敵人
    public void enemycheck() {
        if (leftEnemy > 0)
            return;
        getPrize();
        ExitDoors = GameObject.Find("ExitDoors");
        foreach (Transform child in ExitDoors.transform) 
        {
            child.gameObject.SetActive(true);
        }
    }

    //領獎
    public void getPrize() {

        Vector3 p = ValueData.Instance.Player.transform.position;
        p.y = 0;
        Npc npc;

        switch (nowPrize) {
            case PrizeBase.None:
                Debug.Log("未設定獎勵");
                return;
            case PrizeBase.PassivePoin:
                Debug.Log("天賦點");
                GameObject _itemPassivepoint = Instantiate(itemPassivepoint, p, itemPassivepoint.transform.rotation);
                npc = _itemPassivepoint.GetComponent<Npc>();
                npc.passivepoint = 2;
                npc.Name = 2 + " 天賦點";
                npc.showName();
                return;
            case PrizeBase.Skill:
                Debug.Log("技能商店");
                GameObject skillstore = Instantiate(skillstorePrefab,new Vector3(p.x, skillstorePrefab.transform.position.y, p.z), skillstorePrefab.transform.rotation);
                skillstore.GetComponent<Npc>().RandomItem();
                return;
            case PrizeBase.Weapon:
                Debug.Log("武器商店");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(p.x, weaponstorePrefab.transform.position.y, p.z), weaponstorePrefab.transform.rotation);
                weaponstore.GetComponent<Npc>().RandomItem();
                return;
            case PrizeBase.Money:
                Debug.Log("金幣");
                int min = 10 + 12 * nowclass;
                int max = 15 + 12 * nowclass;
                int _money = Random.Range(min, max);
                GameObject _itemMoney =  Instantiate(itemMoney, p, itemMoney.transform.rotation);
                npc = _itemMoney.GetComponent<Npc>();
                npc.money = _money;
                npc.Name = _money + " 金幣";
                npc.showName();
                break;
        }
    }

    public void NextLevel(int level) {
        StartCoroutine(LoadSceneWithProgress(level));
    }

    IEnumerator LoadSceneWithProgress(int level)
    {
        PlayerCtrl.Instance.canMove = false;

        //畫面淡出
        Color c = UICtrl.Instance.ScreenMask.color;
        for (float i=0;i<1;i+=0.05f)
        {
            c.a = i;
            UICtrl.Instance.ScreenMask.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        c.a = 1;
        UICtrl.Instance.ScreenMask.color = c;

        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//每次到新關卡玩家位置固定為原點

        // 從關卡池抽關卡
        int x = Random.Range(0, Level[level].Length);
        int targetScene = Level[level][x];

        // 開始異步加載場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);

        // 在場景加載完成前，讓程序等待
        while (!asyncLoad.isDone)
        {
            // 可以在這裡顯示加載進度，例如載入畫面
            //float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //Debug.Log("Loading progress: " + (progress * 100) + "%");

            yield return null;
        }
        // 加載完成後執行某些操作
        //Debug.Log("場景加載完成！");
        nowLevel = level;
        if (level == 1)
            nowclass += 1;
        if (level != 0) // 戰鬥關卡抓取敵人
        {
            Enemys = GameObject.Find("Enemys");
            leftEnemy = Enemys.transform.childCount;
        }
        else // 回據點回滿生命
        {
            if (ValueData.Instance.HP != ValueData.Instance.maxHP)
                ValueData.Instance.GetHp(ValueData.Instance.maxHP);
        }

        // 每次進關卡回滿魔力
        ValueData.Instance.GetAp(ValueData.Instance.maxAP);

        // 天賦11
        if(ValueData.Instance.PassiveSkills[11])
        {
            if(ValueData.Instance.maxHP > ValueData.Instance.HP)
            {
                float value = ValueData.Instance.maxHP - ValueData.Instance.HP;
                ValueData.Instance.GetHp(value);
            }
        }

        PlayerCtrl.Instance.canMove = true;

        //畫面淡入
        for (float i = 1; i >= 0; i-=0.05f)
        {
            c.a = i;
            UICtrl.Instance.ScreenMask.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        c.a = 0;
        UICtrl.Instance.ScreenMask.color = c;
    }

    public void DropMoney(int value, Vector3 pos, float offset)
    {
        StartCoroutine(dropMoney(value, pos, offset));
    }

    IEnumerator dropMoney(int value, Vector3 pos, float offset)
    {
        GameObject money = ValueData.Instance.moneyPrefab;
        for (int i = 0; i < value; i++)
        {
            Vector3 _offset = new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
            Vector3 _pos = pos + _offset;
            _pos.y = 0;
            Instantiate(money, _pos, money.transform.rotation);
            yield return new WaitForSeconds(0.05f);
        }
    }

    //生成裝備道具
    public void doItemweapon(int ID)
    {
        Vector3 p = ValueData.Instance.Player.transform.position;
        p.y = 0;
        GameObject item = Instantiate(itemWeapon, p, itemWeapon.transform.rotation);
        Npc npc = item.GetComponent<Npc>();
        npc.item.Add(ID);
        npc.Name = ValueData.Instance.Weapon[ID].Name;
        npc.showName();
    }
    public void doItemskill(int ID, int Lv)
    {
        Vector3 p = ValueData.Instance.Player.transform.position;
        p.y = 0;
        GameObject item = Instantiate(itemSkill, p, itemSkill.transform.rotation);
        Npc npc = item.GetComponent<Npc>();
        npc.item.Add(ID);
        npc.itemlevel.Add(Lv);
        npc.Name = ValueData.Instance.Skill[ID].Name + " Lv." + Lv;
        npc.showName();
    }

    //單例實體
    public static LevelCtrl Instance
    {
        get => instance;
    }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

}

//通關獎勵的類型
public enum PrizeBase{ 
    None = 0,
    PassivePoin = 1,
    Weapon = 2,
    Skill = 3,
    Money = 4,
}