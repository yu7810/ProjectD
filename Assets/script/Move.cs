using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private GameObject AttackCollider;
    Animator m_Animator;
    public float AttackCD;
    public bool canAttack;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(_Move());
        AttackCollider = transform.Find("AttackCollider").gameObject;
        m_Animator = GetComponent<Animator>();
        canAttack = true;
    }

    IEnumerator _Move() {
        agent.SetDestination(target.position);
        yield return new WaitForSeconds(0.05f);//放這裡的原因是SetDestination後remainingDistance不會馬上更新，等待後再抓
        if (agent.remainingDistance <= agent.stoppingDistance) {
            //攻擊行為
            if(canAttack)
                m_Animator.SetBool("Attack", true);
        }
        StartCoroutine(_Move());
    }

    //動畫事件
    public void StartAttack(){
        AttackCollider.SetActive(true);
        canAttack = false;
        StartCoroutine(_AttackCD());
    }
    public void EndAttack(){
        AttackCollider.SetActive(false);
        m_Animator.SetBool("Attack", false);
    }

    IEnumerator _AttackCD() {
        yield return new WaitForSeconds(AttackCD);
        canAttack = true;
    }

}
