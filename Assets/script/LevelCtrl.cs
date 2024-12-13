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
                skillstore.GetComponent<Npc>().item = new List<int> { 0,1,2,3 };
                return;
            case PrizeBase.Weapon:
                Debug.Log("武器商店");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(0, weaponstorePrefab.transform.position.y, 2.5f), weaponstorePrefab.transform.rotation);
                weaponstore.GetComponent<Npc>().item = new List<int> { 0, 1, 2, 3 };
                return;
        }
    }

    public void NextLevel(int level) {
        //SceneManager.LoadScene(level);
        //ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//每次到新關卡玩家位置固定為原點
        StartCoroutine(LoadSceneWithProgress(level));
    }

    IEnumerator LoadSceneWithProgress(int level)
    {
        // 開始異步加載場景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);

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
        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//每次到新關卡玩家位置固定為原點
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
    Skill = 3
}