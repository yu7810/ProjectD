using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public GameObject Skill_DashTrail;
    public GameObject Skill_FlashTrail;
    LayerMask maskFloor = (1 << 7);
    Vector3 startPos = new Vector3(0, 0, 0); // 技能發射位置
    Quaternion startRot = new Quaternion(0, 0, 0,0);
    int UsedTime = 0; // 紀錄技能連續使用次數
    Coroutine useSkillStop; // 攻擊停頓
    float stopTime; // 剩餘停頓時間
    Coroutine _CastOnCritical; // 暴擊時施放的冷卻

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

        if (UsedTime == 0 && !ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement)) // 位移技及多次施放不會有攻擊停頓
        {
            float skillspeed = 0.1f * (ValueData.Instance.SkillField[Fieldid].maxCD / ValueData.Instance.Skill[Skillid].maxCD);

            //攻擊停頓
            if (useSkillStop == null)
            {
                stopTime = skillspeed;
                useSkillStop = StartCoroutine(UseSkillStop());
            }
            else
            {
                if (stopTime < skillspeed)
                    stopTime = skillspeed;
            }
        }

        if (ValueData.Instance.isHaveweaponid(Fieldid, 14)) //武器14 從滑鼠座標攻擊
        {
            startPos = ValueData.Instance.Player.transform.position;
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
            {
                startPos = floorhit.point;
            }
        }
        else if (Startpos == Vector3.zero) {
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

        if(ValueData.Instance.isHaveweaponid(Fieldid, 7)) //武器7的效果
        {
            if (ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement))//當前施放的需為位移技
            {
                int _id = ValueData.Instance.SkillField[0].ID;
                if (ValueData.Instance.SkillTag[_id].Contains(SkillTagType.Movement))
                {
                    return;
                }
                startPos = ValueData.Instance.Player.transform.position;
                startPos.y = 0.3f;
                UseSkill(ValueData.Instance.SkillField[0].ID, 0, startPos, startRot);
            }
        }
        if (ValueData.Instance.isHaveweaponid(Fieldid, 6) && usedTime < 2) //武器6效果
        {
            StartCoroutine(doubleSkill(ValueData.Instance.SkillField[Fieldid].ID, Fieldid, usedTime));
        }
        if (ValueData.Instance.isHaveweaponid(Fieldid, 16)) //武器16的效果
        {
            if (ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement))//當前施放的需為位移技
            {
                ValueData.Instance.GetAp((ValueData.Instance.maxAP - ValueData.Instance.AP) / 2);
            }
        }
    }

    // 攻擊時不可移動
    IEnumerator UseSkillStop()
    {
        if (stopTime <= 0)
        {
            stopTime = 0;
            useSkillStop = null;
            yield break;
        }
            
        PlayerCtrl.Instance.canMove = false;
        while(stopTime >= 0.05f)
        {
            yield return new WaitForSeconds(0.05f);
            stopTime -= 0.05f;
        }
        PlayerCtrl.Instance.canMove = true;
        useSkillStop = null;
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
        startPos.y = 1.3f;
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale *= _size;
    }

    void Dash(int Fieldid)  //閃避
    {
        StartCoroutine(dash(Fieldid));
    }
    IEnumerator dash(int Fieldid)
    {
        PlayerCtrl.Instance.canMove = false;
        ValueData.Instance.canBehurt = false;
        GameObject trail = Instantiate(Skill_DashTrail, startPos, startRot);
        Vector3 m_Input;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            m_Input = ValueData.Instance.Player.transform.forward;
        for(int i =0;i<14;i++)
        {
            Vector3 m = Vector3.MoveTowards(ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.position + m_Input, Time.fixedDeltaTime * ValueData.Instance.SkillField[Fieldid].Speed * 15);
            m_Rigidbody.MovePosition(m);

            //更新托尾效果
            Vector3 newpos = PlayerCtrl.Instance.transform.position;
            newpos.y = 0.3f;
            trail.transform.position = newpos;

            yield return new WaitForSeconds(0.005f);
        }
        trail.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        PlayerCtrl.Instance.canMove = true;
        ValueData.Instance.canBehurt = true;
    }

    void Flash()
    {
        GameObject trail = Instantiate(Skill_FlashTrail, startPos, startRot);
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
            UICtrl.Instance.ChangeSkill_ID[0] = 6;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // 避開替換技能時生成道具
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
            UICtrl.Instance.ChangeSkill_ID[0] = 7;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // 避開替換技能時生成道具
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
            UICtrl.Instance.ChangeSkill_ID[0] = 5;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // 避開替換技能時生成道具
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
        Vector3 targetpos;
        if (!ValueData.Instance.isHaveweaponid(Fieldid, 14)) // 裝備14會讓中的位置改到滑鼠位置
            targetpos = attackPoint(1.5f);
        else
            targetpos = startPos;
        targetpos.y = 0;
        GameObject a = Instantiate(Skill_Bell, targetpos, Skill_Bell.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
    }
    void Magicarrow(int Fieldid)
    {
        //浮動角度
        float rngAngle = Random.Range(0f, 6f);
        Quaternion currentRotation = startRot * Quaternion.Euler(0, rngAngle, 0);

        GameObject a;
        if (!ValueData.Instance.isHaveweaponid(Fieldid, 14)) // 裝備14會讓中的位置改到滑鼠位置
            a = Instantiate(Skill_Magicarrow, attackPoint(0.7f), currentRotation);
        else
            a = Instantiate(Skill_Magicarrow, startPos, currentRotation);
        
        PlayerAttack b = a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>();
        b.fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        b.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
        if (UsedTime > 0)
            b.canSplit = false;
    }
    void Waterball(int Fieldid)
    {
        float _cost = ValueData.Instance.AP/2 * (1 + ValueData.Instance.Cost + ValueData.Instance.WeaponField[Fieldid * 3].Cost + ValueData.Instance.WeaponField[Fieldid * 3 + 1].Cost + ValueData.Instance.WeaponField[Fieldid * 3 + 2].Cost);
        ValueData.Instance.GetAp(-_cost);
        ValueData.Instance.SkillField[Fieldid].Damage = _cost * 10 * (1 + ValueData.Instance.Power + ValueData.Instance.WeaponField[Fieldid * 3].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 1].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 2].Damage);
        StartCoroutine(waterball(Fieldid));
    }
    IEnumerator waterball(int Fieldid)
    {
        Vector3 targetPos = ValueData.Instance.Player.transform.position;
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
        {
            targetPos = floorhit.point;
            targetPos.y = 0.7f;
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

    public bool CastOnCritical() // 判斷暴擊時施放是否在冷卻
    {
        if(_CastOnCritical == null)
        {
            _CastOnCritical = StartCoroutine(COC());
            return true;
        }
        else
            return false;
    }
    IEnumerator COC()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        _CastOnCritical = null;
    }
}
