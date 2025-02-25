using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public EnemyRarity enemyRarity;
    public bool immortal; // 不死的
    public bool notMove; // 不會移動的
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
    Coroutine sprint; // 鯡魚衝刺
    Coroutine call; // 招喚小兵
    Coroutine move;
    Coroutine attackCD;
    public int BossStage; // Boss階段
    public GameObject[] MinionPosition; // 要招喚小怪的點

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
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Tank || enemyType == EnemyType.Warrior)
        {
            AttackCollider = transform.Find("AttackCollider").gameObject;
            NoticeRange = transform.Find("NoticeRange").gameObject;
            canAttack = true;
            canMove = true;
        }
        else if (enemyType == EnemyType.Ranged || enemyType == EnemyType.Mage)
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

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    //受到傷害時使用
    public void Hurt(float Dmg, int Filed = -1) {
        if (NoticeRange && NoticeRange.activeSelf)
            StartAction();
        if (immortal)
        {
            //beAttack();
        }
        else if(Dmg < 0) //受傷
        {
            if (Hp > -Dmg)
            {
                if (Hp == maxHp && hpUI) // 首次受擊時顯示HPUI
                    hpUI.gameObject.SetActive(true);
                Hp += Dmg;
                hpUI.value = Hp / maxHp;
                beAttack();

                if (enemyRarity == EnemyRarity.Boss) // BOSS轉階段條件
                {
                    if (Hp <= maxHp * 3 / 4 && BossStage == 0)
                        BossStage = 1;
                    else if (Hp <= maxHp * 1 / 2 && BossStage == 2)
                        BossStage = 3;
                    else if (Hp <= maxHp * 1 / 4 && BossStage == 4)
                        BossStage = 5;
                }
            }
            else
            {
                Hp = 0;
                immortal = true;
                hpUI.transform.parent.gameObject.SetActive(false);
                Die(Filed);
            }
        }
        else // 回血
        {
            if (maxHp > Dmg + Hp)
            {
                Hp += Dmg;
                hpUI.value = Hp / maxHp;
            }
            else
            {
                Hp = maxHp;
                hpUI.value = 1;
            }
        }

        if (gameObject.tag == "Bell" && bellCD == 0)
        {
            GameObject a = Instantiate(Skill.Instance.Skill_Bellattack, gameObject.transform.position, gameObject.transform.rotation);
            int id = gameObject.GetComponent<PlayerAttack>().fidleid;
            a.transform.gameObject.GetComponent<PlayerAttack>().fidleid = id;
            float size = ValueData.Instance.SkillField[id].Size;
            a.transform.localScale = new Vector3(a.transform.localScale.x * size, a.transform.localScale.y * size, a.transform.localScale.z * size);
            PlayerAttack atk = a.GetComponent<PlayerAttack>();
            atk.dmg = -Dmg * ValueData.Instance.SkillField[id].Damage;
            atk.passTarget.Add(this.gameObject);
            bellCD = 0.1f;
            StartCoroutine(BellCD());
        }
    }

    public void Die(int Filed = -1) {
        //UICtrl.Instance.GetEXP(EXP);
        if (enemyType != EnemyType.Barrel && enemyRarity != EnemyRarity.Minion)
        {
            LevelCtrl.Instance.enemycheck(1);
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
        Destroy(gameObject);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemyAttack")
        {
            // 被鯡魚王吃掉
            if (enemyType == EnemyType.Tank && enemyRarity == EnemyRarity.Minion && other.GetComponent<EnemyAttack>().enemy.enemyRarity == EnemyRarity.Boss)
            {
                Instantiate(Skill.Instance.Skill_Herringhealth, transform.position, Skill.Instance.Skill_Herringhealth.transform.rotation);
                other.GetComponent<EnemyAttack>().enemy.Hurt(500); // boss回血
                Destroy(gameObject);
            }
        }
    }

    void StartAction()
    {
        NoticeRange.SetActive(false);
        m_Animator.SetBool("isMoving", true);
        m_Animator.SetInteger("IdleAnimation", 0);
        if(move == null)
            move = StartCoroutine(_Move());
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
                if(enemyRarity == EnemyRarity.Minion || enemyRarity == EnemyRarity.Normal || enemyRarity == EnemyRarity.Rare) // 會受傷停頓
                {
                    canMove = false;
                    agent.isStopped = true;
                    Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
                    yield return new WaitForSeconds(0.2f);
                    Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
                    canMove = true;
                    agent.isStopped = false;
                }
                else // 不會受傷停頓
                {
                    Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
                    yield return new WaitForSeconds(0.2f);
                    Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
                }
                
            }
            else // 當下本來就不能移動時，不去動agent
            {
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
                yield return new WaitForSeconds(0.2f);
                Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
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
                rb.velocity = Vector3.zero;
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
                    move = null;
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
            if (!notMove)
            {
                m_Animator.SetBool("isMoving", true);
                agent.isStopped = false;
                agent.angularSpeed = rotationSpeed;
                agent.SetDestination(target.position);
            }
        }

        yield return new WaitForNextFrameUnit();
        move = StartCoroutine(_Move());

    }

    //動畫事件
    public void StartAttack()
    {
        if (enemyType == EnemyType.Melee || enemyType == EnemyType.Warrior)
        {
            AttackCollider.SetActive(true);
            m_Animator.SetBool("Attack", false);
        }
        else if (enemyType == EnemyType.Ranged)
        {
            GameObject bullet = Instantiate(Skill.Instance.Skill_Squidarrow, transform.position, transform.rotation);
            bullet.transform.GetChild(0).GetComponent<EnemyAttack>().enemy = this;
            m_Animator.SetBool("Attack", false);
        }
        else if (enemyType == EnemyType.Tank)
        {
            if (enemyRarity == EnemyRarity.Boss) // Boss行為
            {
                if (BossStage == 0 || BossStage == 2 || BossStage == 4 || BossStage == 6)
                {
                    m_Animator.SetInteger("Attack Type", 0);
                    sprint = StartCoroutine(Sprint());
                }
                else if (BossStage == 1)
                {
                    m_Animator.SetInteger("Attack Type", 1);
                    call = StartCoroutine(CallHerring(4));
                    BossStage = 2;
                }
                else if (BossStage == 3)
                {
                    AttackCD = 1.5f;
                    m_Animator.SetInteger("Attack Type", 1);
                    call = StartCoroutine(CallHerring(8));
                    BossStage = 4;
                }
                else if (BossStage == 5)
                {
                    AttackCD = 1f;
                    m_Animator.SetInteger("Attack Type", 1);
                    call = StartCoroutine(CallHerring(12));
                    BossStage = 6;
                }
            }
            else
            {
                sprint = StartCoroutine(Sprint());
            }
                
        }
        else if(enemyType == EnemyType.Mage)
        {
            int _time = Random.Range(3, 6);
            StartCoroutine(Mageattack(_time));
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
        if (enemyType == EnemyType.Warrior)
        {
            AttackCollider.SetActive(false);
            StartCoroutine(Runaway());
        }
        else
        {
            if (move == null)
                move = StartCoroutine(_Move());
            if (attackCD == null)
                attackCD = StartCoroutine(_AttackCD());
        }
        
    }
    public void EndBehit() {
        m_Animator.SetBool("BeHit", false);
    }

    IEnumerator _AttackCD()
    {
        float attackcd = AttackCD;
        if(attackcd != 0)
        {
            float rng = Random.Range(0.7f, 1.7f);
            attackcd *= rng;
        }
        yield return new WaitForSeconds(attackcd);
        canAttack = true;
        attackCD = null;
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
        hpUI.gameObject.SetActive(true);
        float nowtime = time;
        while(nowtime > 0.1f)
        {
            hpUI.value = nowtime / time;
            nowtime -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        hpUI.value = nowtime / time;
        if (nowtime > 0)
            yield return new WaitForSeconds(nowtime);
        hpUI.gameObject.SetActive(false);
    }

    // 衝刺
    IEnumerator Sprint()
    {
        yield return new WaitForSeconds(0.2f);
        AttackCollider.SetActive(true);

        float _time;
        if (enemyRarity == EnemyRarity.Boss)
            _time = 1f;
        else
            _time = 0.6f;
        while (Hp > 0 && _time > 0)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            rb.velocity = transform.forward * 2.5f * agent.speed;
            _time -= Time.fixedDeltaTime;
        }

        StopAction();
    }

    // 招喚小怪鯡魚
    IEnumerator CallHerring(int count)
    {
        if (MinionPosition.Length == 0 || count <= 0)
        {
            StopAction();
            yield break;
        }

        int lastposition = -1;
        float _time = 1f / count; // 在x秒內招喚完
        for(int i = 0 ; i < count ; i++)
        {
            int newposition = Random.Range(0, MinionPosition.Length);
            if (newposition == lastposition && MinionPosition.Length >= 1) //新招喚點與上次招喚點相同則重骰
                newposition = Random.Range(0, MinionPosition.Length);
            lastposition = newposition;
            yield return new WaitForSeconds(_time);
            GameObject _minion = Instantiate(LevelCtrl.Instance.EnemyHerring, MinionPosition[newposition].transform.position, MinionPosition[newposition].transform.rotation);
            Enemy _enemy = _minion.GetComponent<Enemy>();
            _enemy.enemyRarity = EnemyRarity.Minion;
            _enemy.money[0] = 0;
            _enemy.money[1] = 0;
        }
        StopAction();
    }

    // 逃離
    IEnumerator Runaway()
    {
        float moveRange = Random.Range(1f, 3f);

        // 計算逃跑方向（朝遠離目標的方向）
        Vector3 fleeDirection = (transform.position - target.position).normalized;
        Vector3 newDestination = transform.position + fleeDirection * moveRange;

        // 確保目標點在 NavMesh 上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDestination, out hit, moveRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // 還沒走到目標位置
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return new WaitForNextFrameUnit();
        }
        // 回到移動階段
        if (move == null)
            move = StartCoroutine(_Move());
        if (attackCD == null)
            attackCD = StartCoroutine(_AttackCD());
    }

    IEnumerator Mageattack(int time)
    {
        if (time <= 0)
            yield break;
        else
        {
            for (int i = 0; i < time; i++)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                StartCoroutine(Magicexplode());
            }
            m_Animator.SetBool("Attack", false);
            EndAttack();
        }
    }

    // 法師遠距離範圍攻擊
    IEnumerator Magicexplode()
    {
        float offset = 1.5f;
        Vector3 playerpos = PlayerCtrl.Instance.transform.position;
        Vector3 _offset = new (playerpos.x + Random.Range(-offset, offset), 0, playerpos.z + Random.Range(-offset, offset));
        GameObject a = null;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(_offset, out hit, 1, NavMesh.AllAreas))
            a = Instantiate(Skill.Instance.Skill_Magicexplode1, hit.position, Skill.Instance.Skill_Magicexplode1.transform.rotation);
        yield return new WaitForSecondsRealtime(1.5f);
        if(a != null)
            Destroy(a);
        GameObject b = Instantiate(Skill.Instance.Skill_Magicexplode2, _offset, Skill.Instance.Skill_Magicexplode2.transform.rotation);
        b.transform.Find("Collider").GetComponent<EnemyAttack>().enemy = this;
        Destroy(b, 0.5f);
    }

    // 中斷動作用
    public void StopAction()
    {
        if(enemyType == EnemyType.Tank)
        {
            if(sprint != null)
            {
                StopCoroutine(sprint);
                sprint = null;
                rb.velocity = Vector3.zero;
            }
            if(call != null)
            {
                call = null;
            }
            EndAttack();
        }
    }

}

public enum EnemyType 
{
    Melee, // 近戰 蜥蜴
    Ranged, // 遠程 魷魚
    Barrel, // 桶子
    Minion, // 招喚物 (鐘)
    Tank, // 近戰 鯡魚
    Warrior, // 高機動
    Mage, // 遠程法師 (範圍)
}
public enum EnemyRarity
{
    Normal,
    Minion, //招喚物
    Rare, //菁英
    Boss
}