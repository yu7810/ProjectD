using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCtrl : MonoBehaviour
{
    public Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool Move;
    LayerMask mask = (1 << 7);//角色旋轉用
    public bool canMove;
    public bool canAttack;
    public bool isReload; // 正在reload魔力(天賦28)
    public GameObject UpgradeSystem;
    public ValueData valuedata;
    public Skill skill;
    GameObject ontriggerTarget; // 放碰到的物體
    public GameObject openedTarget; // 已經互動的NPC，用來開關對應UI
    RaycastHit floorhit;
    public Vector3 playerToMouse;//滑鼠指到的座標

    //角色漂浮效果
    private float amplitude = 0.15f;  // 漂浮的幅度
    private float frequency = 0.8f;   // 漂浮的頻率
    private Vector3 startPosition; // 初始位置
    public GameObject Character;

    // 靜態實例，用於存儲唯一的實例
    private static PlayerCtrl instance;
    public static PlayerCtrl Instance
    {
        get => instance;
    }

    // 在 Awake 中檢查和創建單例
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
        int L = UICtrl.Instance._passiveskill.transform.childCount;
        valuedata.PassiveSkills = new bool[L];
    }
    public void Start()
    {
        Application.targetFrameRate = 60;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        canMove = true;
        canAttack = true;
        isReload = false;
        valuedata.canBehurt = true;
        //初始技能
        UICtrl.Instance.ChangeSkill_ID[1] = 1;
        UICtrl.Instance.ChangeSkill_ID[0] = 1;
        UICtrl.Instance.SelectSkillChangeField(0);
        UICtrl.Instance.ChangeSkill_ID[0] = 9;
        UICtrl.Instance.SelectSkillChangeField(1);
        UICtrl.Instance.ChangeSkill_ID[0] = 2;
        UICtrl.Instance.SelectSkillChangeField(2);
        //初始武器
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.SelectWeaponChangeField(0);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.SelectWeaponChangeField(1);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.SelectWeaponChangeField(2);
        startPosition = Character.transform.localPosition;
    }

    void FixedUpdate()
    {   
        //基本移動
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (!canMove)
                return;
            Move = true;
            m_Animator.SetBool("Move", Move);
            Vector3 m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            Vector3 _move = m_Input * valuedata.MoveSpeed * Time.fixedDeltaTime * 3f;
            m_Rigidbody.MovePosition(m_Rigidbody.position + _move);
        }
        else
        {
            Move = false;
            m_Animator.SetBool("Move", Move);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            m_Rigidbody.velocity = Vector3.zero;
        }
        //3D角色轉向
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out floorhit, 30f, mask))
        {
            playerToMouse = floorhit.point - transform.position;
            playerToMouse.y = 0;
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            m_Rigidbody.MoveRotation(newRotation);
        }

        //漂浮
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        Character.transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

    }

    private void Update()
    {
        //滑鼠L
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (valuedata.SkillField[0].ID == 0 || UICtrl.Instance.IsPointerOverUI(out GameObject uiElement))
                return;
            if (valuedata.SkillField[0].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[0].Cost && canAttack)
            {
                valuedata.GetAp(-valuedata.SkillField[0].Cost);
                valuedata.SkillField[0].nowCD = valuedata.SkillField[0].maxCD;
                UICtrl.Instance.UpdateSkillCD();
                StartCoroutine(UICtrl.Instance.SkillCD(0));
                skill.UseSkill(valuedata.SkillField[0].ID, 0);
            }
        }
        //滑鼠R
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (valuedata.SkillField[1].ID == 0)
                return;
            if (valuedata.SkillField[1].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[1].Cost && canAttack)
            {
                valuedata.GetAp(-valuedata.SkillField[1].Cost);
                valuedata.SkillField[1].nowCD = valuedata.SkillField[1].maxCD;
                UICtrl.Instance.UpdateSkillCD();
                StartCoroutine(UICtrl.Instance.SkillCD(1));
                skill.UseSkill(valuedata.SkillField[1].ID, 1);
            }
        }
        //空白鍵
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (valuedata.SkillField[2].ID == 0)
                return;
            if (valuedata.SkillField[2].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[2].Cost && canAttack)
            {
                valuedata.GetAp(-valuedata.SkillField[2].Cost);
                valuedata.SkillField[2].nowCD = valuedata.SkillField[2].maxCD;
                UICtrl.Instance.UpdateSkillCD();
                StartCoroutine(UICtrl.Instance.SkillCD(2));
                skill.UseSkill(valuedata.SkillField[2].ID, 2);
            }
        }
        //互動鍵E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ontriggerTarget || !canMove)
                return;
            if (openedTarget != null && openedTarget == ontriggerTarget)
            {
                UICtrl.Instance.showWeaponstore(false);
                UICtrl.Instance.showSkillstore(false);
                openedTarget = null;
            }
            else if (ontriggerTarget.tag == "NPC")
            {
                ontriggerTarget.GetComponent<Npc>().doNpc(true);
                openedTarget = ontriggerTarget;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NPC")
        {
            if (ontriggerTarget) // 關閉上個NPC的名字
            {
                if (ontriggerTarget.TryGetComponent<Npc>(out Npc _npc))
                {
                    if (_npc.NameUI)
                        _npc.NameUI.SetActive(false);
                }
            }
            ontriggerTarget = other.gameObject;
            if(other.TryGetComponent<Npc>(out Npc npc))
            {
                if (npc.NameUI)
                {
                    npc.NameUI.SetActive(true);
                    npc.NameUI.transform.parent.rotation = Camera.main.transform.rotation;
                }
                    
            }
        }
        else if(other.tag == "Money")
        {
            Destroy(other.transform.parent.gameObject);
            ValueData.Instance.GetMoney(1);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "NPC")
        {
            other.GetComponent<Npc>().doNpc(false);
            if (other.TryGetComponent<Npc>(out Npc npc))
            {
                if (npc.NameUI)
                    npc.NameUI.SetActive(false);
            }
            if (ontriggerTarget == other.gameObject)
                ontriggerTarget = null;
            if(openedTarget == other.gameObject)
                openedTarget = null;
        }
        else if (other.tag == "Door")
        {
            if(ontriggerTarget == other.gameObject)
                ontriggerTarget = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!valuedata.canBehurt)
            return;
        if (other.tag == "EnemyAttack")
        {
            //BeHurt(other.transform.parent.GetComponent<Enemy>().Attack, true);
            EnemyAttack enemyattack = other.transform.GetComponent<EnemyAttack>();
            ValueData.Instance.GetHp(-enemyattack.dmg, true);
            if (enemyattack.enemyType == EnemyType.Ranged)
            {
                Destroy(other.gameObject);
            }
        }
    }

    //受傷無敵幀
    public IEnumerator BehurtTimer(bool isvignetteColor)
    {
        valuedata.canBehurt = false;
        if (UICtrl.Instance.vignette != null && isvignetteColor)
            UICtrl.Instance.vignette.color.value = UICtrl.Instance.vignetteBehurtColor;
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.6f, 0, 0));
        //yield return new WaitForSeconds(0.3f);
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return new WaitForSeconds(0.4f);
        if (UICtrl.Instance.vignette != null && isvignetteColor)
            UICtrl.Instance.vignette.color.value = UICtrl.Instance.vignetteColor;
        valuedata.canBehurt = true;
    }

}
