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
                skillstore.GetComponent<Npc>().item = new List<int> { 0,1,2,3 };
                return;
            case PrizeBase.Weapon:
                Debug.Log("�Z���ө�");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(0, weaponstorePrefab.transform.position.y, 2.5f), weaponstorePrefab.transform.rotation);
                weaponstore.GetComponent<Npc>().item = new List<int> { 0, 1, 2, 3 };
                return;
        }
    }

    public void NextLevel(int level) {
        //SceneManager.LoadScene(level);
        //ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//�C����s���d���a��m�T�w�����I
        StartCoroutine(LoadSceneWithProgress(level));
    }

    IEnumerator LoadSceneWithProgress(int level)
    {
        // �}�l���B�[������
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);

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
        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//�C����s���d���a��m�T�w�����I
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
    Skill = 3
}