﻿using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public bool immortal; // 不死的
    public float Hp;
    public float maxHp;
    public float Attack;
    public float EXP; // 怪物提供的經驗值
    public float AttackCD; // 攻擊間隔
    public float AttackRange; // 攻擊範圍
    public float rotationSpeed;
    [Range(0, 2)]
    public int idleAnimation; // 待機動畫
    public int[] money = new int[2]; // 死亡掉落的金幣 浮動值

    public GameObject Mesh;
    public Slider hpUI;
    public GameObject Bullet; // 遠程用的子彈
    private Color C;
    public bool canBeHit; //受擊時短暫無敵，避免重複判定
    Animator m_Animator;
    Rigidbody rb;

    Transform target;
    NavMeshAgent agent;
    GameObject AttackCollider;
    public bool canAttack;
    public bool canMove;
    float bellCD; // bell觸發的冷卻
    GameObject NoticeRange;
    float noticeDistance; // 視野射線距離
    LayerMask mask = (1 << 8) | (1 << 9); // 視線，判斷玩家用

    private void OnEnable() {
        m_Animator = GetComponent<Animator>();
        Hp = maxHp;
        canBeHit = true;
        if (hpUI)
            hpUI.gameObject.SetActive(false);
        if (gameObject.tag == "Barrel" || gameObject.tag == "Bell")
        {
            C = Mesh.transform.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
        }
        else if (gameObject.tag == "Enemy") {
            C = Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.GetColor("_EmissionColor");
            m_Animator.SetInteger("IdleAnimation", idleAnimation);
            rb = GetComponent<Rigidbody>();
        }

    }

    void Start()
    {
        target = ValueData.Instance.Player.transform;
        agent = GetComponent<NavMeshAgent>();
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank)
        {
            AttackCollider = transform.Find("AttackCollider").gameObject;
            NoticeRange = transform.Find("NoticeRange").gameObject;
            canAttack = true;
            canMove = true;
        }
        else if (enemyType == EnemyType.Ranged)
        {
            NoticeRange = transform.Find("NoticeRange").gameObject;
            canAttack = true;
            canMove = true;
        }
        else if (enemyType == EnemyType.Minion)
        {
            float time = transform.parent.GetComponent<DestroyThis>().LifeTime;
            StartCoroutine(MiniontimeCD(time));
        }
        if (NoticeRange)
            noticeDistance = NoticeRange.GetComponent<SphereCollider>().radius;
    }

    void LateUpdate()
    {
        if (hpUI != null && Camera.main != null)
        {
            if (!hpUI.gameObject.activeSelf)
                return;
            hpUI.transform.rotation = UICtrl.Instance.canvas.transform.rotation;
        }
    }

    //受到傷害時使用
    public void Hurt(float Dmg, int Filed = -1) {
        if (NoticeRange && NoticeRange.activeSelf)
            StartAction();
        if (immortal)
        {
            //beAttack();
        }
        else if (Hp > Dmg)
        {
            if (Hp == maxHp && hpUI) // 首次受擊時顯示HPUI
                hpUI.gameObject.SetActive(true);
            Hp -= Dmg;
            hpUI.value = Hp / maxHp;
            beAttack();
        }
        else
        {
            Hp = 0;
            immortal = true;
            hpUI.transform.parent.gameObject.SetActive(false);
            Die(Filed);
        }
        if (gameObject.tag == "Bell" && bellCD == 0)
        {
            GameObject a = Instantiate(Skill.Instance.Skill_Bellattack, gameObject.transform.position, gameObject.transform.rotation);
            int id = gameObject.GetComponent<PlayerAttack>().fidleid;
            a.transform.gameObject.GetComponent<PlayerAttack>().fidleid = id;
            float size = ValueData.Instance.SkillField[id].Size;
            a.transform.localScale = new Vector3(a.transform.localScale.x * size, a.transform.localScale.y * size, a.transform.localScale.z * size);
            PlayerAttack atk = a.GetComponent<PlayerAttack>();
            atk.dmg = Dmg * 1.2f;
            atk.passTarget.Add(this.gameObject);
            bellCD = 0.1f;
            StartCoroutine(BellCD());
        }
    }

    public void Die(int Filed = -1) {
        //UICtrl.Instance.GetEXP(EXP);
        if (enemyType != EnemyType.Barrel)
        {
            LevelCtrl.Instance.leftEnemy -= 1;
            LevelCtrl.Instance.enemycheck();
        }
        if (Filed != -1)
        {
            if (ValueData.Instance.isHaveweaponid(Filed, 9)) //裝備9能力
            {
                int moneyadd = Random.Range(0, 4);
                money[0] *= moneyadd;
                money[1] *= moneyadd;
            }
        }
        if (ValueData.Instance.PassiveSkills[27]) // 天賦27 擊殺回魔
            ValueData.Instance.GetAp(ValueData.Instance.maxAP / 5);

        int _money = Random.Range(money[0], money[1]+1);
        LevelCtrl.Instance.DropMoney(_money, transform.position, 0.5f);

        StopAllCoroutines();
        Destroy(transform.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 playerpos = ValueData.Instance.Player.transform.position;
            playerpos.y = 0.5f;
            Vector3 directionToPlayer = (playerpos - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, noticeDistance, mask))
            {
                if (hit.collider.CompareTag("Player") && NoticeRange && NoticeRange.activeSelf)
                {
                    StartAction();
                }
            }
        }
    }

    void StartAction()
    {
        NoticeRange.SetActive(false);
        m_Animator.SetBool("isMoving", true);
        m_Animator.SetInteger("IdleAnimation", 0);
        StartCoroutine(_Move());
    }

    public void beAttack() {
        StartCoroutine(beAttackEffect());
    }

    IEnumerator beAttackEffect() {
        //桶子是用MeshRenderer，怪物是用SkinnedMeshRenderer
        if (gameObject.tag == "Barrel" || gameObject.tag == "Bell")
        {
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.188f, 0.188f, 0.188f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", C);
        }
        else if (gameObject.tag == "Enemy") {
            if(canMove)
            {
                canMove = false;
                agent.isStopped = true;
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
                yield return new WaitForSeconds(0.2f);
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
                //yield return new WaitForSeconds(0.1f);
                canMove = true;
                agent.isStopped = false;
            }
            else // 當下本來就不能移動時，不去動agent
            {
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
                yield return new WaitForSeconds(0.2f);
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
                //yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator _Move()
    {
        while (!canMove) {
            m_Animator.SetBool("isMoving", false);
            yield return new WaitForFixedUpdate();
        }

        Vector3 playerpos = ValueData.Instance.Player.transform.position;
        playerpos.y = 0.5f;
        Vector3 directionToPlayer = (playerpos - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, AttackRange, mask))
        {
            if (hit.collider.CompareTag("Player")) // 若玩家進到攻擊範圍且在視野內
            {
                // 停下並轉向玩家
                agent.isStopped = true;
                m_Animator.SetBool("isMoving", false);

                // 計算目標方向
                Vector3 direction = (target.position - transform.position);
                direction.y = 0; // 忽略垂直方向的差異

                // 旋轉角色面向目標
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

                // 攻擊行為
                if (canAttack)
                {
                    canMove = false;
                    m_Animator.SetBool("Attack", true);
                    yield break;
                }
            }
            else  // 若看不到玩家
            {
                m_Animator.SetBool("isMoving", true);
                agent.isStopped = false;
                agent.angularSpeed = rotationSpeed;
                agent.SetDestination(target.position);
            }
        }
        else // 若玩家在視野外
        {
            m_Animator.SetBool("isMoving", true);
            agent.isStopped = false;
            agent.angularSpeed = rotationSpeed;
            agent.SetDestination(target.position);
        }

        yield return new WaitForNextFrameUnit();
        StartCoroutine(_Move());

    }

    //動畫事件
    public void StartAttack()
    {
        if (enemyType == EnemyType.Melee)
        {
            AttackCollider.SetActive(true);
            m_Animator.SetBool("Attack", false);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            Instantiate(Bullet, transform.position, transform.rotation);
            m_Animator.SetBool("Attack", false);
        }
        else if (enemyType == EnemyType.Tank)
        {
            StartCoroutine(Sprint());
        }
        canAttack = false;
    }
    public void EndAttack()
    {
        if (enemyType == EnemyType.Melee)
            AttackCollider.SetActive(false);
        else if(enemyType == EnemyType.Tank)
        {
            AttackCollider.SetActive(false);
            m_Animator.SetBool("Attack", false);
        }
        canMove = true;
        agent.isStopped = false;
        StartCoroutine(_Move());
        StartCoroutine(_AttackCD());
    }
    public void EndBehit() {
        m_Animator.SetBool("BeHit", false);
    }

    IEnumerator _AttackCD()
    {
        yield return new WaitForSeconds(AttackCD);
        canAttack = true;
    }

    IEnumerator BellCD()
    {
        while(bellCD > 0)
        {
            if (bellCD <= 0.1f)
            {
                yield return new WaitForSeconds(bellCD);
                bellCD = 0;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                bellCD -= 0.1f;
            }
        }
    }

    IEnumerator MiniontimeCD(float time)
    {
        if (time <= 0)
            yield return 0;
        float nowtime = time;
        while(nowtime > 0.1f)
        {
            hpUI.value = nowtime / time;
            nowtime -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        if(nowtime > 0)
            yield return new WaitForSeconds(nowtime);
        hpUI.transform.parent.gameObject.SetActive(false);
    }

    // 衝刺
    IEnumerator Sprint()
    {
        yield return new WaitForSeconds(0.2f);
        AttackCollider.SetActive(true);
        float _time = 1;
        while(Hp > 0 && _time > 0)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            rb.velocity = transform.forward * 6;
            _time -= Time.fixedDeltaTime;
        }
        rb.velocity = Vector3.zero;
        EndAttack();
    }

}

public enum EnemyType 
{
    Melee, // 近戰 蜥蜴
    Ranged, // 遠程 魷魚
    Barrel, // 桶子
    Minion, // 招喚物
    Tank, // 近戰 鯡魚
}