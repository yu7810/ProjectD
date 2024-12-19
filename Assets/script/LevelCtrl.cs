using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowLevel = 0;//��e���d�s��
    public PrizeBase nowPrize;//��e���d�q����|��o�����y
    public GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy;//�Ѿl�ĤH�ƶq

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;

    //���d��
    public int[][] Level = new int[][] // [����][���dID]
    {
        new int[]{ 0 },
        new int[]{ 1,},
        new int[]{ 2,},
        new int[]{ 3,},
        new int[]{ 4,},
    };

    //�T�{�Ѿl�ĤH
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

    //���
    public void getPrize() {
        switch (nowPrize) {
            case PrizeBase.None:
                Debug.Log("���]�w���y");
                return;
            case PrizeBase.PassivePoin:
                Debug.Log("�ѽ��I");
                ValueData.Instance.passiveskillPoint += 3;
                UICtrl.Instance.passiveskillPoint.text = ValueData.Instance.passiveskillPoint.ToString();
                return;
            case PrizeBase.Skill:
                Debug.Log("�ޯ�ө�");
                GameObject skillstore = Instantiate(skillstorePrefab,new Vector3(0, skillstorePrefab.transform.position.y, 2.5f), skillstorePrefab.transform.rotation);
                List<int> _skillpool = new List<int>();//�Ҧ��ӫ~��
                _skillpool.AddRange(ValueData.Instance.skillstorePool);
                List<int> _skillitem = new List<int>();//�s����X���ӫ~
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
                Debug.Log("�Z���ө�");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(0, weaponstorePrefab.transform.position.y, 2.5f), weaponstorePrefab.transform.rotation);
                List<int> _weaponpool = new List<int>();//�Ҧ��ӫ~��
                _weaponpool.AddRange(ValueData.Instance.weaponstorePool);
                List<int> _weaponitem = new List<int>();//�s����X���ӫ~
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
        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//�C����s���d���a��m�T�w�����I

        // �q���d�������d
        int x = Random.Range(0, Level[level].Length);
        int targetScene = Level[level][x];

        // �}�l���B�[������
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);

        // �b�����[�������e�A���{�ǵ���
        while (!asyncLoad.isDone)
        {
            // �i�H�b�o����ܥ[���i�סA�Ҧp���J�e��
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log("Loading progress: " + (progress * 100) + "%");

            yield return null;
        }
        // �[�����������Y�Ǿާ@
        Debug.Log("�����[�������I");
        nowLevel = level;
        if (level != 0) // �԰����d����ĤH
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

    //��ҹ���
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

//�q�����y������
public enum PrizeBase{ 
    None = 0,
    PassivePoin = 1,
    Weapon = 2,
    Skill = 3,
    Money = 4,
}