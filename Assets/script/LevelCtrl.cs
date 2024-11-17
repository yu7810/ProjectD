using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowLevel = 0;//��e���d�s��
    public PrizeBase nowPrize;//��e���d�q����|��o�����y
    GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy;//�Ѿl�ĤH�ƶq

    //�T�{�Ѿl�ĤH
    public void enemycheck() {
        if (!Enemys) 
        {
            Enemys = GameObject.Find("Enemys");
            leftEnemy = Enemys.transform.childCount - 1;//�ȥΡA�u�ƹL����n�״_
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

    //���
    public void getPrize() {
        switch (nowPrize) {
            case PrizeBase.None:
                Debug.Log("���]�w���y");
                return;
            case PrizeBase.PassivePoin:
                Debug.Log("�ѽ��I");
                ValueData.Instance.passiveskillPoint += 1;
                return;
            case PrizeBase.Skill:
                Debug.Log("�ޯ�");
                return;
            case PrizeBase.Weapon:
                Debug.Log("�Z��");
                return;
        }
    }

    public void NextLevel(int level) {
        SceneManager.LoadScene(level);
        ValueData.Instance.Player.transform.position = new Vector3(0, ValueData.Instance.Player.transform.position.y, 0);//�C����s���d���a��m�T�w�����I
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