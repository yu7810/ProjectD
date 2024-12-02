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
    public ParticleSystem SmokeTrail;
    LayerMask maskFloor = (1 << 6);

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

    public void UseSkill(int Skillid, int Fieldid, int usedTime = 1) {
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
                    UseSkill(ValueData.Instance.SkillField[0].ID, 0);
                }
            }
        }
        if (ValueData.Instance.WeaponField[Fieldid].ID == 6 && usedTime <= 2) //武器6效果
        {
            StartCoroutine(doubleSkill(ValueData.Instance.SkillField[Fieldid].ID, Fieldid, usedTime));
        }
    }

    //技能發射點
    Vector3 attackPoint(float distance = 1f)
    {
        Vector3 newPosition = ValueData.Instance.Player.transform.position + ValueData.Instance.Player.transform.forward * distance;
        return newPosition;
    }

    void Basicattack(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation);
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
        Vector3 m_Input;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            m_Input = ValueData.Instance.Player.transform.forward;
        for(int i =0;i<6;i++)
        {
            Vector3 m = Vector3.MoveTowards(ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.position + m_Input, Time.fixedDeltaTime * ValueData.Instance.SkillField[Fieldid].Speed * 25);
            m_Rigidbody.MovePosition(m);
            yield return new WaitForSeconds(0.01f);
        }
        ValueData.Instance.GetAp((ValueData.Instance.maxAP - ValueData.Instance.AP) / 2);
        PlayerCtrl.Instance.canMove = true;
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
        GameObject a = Instantiate(Skill_A, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 5)
        {
            UICtrl.Instance.ChangeSkill_ID = 6;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }
    void Quartermoon(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 6)
        {
            UICtrl.Instance.ChangeSkill_ID = 7;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }
    void Fullmoon(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 7)
        {
            UICtrl.Instance.ChangeSkill_ID = 5;
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
            StartCoroutine(UICtrl.Instance.SkillCD(Fieldid));
        }
    }

    IEnumerator doubleSkill(int Skillid, int Fieldid, int usedTime) 
    {
        yield return new WaitForSeconds(0.2f);
        UseSkill(Skillid, Fieldid, usedTime+1);
    }

    void Bell(int Fieldid) 
    {
        GameObject a = Instantiate(Skill_Bell, attackPoint(1.5f), ValueData.Instance.Player.transform.rotation);
        a.GetComponent<PlayerAttack>().fidleid = Fieldid;
    }
    void Magicarrow(int Fieldid)
    {
        GameObject a = Instantiate(Skill_Magicarrow, attackPoint(0.7f), ValueData.Instance.Player.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
    }
}
