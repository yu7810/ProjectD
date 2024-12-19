using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowLevel = 0;//當前關卡編號
    public PrizeBase nowPrize;//當前關卡通關後會獲得的獎勵
    public GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy;//剩餘敵人數量

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;

    //關卡池
    public int[][] Level = new int[][] // [等級][關卡ID]
    {
        new int[]{ 0 },
        new int[]{ 1,},
        new int[]{ 2,},
        new int[]{ 3,},
        new int[]{ 4,},
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
        switch (nowPrize) {
            case PrizeBase.None:
                Debug.Log("未設定獎勵");
                return;
            case PrizeBase.PassivePoin:
                Debug.Log("天賦點");
                ValueData.Instance.passiveskillPoint += 3;
                UICtrl.Instance.passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
                return;
            case PrizeBase.Skill:
                Debug.Log("技能商店");
                GameObject skillstore = Instantiate(skillstorePrefab,new Vector3(0, skillstorePrefab.transform.position.y, 2.5f), skillstorePrefab.transform.rotation);
                List<int> _skillpool = new List<int>();//所有商品池
                _skillpool.AddRange(ValueData.Instance.skillstorePool);
                List<int> _skillitem = new List<int>();//存放取出的商品
                for (int i = 0; i < 3; i++)
                {
                    int randomIndex = Random.Range(0, _skillpool.Count);
                    _skillitem.Add(_skillpool[randomIndex]);
                    _skillpool.RemoveAt(randomIndex);
                }
                skillstore.GetComponent<Npc>().item.Clear();
                skillstore.GetComponent<Npc>().item.AddRange(_skillitem);
                return;
            case PrizeBase.Weapon:
                Debug.Log("武器商店");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(0, weaponstorePrefab.transform.position.y, 2.5f), weaponstorePrefab.transform.rotation);
                List<int> _weaponpool = new List<int>();//所有商品池
                _weaponpool.AddRange(ValueData.Instance.weaponstorePool);
                List<int> _weaponitem = new List<int>();//存放取出的商品
                for (int i = 0; i < 5; i++)
                {
                    int randomIndex = Random.Range(0, _weaponpool.Count);
                    _weaponitem.Add(_weaponpool[randomIndex]);
                    _weaponpool.RemoveAt(randomIndex);
                }
                weaponstore.GetComponent<Npc>().item.Clear();
                weaponstore.GetComponent<Npc>().item.AddRange(_weaponitem);
                return;
            case PrizeBase.Money:
                int min = 10 * nowLevel;
                int max = 5 + 11 * nowLevel;
                DropMoney(min,max, ValueData.Instance.Player.transform.position, 1f);
                break;
        }
    }

    public void NextLevel(int level) {
        StartCoroutine(LoadSceneWithProgress(level));
    }

    IEnumerator LoadSceneWithProgress(int level)
    {
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
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log("Loading progress: " + (progress * 100) + "%");

            yield return null;
        }
        // 加載完成後執行某些操作
        Debug.Log("場景加載完成！");
        nowLevel = level;
        if (level != 0) // 戰鬥關卡抓取敵人
        {
            Enemys = GameObject.Find("Enemys");
            leftEnemy = Enemys.transform.childCount;
        }
        else
        {
            if (ValueData.Instance.HP <= 0)
                ValueData.Instance.GetHp(ValueData.Instance.maxHP);
        }
        ValueData.Instance.GetAp(ValueData.Instance.maxAP);
    }

    public void DropMoney(int min, int max, Vector3 pos, float offset)
    {
        StartCoroutine(dropMoney(min, max+1, pos, offset));
    }

    IEnumerator dropMoney(int min, int max, Vector3 pos, float offset)
    {
        int _money = Random.Range(min, max);
        GameObject money = ValueData.Instance.moneyPrefab;
        for (int i = 0; i < _money; i++)
        {
            Vector3 _offset = new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
            Vector3 _pos = pos + _offset;
            _pos.y = 0;
            Instantiate(money, _pos, money.transform.rotation);
            yield return new WaitForSeconds(0.1f);
        }
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