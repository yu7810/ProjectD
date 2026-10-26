using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using I2.Loc;

public partial class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int CurrentLevelIndex { get; private set; } = 0;
    public int nowclass = 0; // 當前章節編號
    public int nowLevel = 0; // 當前關卡編號
    public GameObject Enemys;
    private readonly HashSet<Enemy> requiredEnemies = new HashSet<Enemy>();
    private bool stageCompleted = true;
    private bool isLoadingLevel;
    public int leftEnemy; // 剩餘敵人數量

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;
    public GameObject itemMoney;
    public GameObject itemPassivepoint;
    public GameObject itemPassiveTree;
    public GameObject itemLife;
    public GameObject itemWeapon;
    public GameObject itemSkill;
    public CanvasGroup ScreenMask;
    public TextMeshProUGUI Progress;

    // 用來招喚的怪物
    public GameObject EnemyHerring;

    private void ClearStageEnemies()
    {
        stageCompleted = true;
        requiredEnemies.Clear();
        leftEnemy = 0;
    }

    private void InitializeStageEnemies()
    {
        requiredEnemies.Clear();
        if (Enemys != null)
        {
            foreach (Enemy enemy in Enemys.GetComponentsInChildren<Enemy>())
            {
                if (enemy.enemyType != EnemyType.Barrel && enemy.enemyRarity != EnemyRarity.Minion)
                    requiredEnemies.Add(enemy);
            }
        }
        leftEnemy = requiredEnemies.Count;
        stageCompleted = false;
    }

    public void enemycheck(Enemy enemy)
    {
        if (stageCompleted || enemy == null || !requiredEnemies.Remove(enemy))
            return;

        leftEnemy = requiredEnemies.Count;
        if (leftEnemy > 0)
            return;

        CompleteStage();
    }

    //領獎
    public void getPrize()
    {
        if (CurrentRoom == null || !CurrentRoom.GrantsCompletionRewards || !CurrentRoom.HasRewards)
            return;
        Vector3 position = ValueData.Instance.Player.transform.position;
        position.y = 0;
        DropConfiguredRewards(position);
    }

    public void ResetRun()
    {
        StopAllCoroutines();
        isLoadingLevel = false;
        ClearStageEnemies();
        if (Enemys)
            Enemys.SetActive(false);
        Enemys = null;
        leftEnemy = 0;
        CurrentRoom = null;
        offeredDoors.Clear();
        if (generatedDoors != null) Destroy(generatedDoors.gameObject);
        generatedDoors = null;
        doorAnchors.Clear();
    }

    public void NextLevel(int level)
    {
        TryLoadStage(level, null, false);
    }

    IEnumerator LoadSceneWithProgress(int level, StageRoomSelection room)
    {
        ClearStageEnemies();
        if(PlayerCtrl.Instance)
            PlayerCtrl.Instance.canMove = false;

        //畫面淡出
        ScreenMask.blocksRaycasts = true;
        Progress.text = "0 %";
        for (float i=0;i<1;i+=0.05f)
        {
            ScreenMask.alpha = i;
            yield return new WaitForSeconds(0.01f);
        }
        ScreenMask.alpha = 1;

        string targetScene = level == -1 ? "Title" : room.sceneName;
        if (level == -1 && DontDestroy.instance)
            Destroy(DontDestroy.instance.gameObject);
        if (level >= 0 && PlayerCtrl.Instance)
            PlayerCtrl.Instance.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);
        // Merchants initialize during scene activation, so set depth before loading.
        SetProgress(level);
        CurrentRoom = room;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);

        // 在場景加載完成前，讓程序等待
        while (!asyncLoad.isDone)
        {
            // 加載進度
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Progress.text = (progress * 100).ToString("0") + " %";

            yield return null;
        }
        // 加載完成後執行某些操作
        //Debug.Log("場景加載完成！");
        if (level > 0)
        {
            PrepareRoom();
            if (CurrentRoom.kind == StageRoomKind.Boss)
                UICtrl.Instance.classUI.text = "Boss " + nowLevel;
            else
                UICtrl.Instance.classUI.text = "Class " + nowLevel + "-" + nowclass;
        }
        else if (level == 0)
        {
            PrepareRoom();
            if (ValueData.Instance.HP != ValueData.Instance.maxHP)
                ValueData.Instance.GetHp(ValueData.Instance.maxHP);

            LocalizationManager.TryGetTranslation("UI/Home", out string classname);
            UICtrl.Instance.classUI.text = classname;
        }

        // 每次進關卡回滿魔力
        ValueData.Instance.GetAp(ValueData.Instance.maxAP);

        PlayerCtrl.Instance.canMove = true;
        ValueData.Instance.immortal = false;

        //畫面淡入
        for (float i = 1; i >= 0; i-=0.05f)
        {
            ScreenMask.alpha = i;
            yield return new WaitForSeconds(0.01f);
        }
        ScreenMask.alpha = 0;
        ScreenMask.blocksRaycasts = false;
        isLoadingLevel = false;
    }

    public void DropMoney(int value, Vector3 pos, float offset)
    {
        StartCoroutine(dropMoney(value, pos, offset));
    }

    IEnumerator dropMoney(int value, Vector3 pos, float offset)
    {
        GameObject money = ValueData.Instance.moneyPrefab;

        // 每個金幣間的掉落延遲時間
        float _delay = 1f / value;
        if (_delay < 0.01f)
            _delay = 0.01f;
        else if (_delay > 0.1f)
            _delay = 0.1f;

        for (int i = 0; i < value; i++)
        {
            Vector3 _offset = new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
            Vector3 _pos = pos + _offset;
            _pos.y = 0;
            Instantiate(money, _pos, money.transform.rotation);
            yield return new WaitForSeconds(_delay);
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
    RandomSkill = 5,
}