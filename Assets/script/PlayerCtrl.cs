using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCtrl : MonoBehaviour
{
    public Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool Move;
    LayerMask mask = (1 << 7);//角色旋轉用
    public bool canMove;
    public GameObject UpgradeSystem;
    public ValueData valuedata;
    public Skill skill;
    GameObject ontriggerTarget;//放碰到的物體
    public RaycastHit floorhit;//滑鼠指到的目標

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
    void Start()
    {
        
        Application.targetFrameRate = 60;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        canMove = true;
        valuedata.canBehurt = true;
        StartCoroutine(RestoreAP());
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
            Vector3 _move = m_Input * valuedata.MoveSpeed * Time.fixedDeltaTime * 3;
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
            Vector3 playerToMouse = floorhit.point - transform.position;
            playerToMouse.y = 0;
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            m_Rigidbody.MoveRotation(newRotation);
        }
        
        //滑鼠L
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (valuedata.SkillField[0].ID == 0 || UICtrl.Instance.IsPointerOverUI(out GameObject uiElement))
                return;
            if (valuedata.SkillField[0].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[0].Cost)
            {
                valuedata.AP -= valuedata.SkillField[0].Cost;
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
            if (valuedata.SkillField[1].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[1].Cost)
            {
                valuedata.AP -= valuedata.SkillField[1].Cost;
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
            if (valuedata.SkillField[2].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[2].Cost)
            {
                valuedata.AP -= valuedata.SkillField[2].Cost;
                valuedata.SkillField[2].nowCD = valuedata.SkillField[2].maxCD;
                UICtrl.Instance.UpdateSkillCD();
                StartCoroutine(UICtrl.Instance.SkillCD(2));
                skill.UseSkill(valuedata.SkillField[2].ID, 2);
            }
        }
        //互動鍵E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ontriggerTarget)
                return;
            if (ontriggerTarget.tag == "Door")
            {
                LevelCtrl.Instance.nowPrize = ontriggerTarget.GetComponent<ExitDoor>().Prize;
                int nextLevel = ontriggerTarget.GetComponent<ExitDoor>().Level;
                LevelCtrl.Instance.NextLevel(nextLevel);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NPC")
        {
            other.GetComponent<Npc>().doNpc(true);
        }
        else if (other.tag == "Door")
        {
            ontriggerTarget = other.gameObject;
        }
        else if(other.tag == "Money")
        {
            Destroy(other.gameObject);
            ValueData.Instance.GetMoney(1);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "NPC")
        {
            other.GetComponent<Npc>().doNpc(false);
            ontriggerTarget = null;
        }
        else if (other.tag == "Door")
        {
            
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
            BeHurt(enemyattack.dmg, true);
            if (enemyattack.enemyType == EnemyType.Ranged)
            {
                Destroy(other.gameObject);
            }

        }
    }

    /// <summary>
    /// 扣血通用含式
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="useBehurtTimer">是否進無敵幀</param>
    public void BeHurt(float Value, bool useBehurtTimer)
    {
        Value *= (1 - valuedata.Damagereduction);
        if (valuedata.HP > Value)
        {
            valuedata.HP -= Value;
            if (useBehurtTimer == true)
                StartCoroutine(BehurtTimer());
        }
        else
        {
            //失敗
            valuedata.HP = 0;
            UICtrl.Instance.gameover();

        }
    }

    //受傷無敵幀
    IEnumerator BehurtTimer()
    {
        valuedata.canBehurt = false;
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.6f, 0, 0));
        yield return new WaitForSeconds(0.3f);
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return new WaitForSeconds(0.2f);
        valuedata.canBehurt = true;
    }

    //自動回魔
    IEnumerator RestoreAP() 
    {
        float value = valuedata.RestoreAP / 50;
        if (valuedata.AP >= valuedata.maxAP - value)
        {
            valuedata.AP = valuedata.maxAP;
        }
        else
        {
            valuedata.AP += value;
        }
        yield return new WaitForSeconds(0.02f);
        StartCoroutine(RestoreAP());
    }

}
