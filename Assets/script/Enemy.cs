using System.Collections;
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
    public int[] money = new int[2]; // 死亡掉落的金幣 浮動值

    public GameObject Mesh;
    public Slider hpUI;
    public GameObject Bullet; // 遠程用的子彈
    private Color C;
    public bool canBeHit; //受擊時短暫無敵，避免重複判定
    Animator m_Animator;

    private Transform target;
    private NavMeshAgent agent;
    private GameObject AttackCollider;
    public bool canAttack;
    public bool canMove;
    private float bellCD; // bell觸發的冷卻

    private void OnEnable(){
        m_Animator = GetComponent<Animator>();
        Hp = maxHp;
        canBeHit = true;
        if (gameObject.tag == "Barrel" || gameObject.tag == "Bell")
        {
            C = Mesh.transform.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
        }
        else if (gameObject.tag == "Enemy") {
            C = Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.GetColor("_EmissionColor");
        }
        
    }

    void Start()
    {
        target = ValueData.Instance.Player.transform;
        agent = GetComponent<NavMeshAgent>();
        if (enemyType == EnemyType.Melee )
        {
            AttackCollider = transform.Find("AttackCollider").gameObject;
            canAttack = true;
            canMove = true;
            StartCoroutine(_Move());
        }
        else if(enemyType == EnemyType.Ranged)
        {
            canAttack = false;
            StartCoroutine(_AttackCD());
            canMove = true;
            StartCoroutine(_Move());
        }
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
    public void Hurt(float Dmg) {
        if(immortal)
            beAttack();
        else if (Hp > Dmg)
        {
            Hp -= Dmg;
            hpUI.value = Hp / maxHp;
            beAttack();
        }
        else {
            Hp = 0;
            hpUI.transform.parent.gameObject.SetActive(false);
            Die();
        }
        if(gameObject.tag == "Bell" && bellCD == 0)
        {
            GameObject a = Instantiate(Skill.Instance.Skill_Bellattack, gameObject.transform.position, gameObject.transform.rotation);
            int id = gameObject.GetComponent<PlayerAttack>().fidleid;
            float size = ValueData.Instance.SkillField[id].Size;
            a.transform.localScale = new Vector3(a.transform.localScale.x * size, a.transform.localScale.y * size, a.transform.localScale.z * size);
            PlayerAttack atk = a.GetComponent<PlayerAttack>();
            atk.dmg = Dmg * 3;
            atk.passTarget = new GameObject[1] { this.gameObject }; //防止重複打到自己
            bellCD = 0.1f;
            StartCoroutine(BellCD());
        }
    }

    public void Die() {
        //UICtrl.Instance.GetEXP(EXP);
        LevelCtrl.Instance.leftEnemy -= 1;
        LevelCtrl.Instance.enemycheck();
        float rng = 0.5f;
        int _money = Random.Range(money[0], money[1]);
        for(int i=0;i<_money;i++)
        {
            Vector3 offset = new Vector3(Random.Range(-rng, rng), 0, Random.Range(-rng, rng));
            Vector3 pos = transform.position + offset;
            GameObject money = ValueData.Instance.moneyPrefab;
            Instantiate(money, pos, money.transform.rotation);
        }
        if (ValueData.Instance.PassiveSkills[11])
            ValueData.Instance.Health(1);
        Destroy(transform.gameObject);
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
        else if (gameObject.tag == "Enemy"){
            canMove = false;
            agent.isStopped = true;
            //m_Animator.SetBool("BeHit", true);
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
            yield return new WaitForSeconds(0.1f);
            canMove = true;
            agent.isStopped = false;
            StartCoroutine(_Move());
        }
    }

    IEnumerator _Move()
    {
        if (canMove) {
            agent.SetDestination(target.position);
            yield return new WaitForSeconds(0.05f);//放這裡的原因是SetDestination後remainingDistance不會馬上更新，等待後再抓
        }
        else
            yield break;
        //玩家進到攻擊範圍
        while (agent.remainingDistance <= AttackRange)
        {
            if(canMove) // 持續轉向玩家
            {
                agent.isStopped = true;

                // 計算目標方向
                Vector3 direction = (target.position - transform.position);
                direction.y = 0; // 忽略垂直方向的差異

                // 旋轉角色面向目標
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            //攻擊行為
            if (canAttack)
            {
                canMove = false;
                m_Animator.SetBool("Attack", true);
            }

            yield return new WaitForSeconds(0.02f);
        }
        
        agent.isStopped = false;
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(_Move());
    }

    //動畫事件
    public void StartAttack()
    {
        if (enemyType == EnemyType.Melee)
            AttackCollider.SetActive(true);
        else if (enemyType == EnemyType.Ranged)
        {
            Instantiate(Bullet, transform.position, transform.rotation);
        }
        canAttack = false;
        m_Animator.SetBool("Attack", false);
    }
    public void EndAttack()
    {
        if (enemyType == EnemyType.Melee)
            AttackCollider.SetActive(false);
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

}

public enum EnemyType 
{
    Melee, // 近戰
    Ranged, // 遠程
    Barrel, // 桶子
    Minion, // 招喚物
}