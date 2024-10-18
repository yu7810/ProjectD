using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCtrl : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool Run;
    bool Move;
    float value_RunSpeed = 1.5f;
    public GameObject UIctrl;
    LayerMask mask = ~(1 << 6);
    public bool canMove;
    public bool canAttack02;
    public GameObject SmokeTrail;
    public bool canBehurt;//可被攻擊，用於受傷無敵幀
    //public GameObject Mesh;
    public GameObject Skill_A;
    public GameObject UpgradeSystem;
    //public GameObject Warrior; //2D人物

    void Start(){
        Application.targetFrameRate = 60;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        canMove = true;
        canAttack02 = false;
        canBehurt = true;
        //StartCoroutine(TimeHP());
    }

    void Update(){
        //鏡頭跟隨
        Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z - 10f);
        //自然回體
        if (UIctrl.GetComponent<UICtrl>().AP >= UIctrl.GetComponent<UICtrl>().maxAP){
            UIctrl.GetComponent<UICtrl>().AP = UIctrl.GetComponent<UICtrl>().maxAP;
        }
        else {
            UIctrl.GetComponent<UICtrl>().AP += 0.6f * Time.deltaTime;
        }
        //基本移動
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (!canMove)
                return;
            ValueCount();
            Move = true;
            m_Animator.SetBool("Move", Move);
            Vector3 m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //m_Rigidbody.MovePosition(transform.position + m_Input * Time.deltaTime * UIctrl.GetComponent<UICtrl>().MoveSpeed);
            m_Rigidbody.velocity = new Vector3(Input.GetAxis("Horizontal") * Time.timeScale * UIctrl.GetComponent<UICtrl>().MoveSpeed, 0, Input.GetAxis("Vertical") * Time.timeScale * UIctrl.GetComponent<UICtrl>().MoveSpeed);
            //跑步耗體
            /*if (Run)
            {
                if (UIctrl.GetComponent<UICtrl>().AP > 0.1f)
                    UIctrl.GetComponent<UICtrl>().AP -= 1f * Time.deltaTime;
            }*/
            //2D角色轉向
            /*if (Input.GetKey(KeyCode.A))
                transform.rotation = Quaternion.Euler(0,180,0);
            else if (Input.GetKey(KeyCode.D))
                transform.rotation = Quaternion.Euler(0,0,0);*/
        }
        else
        {
            Move = false;
            m_Animator.SetBool("Move", Move);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D)){
            m_Rigidbody.velocity = Vector3.zero;
        }
        //跑步
        /*if (Input.GetKey(KeyCode.LeftControl)){
            if (UIctrl.GetComponent<UICtrl>().AP <= 0.1f) {
                Run = false;
                m_Animator.SetBool("Run", Run);
                UIctrl.GetComponent<UICtrl>().MoveSpeed = UIctrl.GetComponent<UICtrl>().value_MoveSpeed;
                return;
            }
            Run = true;
            m_Animator.SetBool("Run", Run);
            UIctrl.GetComponent<UICtrl>().MoveSpeed = UIctrl.GetComponent<UICtrl>().value_MoveSpeed * value_RunSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl)){
            Run = false;
            m_Animator.SetBool("Run", Run);
            UIctrl.GetComponent<UICtrl>().MoveSpeed = UIctrl.GetComponent<UICtrl>().value_MoveSpeed;
        }*/
        //3D角色轉向
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorhit;
        if (Physics.Raycast(camRay, out floorhit , 30f, mask)) { 
            Vector3 playerToMouse = floorhit.point - transform.position;
            playerToMouse.y = 0;
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            m_Rigidbody.MoveRotation(newRotation);
        }
        //攻擊
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            UseSkill_A();
            /*if (canAttack02 && !m_Animator.GetBool("Attack02"))
                m_Animator.SetBool("Attack02", true);
            else if(!m_Animator.GetBool("Attack01"))
                m_Animator.SetBool("Attack01",true);*/
        }
        //衝刺
        if (Input.GetKeyDown(KeyCode.Space) && UpgradeSystem.GetComponent<UpgradeSystem>().UpgradeList[9].Lv >=1) {
            if (UIctrl.GetComponent<UICtrl>().AP >= UICtrl.FlashCost) {
                UIctrl.GetComponent<UICtrl>().AP -= UICtrl.FlashCost;
                StartCoroutine(Flash());
            }
        }

    }

    void ValueCount() {
        UIctrl.GetComponent<UICtrl>().MoveSpeed = UICtrl.value_MoveSpeed + UICtrl.value_SkillB_MoveSpeed;
    }

    void AttackEnd(int AttackID) {
        if (AttackID == 1){
            m_Animator.SetBool("Attack01", false);
            canAttack02 = false;
        }
        else if (AttackID == 2) {
            m_Animator.SetBool("Attack02", false);
            m_Animator.SetBool("Attack01", false);
            canAttack02 = false;
        }
        
    }

    void canAttack02Event() {
        canAttack02 = true;
    }

    IEnumerator Flash() {
        canBehurt = false;
        SmokeTrail.GetComponent<ParticleSystem>().Play();
        if (UpgradeSystem.GetComponent<UpgradeSystem>().UpgradeList[11].Lv > 0) {
            Health(0.5f);
        }
        Vector3 m_Input;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            m_Input = transform.forward;
        for (int i = 0; i < 20; i++) {
            m_Rigidbody.MovePosition(transform.position + m_Input * Time.deltaTime * UICtrl.FlashDistance);
            yield return new WaitForSeconds(0.01f);
        }
        SmokeTrail.GetComponent<ParticleSystem>().Stop();
        canBehurt = true;
    }

    private void OnTriggerStay(Collider other){
        if (!canBehurt)
            return;
        if (other.tag == "EnemyAttack" ) {
            BeHurt(other.transform.parent.GetComponent<Enemy>().Attack, true);
            if (other.tag == "EnemyAttack") {
                //Destroy(other.gameObject); //要思考子彈怎處理
            }

        }
    }

    /// <summary>
    /// 扣血通用含式
    /// </summary>
    /// <param name="Value"></param>
    /// <param name="useBehurtTimer">是否進無敵幀</param>
    public void BeHurt(float Value, bool useBehurtTimer) {
        if (UIctrl.GetComponent<UICtrl>().HP > Value)
        {
            UIctrl.GetComponent<UICtrl>().HP -= Value;
            if (useBehurtTimer == true)
                StartCoroutine(BehurtTimer());
        }
        else
        {
            //失敗
            UIctrl.GetComponent<UICtrl>().HP = 0;
            UIctrl.GetComponent<UICtrl>().gameover();

        }
    }

    //受傷無敵幀
    IEnumerator BehurtTimer() {
        canBehurt = false;
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.6f, 0, 0));
        yield return new WaitForSeconds(0.3f);
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return new WaitForSeconds(0.2f);
        canBehurt = true;
    }

    IEnumerator AutoSkill_A() {
        UseSkill_A();
        float CD = 1 / UICtrl.Skill_A_AttackSpeed;
        yield return new WaitForSeconds(CD);
        StartCoroutine(AutoSkill_A());
    }

    void UseSkill_A() {
        if (UIctrl.GetComponent<UICtrl>().Skill_A_CD > 0)
            return;
        //呼叫技能CD
        GameObject a = Instantiate(Skill_A, transform.position, transform.rotation);
        float size = UICtrl.Skill_A_Size;
        a.transform.localScale = new Vector3(size, 1, size);
    }

    //回復生命
    public void Health(float value) {
        if (UIctrl.GetComponent<UICtrl>().HP < UIctrl.GetComponent<UICtrl>().maxHP - value)
            UIctrl.GetComponent<UICtrl>().HP += value;
        else
            UIctrl.GetComponent<UICtrl>().HP = UIctrl.GetComponent<UICtrl>().maxHP;
    }

    //暫，將血條與時間倒數合併
    IEnumerator TimeHP() {
        yield return new WaitForSeconds(0.1f);
        BeHurt(0.1f,false);
        StartCoroutine(TimeHP());
    }

}
