using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class Skill : MonoBehaviour
{
    private static Skill instance;
    Rigidbody m_Rigidbody;
    // ���a�ޯ�
    public GameObject Skill_A;
    public GameObject Skill_Bell;
    public GameObject Skill_Bellattack;
    public GameObject Skill_Magicarrow;
    public GameObject Skill_Waterball_1;
    public GameObject Skill_Waterball_2;
    public GameObject Skill_DashTrail;
    public GameObject Skill_FlashTrail;
    public GameObject Skill_Charge;
    public GameObject Skill_Sunfire;
    private readonly Dictionary<int, SunfireAura> sunfireAuras = new Dictionary<int, SunfireAura>();

    public void StopSunfire(int fieldId)
    {
        if (sunfireAuras.TryGetValue(fieldId, out var aura) && aura != null)
        {
            aura.gameObject.SetActive(false);
            Destroy(aura.gameObject);
        }
        sunfireAuras.Remove(fieldId);
    }

    public void StopAllSunfire()
    {
        foreach (int fieldId in sunfireAuras.Keys.ToArray()) StopSunfire(fieldId);
    }

    private void ToggleSunfire(int fieldId, int usedTime)
    {
        // Repeats are applied to each pulse by the aura; they never toggle it.
        if (usedTime > 0) return;
        ValueData.Instance.SkillField[fieldId].nowCD = ValueData.Instance.SkillField[fieldId].maxCD;
        UICtrl.Instance.UpdateSkillCD();
        if (sunfireAuras.TryGetValue(fieldId, out var existing) && existing != null)
        {
            StopSunfire(fieldId);
            return;
        }
        var root = Skill_Sunfire != null ? Instantiate(Skill_Sunfire) : new GameObject("Sunfire Aura");
        root.transform.SetParent(ValueData.Instance.Player.transform, false);
        var aura = root.GetComponent<SunfireAura>();
        if (aura == null) aura = root.AddComponent<SunfireAura>();
        sunfireAuras[fieldId] = aura;
        aura.Initialize(fieldId);
    }
    
    // �ĤH�ޯ�
    public GameObject Skill_Squidarrow;
    public GameObject Skill_Magicexplode1;
    public GameObject Skill_Magicexplode2;
    public GameObject Skill_Herringhealth; // �ֳ������^��S��

    LayerMask maskFloor = (1 << 7);
    LayerMask enemyLayer = (1 << 8 | 1 << 11);
    Vector3 startPos = new Vector3(0, 0, 0); // �ޯ�o�g��m
    Quaternion startRot = new Quaternion(0, 0, 0,0);
    int UsedTime = 0; // �����ޯ�s��ϥΦ���
    Coroutine useSkillStop; // �������y
    float stopTime; // �Ѿl���y�ɶ�
    Coroutine _CastOnCritical; // �����ɬI�񪺧N�o
    Coroutine _Charge; // �Ĩ����

    void Awake()
    {
        // �p�G��ҩ|���s�b�A�]�m�����󬰰ߤ@��ҨëO�d�b����������
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �p�G�w�g�s�b�@�ӹ�ҡA�P�����e����
            Destroy(gameObject);
        }
    }
    public static Skill Instance
    {
        get => instance;
    }
    public void ResetRun()
    {
        StopAllSunfire();
        StopAllCoroutines();
        useSkillStop = null;
        _CastOnCritical = null;
        _Charge = null;
        stopTime = 0;
        UsedTime = 0;
        startPos = Vector3.zero;
        startRot = Quaternion.identity;
    }

    private void Start()
    {
        m_Rigidbody = ValueData.Instance.Player.GetComponent<Rigidbody>();
    }

    public void UseSkill(int Skillid, int Fieldid, Vector3 Startpos = default, Quaternion Startrot = default, int usedTime = 0, float inheritedDamageMultiplier = -1f) {
        if (Skillid == ValueData.SunfireSkillId)
        {
            ToggleSunfire(Fieldid, usedTime);
            return;
        }
        float damageMultiplier = inheritedDamageMultiplier >= 0f
            ? inheritedDamageMultiplier : ValueData.Instance.ConsumeSunbreakerRage(Fieldid);
        UsedTime = usedTime;

        if (UsedTime == 0 && !ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement)) // �첾�ޤΦh���I�񤣷|���������y
        {
            float skillspeed = 0.1f * ValueData.Instance.SkillField[Fieldid].maxCD;
            if (skillspeed > 0.15f)
                skillspeed = 0.15f;
            else if (skillspeed < 0.03f)
                skillspeed = 0.03f;

            //�������y
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

        if (ValueData.Instance.isHaveweaponid(Fieldid, 14)) //�Z��14 �q�ƹ��y�Ч���
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
                Basicattack(Fieldid, damageMultiplier);
                break;
            case 2:
                Dash(Fieldid);
                break;
            case 4:
                Flash();
                break;
            case 5:
                Newmoon(Fieldid, damageMultiplier);
                break;
            case 6:
                Quartermoon(Fieldid, damageMultiplier);
                break;
            case 7:
                Fullmoon(Fieldid, damageMultiplier);
                break;
            case 8:
                Bell(Fieldid, damageMultiplier);
                break;
            case 9:
                Magicarrow(Fieldid, damageMultiplier);
                break;
            case 10:
                Waterball(Fieldid, damageMultiplier);
                break;
            case 11:
                Charge(Fieldid, damageMultiplier);
                break;
        }

        if(ValueData.Instance.isHaveweaponid(Fieldid, 7)) //�Z��7���ĪG
        {
            if (ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement))//���e�I�񪺻ݬ��첾��
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
        if (ValueData.Instance.isHaveweaponid(Fieldid, 6) && usedTime < 2) //�Z��6�ĪG
        {
            StartCoroutine(doubleSkill(ValueData.Instance.SkillField[Fieldid].ID, Fieldid, usedTime, damageMultiplier));
        }
        if (ValueData.Instance.isHaveweaponid(Fieldid, 16)) //�Z��16���ĪG
        {
            if (ValueData.Instance.SkillTag[Skillid].Contains(SkillTagType.Movement))//���e�I�񪺻ݬ��첾��
            {
                ValueData.Instance.GetAp((ValueData.Instance.maxAP - ValueData.Instance.AP) / 2);
            }
        }
    }

    // �����ɤ��i����
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

    //�ޯ�o�g�I
    Vector3 attackPoint(float distance = 1f)
    {
        Vector3 newPosition = startPos + ValueData.Instance.Player.transform.forward * distance;
        newPosition.y = 0.5f;
        return newPosition;
    }

    private static void ConfigureAttack(PlayerAttack attack, int fieldId, float damageMultiplier)
    {
        attack.fidleid = fieldId;
        attack.damageMultiplier = damageMultiplier;
    }

    void Basicattack(int Fieldid, float damageMultiplier)
    {
        startPos.y = 1.3f;
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        ConfigureAttack(a.transform.Find("Collider").GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale *= _size;
    }

    void Dash(int Fieldid)  //�{��
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
        for(int i =0;i<15;i++)
        {
            Vector3 m = Vector3.MoveTowards(ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.position + m_Input, Time.fixedDeltaTime * ValueData.Instance.SkillField[Fieldid].Speed * 20);
            if (NavMesh.SamplePosition(m, out NavMeshHit _hit, 10f, NavMesh.AllAreas))
                m_Rigidbody.MovePosition(_hit.position);

            //��s�����ĪG
            Vector3 newpos = PlayerCtrl.Instance.transform.position;
            newpos.y = 0.3f;
            trail.transform.position = newpos;

            yield return new WaitForSecondsRealtime(0.005f);
        }
        m_Rigidbody.linearVelocity = Vector3.zero;
        trail.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        PlayerCtrl.Instance.canMove = true;
        ValueData.Instance.canBehurt = true;
    }

    void Flash()
    {
        Instantiate(Skill_FlashTrail, startPos, startRot); // �S��
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
        {
            Vector3 targetPos = floorhit.point;
            if(NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 100f, NavMesh.AllAreas))
                PlayerCtrl.Instance.gameObject.transform.position = new Vector3(hit.position.x, PlayerCtrl.Instance.gameObject.transform.position.y, hit.position.z);
        }
    }

    void Newmoon(int Fieldid, float damageMultiplier)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        ConfigureAttack(a.transform.Find("Collider").GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 5)
        {
            UICtrl.Instance.ChangeSkill_ID[0] = 6;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // �׶}�����ޯ�ɥͦ��D��
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
        }
    }
    void Quartermoon(int Fieldid, float damageMultiplier)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        ConfigureAttack(a.transform.Find("Collider").GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 6)
        {
            UICtrl.Instance.ChangeSkill_ID[0] = 7;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // �׶}�����ޯ�ɥͦ��D��
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
        }
    }
    void Fullmoon(int Fieldid, float damageMultiplier)
    {
        GameObject a = Instantiate(Skill_A, startPos, startRot);
        ConfigureAttack(a.transform.Find("Collider").GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, 1, a.transform.localScale.z * _size);
        if (ValueData.Instance.SkillField[Fieldid].ID == 7)
        {
            UICtrl.Instance.ChangeSkill_ID[0] = 5;
            UICtrl.Instance.ChangeSkill_ID[1] = ValueData.Instance.SkillField[Fieldid].Level;
            UICtrl.Instance.isSpendmoney = false;
            ValueData.Instance.SkillField[Fieldid].ID = 0; // �׶}�����ޯ�ɥͦ��D��
            UICtrl.Instance.SelectSkillChangeField(Fieldid);
        }
    }

    IEnumerator doubleSkill(int Skillid, int Fieldid, int usedTime, float damageMultiplier)
    {
        yield return new WaitForSeconds(0.2f);
        UseSkill(Skillid, Fieldid, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation, usedTime + 1, damageMultiplier);
    }

    void Bell(int Fieldid, float damageMultiplier)
    {
        Vector3 targetpos;
        if (!ValueData.Instance.isHaveweaponid(Fieldid, 14)) // �˳�14�|��������m���ƹ���m
            targetpos = attackPoint(1.5f);
        else
            targetpos = startPos;
        targetpos.y = 0;
        GameObject a = Instantiate(Skill_Bell, targetpos, Skill_Bell.transform.rotation);
        ConfigureAttack(a.transform.Find("Collider").GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
    }
    void Magicarrow(int Fieldid, float damageMultiplier)
    {
        //�B�ʨ���
        float rngAngle = Random.Range(0f, 6f);
        Quaternion currentRotation = startRot * Quaternion.Euler(0, rngAngle, 0);

        GameObject a;
        if (!ValueData.Instance.isHaveweaponid(Fieldid, 14)) // �˳�14�|��������m���ƹ���m
            a = Instantiate(Skill_Magicarrow, attackPoint(0.7f), currentRotation);
        else
            a = Instantiate(Skill_Magicarrow, startPos, currentRotation);
        
        PlayerAttack b = a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>();
        ConfigureAttack(b, Fieldid, damageMultiplier);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        b.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
        if (UsedTime > 0)
            b.canSplit = false;
    }
    void Waterball(int Fieldid, float damageMultiplier)
    {
        StartCoroutine(waterball(Fieldid, damageMultiplier));
    }
    IEnumerator waterball(int Fieldid, float damageMultiplier)
    {
        float _cost = ValueData.Instance.WaterballCost(Fieldid);
        float damagePerMana = ValueData.Instance.GetSkillLevelStats(10, ValueData.Instance.SkillField[Fieldid].Level)?.WaterballDamagePerMana ?? 10f;
        ValueData.Instance.GetAp(-_cost);
        //ValueData.Instance.SkillField[Fieldid].Damage = _cost * damagePerMana * (1 + ValueData.Instance.Power + ValueData.Instance.WeaponField[Fieldid * 3].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 1].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 2].Damage);

        Vector3 targetPos = ValueData.Instance.Player.transform.position;
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out RaycastHit floorhit, 30f, maskFloor))
        {
            if (NavMesh.SamplePosition(floorhit.point, out NavMeshHit hit, 100, NavMesh.AllAreas))
                targetPos = hit.position;
            targetPos.y = 0.7f;
        }
        GameObject a = Instantiate(Skill_Waterball_1, targetPos, Skill_Waterball_1.transform.rotation);
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(a.transform.localScale.x * _size, a.transform.localScale.y * _size, a.transform.localScale.z * _size);
        Destroy(a, 1f);
        yield return new WaitForSeconds(0.7f);
        GameObject b = Instantiate(Skill_Waterball_2, targetPos, Skill_Waterball_2.transform.rotation);
        ConfigureAttack(b.GetComponent<PlayerAttack>(), Fieldid, damageMultiplier);
        b.transform.GetComponent<PlayerAttack>().dmg = _cost * damagePerMana * (1 + ValueData.Instance.Power + ValueData.Instance.WeaponField[Fieldid * 3].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 1].Damage + ValueData.Instance.WeaponField[Fieldid * 3 + 2].Damage);
        b.transform.localScale = new Vector3(b.transform.localScale.x * _size, b.transform.localScale.y * _size, b.transform.localScale.z * _size);
        Destroy(b, 1f);
    }

    void Charge(int Fieldid, float damageMultiplier)
    {
        if(_Charge == null)
            _Charge = StartCoroutine(charge(Fieldid, damageMultiplier));
    }

    IEnumerator charge(int Fieldid, float damageMultiplier)
    {
        PlayerCtrl.Instance.canMove = false;
        ValueData.Instance.canBehurt = false;
        m_Rigidbody.isKinematic = true;
        GameObject trail = Instantiate(Skill_DashTrail, startPos, startRot);
        Vector3 m_Input = ValueData.Instance.Player.transform.forward;
        Vector3 m = PlayerCtrl.Instance.transform.position;
        Vector3 startpos = m; // ��l��m�A�p��ˮ`��

        for (int i = 0; i < 10; i++)
        {
            Vector3 rayPos = PlayerCtrl.Instance.transform.position + new Vector3(0, 1f, 0);
            //Debug.DrawRay(rayPos, PlayerCtrl.Instance.transform.forward * 1.5f, Color.red, 1f);

            if (Physics.Raycast(rayPos, PlayerCtrl.Instance.transform.forward, out RaycastHit hit, 0.8f, enemyLayer))
            {
                m.y = 1.3f;
                GameObject a = Instantiate(Skill_Charge, m, startRot);
                PlayerAttack atk = a.GetComponent<PlayerAttack>();
                ConfigureAttack(atk, Fieldid, damageMultiplier);

                // �̽Ĩ�Z���M�w�ˮ`
                float distance = Vector3.Distance(startpos, PlayerCtrl.Instance.transform.position);
                atk.dmg = ValueData.Instance.SkillField[Fieldid].Damage + distance * (ValueData.Instance.GetSkillLevelStats(11, ValueData.Instance.SkillField[Fieldid].Level)?.ChargeDamagePerMeter ?? 3f);

                float _size = ValueData.Instance.SkillField[Fieldid].Size;
                a.transform.localScale *= _size;

                break;
            }
            else
            {
                m = Vector3.MoveTowards(ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.position + m_Input, Time.fixedDeltaTime * ValueData.Instance.SkillField[Fieldid].Speed * 30);
                if (NavMesh.SamplePosition(m, out NavMeshHit _hit, 10f, NavMesh.AllAreas))
                    m_Rigidbody.MovePosition(_hit.position);

                //��s�����ĪG
                Vector3 newpos = PlayerCtrl.Instance.transform.position;
                newpos.y = 0.3f;
                trail.transform.position = newpos;
                yield return new WaitForSecondsRealtime(0.005f);
            }
        }
        
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.linearVelocity = Vector3.zero;
        trail.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        PlayerCtrl.Instance.canMove = true;
        ValueData.Instance.canBehurt = true;
        _Charge = null;
    }

    public bool CastOnCritical() // �P�_�����ɬI��O�_�b�N�o
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
