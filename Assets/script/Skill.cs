using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    private static Skill instance;
    Rigidbody m_Rigidbody;
    public GameObject Skill_A;
    public GameObject Skill_Bell;
    public GameObject Skill_Bellattack;
    public GameObject Skill_Magicarrow;
    public GameObject Skill_Waterball_1;
    public GameObject Skill_Waterball_2;
    public ParticleSystem SmokeTrail;
    LayerMask maskFloor = (1 << 7);
    Vector3 startPos = new Vector3(0, 0, 0); // 技能發射位置
    Quaternion startRot = new Quaternion(0, 0, 0,0);
    int UsedTime = 0; // 紀錄技能連續使用次數

    void Awake()
    {
        // 如果實例尚未存在，設置此物件為唯一實例並保留在場景切換中
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已經存在一個實例，銷毀當前物件
            Destroy(gameObject);
        }
    }
    public static Skill Instance
    {
        get => instance;
    }
    private void Start()
    {
        m_Rigidbody = ValueData.Instance.Player.GetComponent<Rigidbody>();
    }

    public void UseSkill(int Skillid, int Fieldid, Vector3 Startpos = default, Quaternion Startrot = default, int usedTime = 0) {
        UsedTime = usedTime;

        if (Startpos == Vector3.zero) {
            startPos = ValueData.Instance.Player.transform.position;
            startPos.y = 0.3f;
        }
        else
            startPos = Startpos;

        if (Startrot.eulerAngles == new Vector3(0,0,0))
            startRot = ValueData.Instance.Player.transform.rotation;
        else
            startRot = Startrot;


        switch (Skillid) {
            case 1:
                Basicattack(Fieldid);
                break;
            case 2:
                Dash(Fieldid);
                break;
            case 4:
                Flash();
                break;
            case 5:
                Newmoon(Fieldid);
                break;
            case 6:
                Quartermoon(Fieldid);
                break;
            case 7:
                Fullmoon(Fieldid);
                break;
            case 8:
                Bell(Fieldid);
                break;
            case 9:
                Magicarrow(Fieldid);
                break;
            case 10:
                Waterball(Fieldid);
                break;
        }

        if(ValueData.Instance.WeaponField[Fieldid].ID == 7) //武器7的效果
        {
            foreach (SkillTagType tag in ValueData.Instance.SkillTag[Skillid])
            {
                if (tag == SkillTagType.Movement)//當前施放的需為位移技
                {
                    int _id = ValueData.Instance.SkillField[0].ID;
                    foreach (SkillTagType _tag in ValueData.Instance.SkillTag[_id])//額外觸發的需為非位移技
                    {
                        if (_tag == SkillTagType.Movement)
                            return;
                    }
                    startPos = ValueData.Instance.Player.transform.position;
                    startPos.y = 0.3f;
                    UseSkill(ValueData.Instance.SkillField[0].ID, 0, startPos, startRot);
                }
            }
        }
        if (ValueData.Instance.WeaponField[Fieldid].ID == 6 && usedTime < 2) //武器6效果
        {
            StartCoroutine(doubleSkill(ValueData.Instance.SkillField[Fieldid].ID, Fieldid, usedTime));
        }
    }

    //技能發射點
    Vector3 attackPoint(float distance = 1f)
    {
        Vector3 newPosition = startPos + ValueData.Instance.Player.transform.forward * distance;
        newPosition.y = 0.5f;
        return newPosition;
    }

    void Basicattack(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
    }

    void Dash(int Fieldid)  //閃避
    {
        StartCoroutine(dash(Fieldid));
    }
    IEnumerator dash(int Fieldid)
    {
        //ValueData.Instance.canBehurt = false;
        //SmokeTrail.Play();
        PlayerCtrl.Instance.canMove = false;
        ValueData.Instance.canBehurt = false;
        Vector3 m_Input;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            m_Input = ValueData.Instance.Player.transform.forward;
        for(int i =0;i<10;i++)
        {
            Vector3 m = Vector3.MoveTowards(ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.position + m_Input, Time.fixedDeltaTime * ValueData.Instance.SkillField[Fieldid].Speed * 15);
            m_Rigidbody.MovePosition(m);
            yield return new WaitForSeconds(0.005f);
        }
        ValueData.Instance.GetAp((ValueData.Instance.maxAP - ValueData.Instance.AP) / 2);
        PlayerCtrl.Instance.canMove = true;
        ValueData.Instance.canBehurt = true;
        //SmokeTrail.Stop();
        //ValueData.Instance.canBehurt = true;
    }

    void Flash()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
        {
            Vector3 targetPos = floorhit.point;
            PlayerCtrl.Instance.gameObject.transform.position = new Vector3(targetPos.x, PlayerCtrl.Instance.gameObject.transform.position.y, targetPos.z);
        }
    }

    void Newmoon(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 5)
        {
            UICtrl.Instance.ChangeSkill_ID = 6;
            UICtrl.Instance.isSpendmoney = false;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }
    void Quartermoon(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 6)
        {
            UICtrl.Instance.ChangeSkill_ID = 7;
            UICtrl.Instance.isSpendmoney = false;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }
    void Fullmoon(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 7)
        {
            UICtrl.Instance.ChangeSkill_ID = 5;
            UICtrl.Instance.isSpendmoney = false;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }

    IEnumerator doubleSkill(int Skillid, int Fieldid, int usedTime) 
    {
        yield return new WaitForSeconds(0.2f);
        UseSkill(Skillid, Fieldid, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation, usedTime + 1);
    }

    void Bell(int Fieldid) 
    {
        Vector3 targetpos = attackPoint(1.5f);
        targetpos.y = 0;
        GameObject a = Instantiate(Skill_Bell, targetpos, Skill_Bell.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
    }
    void Magicarrow(int Fieldid)
    {
        GameObject a = Instantiate(Skill_Magicarrow, attackPoint(0.7f), startRot);
        PlayerAttack b = a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>();
        b.fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        b.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
        if (UsedTime > 0)
            b.canSplit = false;
    }
    void Waterball(int Fieldid)
    {
        float _cost = ValueData.Instance.AP/2 * ValueData.Instance.CostDown;
        ValueData.Instance.GetAp(-_cost);
        ValueData.Instance.SkillField[Fieldid].Damage = _cost * 10 * ValueData.Instance.Power;
        StartCoroutine(waterball(Fieldid, _cost));
    }
    IEnumerator waterball(int Fieldid, float _dmg)
    {
        Vector3 targetPos = ValueData.Instance.Player.transform.position;
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
        {
            targetPos = floorhit.point;
            targetPos.y = 0.5f;
        }
        GameObject a = Instantiate(Skill_Waterball_1, targetPos, Skill_Waterball_1.transform.rotation);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
        Destroy(a, 1f);
        yield return new WaitForSeconds(0.7f);
        GameObject b = Instantiate(Skill_Waterball_2, targetPos, Skill_Waterball_2.transform.rotation);
        b.transform.GetComponent<PlayerAttack>().fidleid = Fieldid;
        b.transform.localScale = new Vector3(b.transform.localScale.x * _size, b.transform.localScale.y * _size, b.transform.localScale.z * _size);
        Destroy(b, 1f);
    }

}
