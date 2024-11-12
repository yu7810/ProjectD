using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float Hp;
    public float maxHp;
    public float Attack;
    public float EXP;//怪物提供的經驗值
    public float AttackCD;//攻擊間隔

    public GameObject Mesh;
    private Color C;
    public bool canBeHit;//受擊時短暫無敵，避免重複判定
    Animator m_Animator;

    public Transform target;
    private NavMeshAgent agent;
    private GameObject AttackCollider;
    public bool canAttack;
    public bool canMove;

    private void OnEnable(){
        m_Animator = GetComponent<Animator>();
        Hp = maxHp;
        canBeHit = true;
        if (gameObject.tag == "Barrel"){
            C = Mesh.transform.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
        }
        else if (gameObject.tag == "Enemy") {
            C = Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.GetColor("_EmissionColor");
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        canAttack = true;
        if (tag == "Enemy") {
            AttackCollider = transform.Find("AttackCollider").gameObject;
            canMove = true;
            StartCoroutine(_Move());
        }
    }

    //受到傷害時使用
    public void Hurt(float Dmg) {
        if (Hp > Dmg)
        {
            Hp -= Dmg;
            beAttack();
        }
        else {
            Hp = 0;
            Die();
        }
            
    }

    public void Die() {
        UICtrl.Instance.GetEXP(EXP);
        Destroy(transform.gameObject);
    }

    public void beAttack() {
        if (gameObject.tag == "Enemy")
            m_Animator.SetBool("BeHit", true);
        StartCoroutine(beAttackEffect());
    }

    IEnumerator beAttackEffect() {
        //桶子是用MeshRenderer，怪物是用SkinnedMeshRenderer
        if (gameObject.tag == "Barrel")
        {
            //canBeHit = false;
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.188f, 0.188f, 0.188f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", C);
            //canBeHit = true;
        }
        else if (gameObject.tag == "Enemy"){
            canBeHit = false;
            canMove = false;
            agent.isStopped = true;
            m_Animator.SetBool("BeHit", true);
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
            yield return new WaitForSeconds(0.2f);
            canBeHit = true;
            canMove = true;
            agent.isStopped = false;
            StartCoroutine(_Move());
        }
    }

    IEnumerator _Move()
    {
        if (canMove)
            agent.SetDestination(target.position);
        else
            yield break;
        yield return new WaitForSeconds(0.05f);//放這裡的原因是SetDestination後remainingDistance不會馬上更新，等待後再抓
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            //攻擊行為
            if (canAttack)
            {
                canMove = false;
                agent.isStopped = true;
                m_Animator.SetBool("Attack", true);
            }
        }
        StartCoroutine(_Move());
    }

    //動畫事件
    public void StartAttack()
    {
        AttackCollider.SetActive(true);
        canAttack = false;
        m_Animator.SetBool("Attack", false);
        StartCoroutine(_AttackCD());
    }
    public void EndAttack()
    {
        AttackCollider.SetActive(false);
        canMove = true;
        agent.isStopped = false;
        StartCoroutine(_Move());
    }
    public void EndBehit() {
        m_Animator.SetBool("BeHit", false);
    }

    IEnumerator _AttackCD()
    {
        yield return new WaitForSeconds(AttackCD);
        canAttack = true;
    }

}
