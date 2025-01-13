using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrl : MonoBehaviour
{
    private static LevelCtrl instance;
    public int nowclass = 0; // ��e���`�s��
    public int nowLevel = 0; // ��e���d�s��
    public PrizeBase nowPrize; // ��e���d�q����|��o�����y
    public GameObject Enemys;
    GameObject ExitDoors;
    public int leftEnemy; // �Ѿl�ĤH�ƶq

    public GameObject skillstorePrefab;
    public GameObject weaponstorePrefab;
    public GameObject itemMoney;
    public GameObject itemPassivepoint;
    public GameObject itemWeapon;
    public GameObject itemSkill;

    //���d��
    public int[][] Level = new int[][] // [����][���dID]
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

        Vector3 p = ValueData.Instance.Player.transform.position;
        p.y = 0;
        Npc npc;

        switch (nowPrize) {
            case PrizeBase.None:
                Debug.Log("���]�w���y");
                return;
            case PrizeBase.PassivePoin:
                Debug.Log("�ѽ��I");
                GameObject _itemPassivepoint = Instantiate(itemPassivepoint, p, itemPassivepoint.transform.rotation);
                npc = _itemPassivepoint.GetComponent<Npc>();
                npc.passivepoint = 2;
                npc.Name = 2 + " �ѽ��I";
                npc.showName();
                return;
            case PrizeBase.Skill:
                Debug.Log("�ޯ�ө�");
                GameObject skillstore = Instantiate(skillstorePrefab,new Vector3(p.x, skillstorePrefab.transform.position.y, p.z), skillstorePrefab.transform.rotation);
                skillstore.GetComponent<Npc>().RandomItem();
                return;
            case PrizeBase.Weapon:
                Debug.Log("�Z���ө�");
                GameObject weaponstore = Instantiate(weaponstorePrefab, new Vector3(p.x, weaponstorePrefab.transform.position.y, p.z), weaponstorePrefab.transform.rotation);
                weaponstore.GetComponent<Npc>().RandomItem();
                return;
            case PrizeBase.Money:
                Debug.Log("����");
                int min = 10 + 12 * nowclass;
                int max = 15 + 12 * nowclass;
                int _money = Random.Range(min, max);
                GameObject _itemMoney =  Instantiate(itemMoney, p, itemMoney.transform.rotation);
                npc = _itemMoney.GetComponent<Npc>();
                npc.money = _money;
                npc.Name = _money + " ����";
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

        //�e���H�X
        Color c = UICtrl.Instance.ScreenMask.color;
        for (float i=0;i<1;i+=0.05f)
        {
            c.a = i;
            UICtrl.Instance.ScreenMask.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        c.a = 1;
        UICtrl.Instance.ScreenMask.color = c;

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
            //float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //Debug.Log("Loading progress: " + (progress * 100) + "%");

            yield return null;
        }
        // �[�����������Y�Ǿާ@
        //Debug.Log("�����[�������I");
        nowLevel = level;
        if (level == 1)
            nowclass += 1;
        if (level != 0) // �԰����d����ĤH
        {
            Enemys = GameObject.Find("Enemys");
            leftEnemy = Enemys.transform.childCount;
        }
        else // �^���I�^���ͩR
        {
            if (ValueData.Instance.HP != ValueData.Instance.maxHP)
                ValueData.Instance.GetHp(ValueData.Instance.maxHP);
        }

        // �C���i���d�^���]�O
        ValueData.Instance.GetAp(ValueData.Instance.maxAP);

        // �ѽ�11
        if(ValueData.Instance.PassiveSkills[11])
        {
            if(ValueData.Instance.maxHP > ValueData.Instance.HP)
            {
                float value = ValueData.Instance.maxHP - ValueData.Instance.HP;
                ValueData.Instance.GetHp(value);
            }
        }

        PlayerCtrl.Instance.canMove = true;

        //�e���H�J
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

    //�ͦ��˳ƹD��
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