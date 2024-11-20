using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowLevel = 0;//當前關卡編號
    public PrizeBase nowPrize;//當前關卡通關後會獲得的獎勵
    GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy;//剩餘敵人數量

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;

    //確認剩餘敵人
    public void enemycheck() {
        if (!Enemys) 
        {
            Enemys = GameObject.Find("Enemys");
            leftEnemy = Enemys.transform.childCount - 1;//暫用，優化過場後要修復
        }
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
                ValueData.Instance.passiveskillPoint += 1;
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
        SceneManager.LoadScene(level);
        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//每次到新關卡玩家位置固定為原點
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