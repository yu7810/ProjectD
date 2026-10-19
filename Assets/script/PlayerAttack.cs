using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    public GameObject AttackParticle;
    private SkillFieldBase thisSkill;
    private int _fidleid = -1;
    public float dmg;
    public float damageMultiplier = 1f; // Snapshot for this cast, independent of live Rage.
    float crit;
    public List<GameObject> passTarget = new List<GameObject>();
    public List<GameObject> Target = new List<GameObject>();

    public bool isBullet; //投射物，會往前飛
    public bool canSplit = true; // 投射物可以分裂
    private Vector3 startPos;//投射物用，紀錄發射位置
    private Collider[] homingCandidates;
    private float homingRange;
    private float homingTurnSpeed;

    private void Start()
    {
        if (isBullet)
        {
            startPos = transform.position;
            StartCoroutine(Bullet());
        }
        if(ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Range))
        {
            StartCoroutine(Range());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Barrel" || other.transform.tag == "Enemy" || other.transform.tag == "Bell")
        {
            if (passTarget.Contains(other.gameObject)) // 要無視的目標
                return;

            if (!other.transform.GetComponent<Enemy>().canBeHit)
                return;

            if (ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Range))
                Target.Add(other.gameObject);
            else
                doDamage(other.gameObject);

        }
        else if (other.transform.tag == "Wall" && isBullet)
        {
            if (ValueData.Instance.isHaveweaponid(_fidleid, 10)) //武器10 反彈
            {
                Vector3 direction = transform.forward;

                // 計算碰撞法線
                Vector3 collisionPoint = transform.position; // 子彈當前位置
                Vector3 normal = other.ClosestPoint(collisionPoint) - collisionPoint;
                normal = normal.normalized;

                // 計算反射方向
                direction = Vector3.Reflect(direction, normal);

                // 更新子彈朝向
                transform.forward = direction;
            }
            else
            {
                GameObject P = Instantiate(AttackParticle, transform.position, AttackParticle.transform.rotation);
                P.transform.localScale = new Vector3(P.transform.localScale.x * thisSkill.Size, P.transform.localScale.y * thisSkill.Size, P.transform.localScale.z * thisSkill.Size);
                Destroy(gameObject);
            }
        }
        else if (other.transform.tag == "EnemyAttack" || other.transform.tag == "PlayerAttack")
        {
            // Extinguisher only intercepts hostile projectiles for this skill slot.
            if (other.CompareTag("EnemyAttack") && _fidleid >= 0
                && ValueData.Instance.isHaveweaponid(_fidleid, 27)
                && other.TryGetComponent<EnemyAttack>(out var enemyAttack)
                && enemyAttack.enemyType == EnemyType.Ranged)
            {
                if (!other.gameObject.activeInHierarchy || passTarget.Contains(other.gameObject))
                    return;

                // Disable immediately so another collider cannot hit the same projectile
                // before Unity processes Destroy at the end of the frame.
                GameObject projectile = other.transform.parent != null
                    ? other.transform.parent.gameObject : other.gameObject;
                projectile.SetActive(false);
                doDamage(other.gameObject);
                Destroy(projectile);
                return;
            }

            if (ValueData.Instance.PassiveSkills[9]) // 天賦9
            {
                if (!ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Attack) || ValueData.Instance.Rage != ValueData.Instance.maxRage)
                    return;
                if ((other.transform.tag == "EnemyAttack" && other.GetComponent<EnemyAttack>().enemyType == EnemyType.Ranged) || (other.transform.tag == "PlayerAttack" && other.GetComponent<PlayerAttack>().isBullet == true))
                {
                    doDamage(other.gameObject);
                    Destroy(other.transform.parent.gameObject);

                    // 天賦0 獲得盛怒
                    if (ValueData.Instance.PassiveSkills[0])
                        ValueData.Instance.GetRage(1);
                    // 天賦27 擊殺回魔
                    if (ValueData.Instance.PassiveSkills[27])
                        ValueData.Instance.GetAp(ValueData.Instance.maxAP / 5);
                }
            }
        }


    }

    void doDamage(GameObject target)
    {
        float _dmg = dmg; // 多一層，避免傷害疊加

        if (ValueData.Instance.isHaveweaponid(_fidleid, 12)) //武器12狙擊
        {
            Vector3 playerpos = ValueData.Instance.Player.transform.position;
            float distance = Vector3.Distance(playerpos, transform.position);
            _dmg += distance * 2;
        }
        if(ValueData.Instance.isHaveweaponid(_fidleid,8)) // 武器8
        {
            if (ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Cold))
                _dmg *= 1 + ((Target.Count -1) * 0.2f);
        }

        //暴擊
        _dmg *= damageMultiplier;
        float randomvalue = Random.Range(0f, 1f);
        if (crit > randomvalue && thisSkill.ID != 8) // 喪鐘不會暴擊
        {
            _dmg *= thisSkill.CritDmg;
            UICtrl.Instance.ShowDamage(_dmg, target.transform.position, true);

            if (ValueData.Instance.isHaveweaponid(_fidleid, 13)) // 武器13
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != _fidleid)
                        ValueData.Instance.doCooldown(ValueData.Instance.SkillField[i], 1f);
                }
            }
            //天賦0
            if (ValueData.Instance.PassiveSkills[0])
            {
                ValueData.Instance.GetRage(1);
            }
            // 天賦19 被消耗
            if (ValueData.Instance.PassiveSkills[19])
            {
                ValueData.Instance.add_ReloadCrit = 0;
                ValueData.Instance.PlayerValueUpdate();
                ValueData.Instance.SkillFieldValueUpdate();
            }
            //裝備18 暴擊時施放
            for(int i=0; i<3; i++)
            {
                if (_fidleid != i && ValueData.Instance.isHaveweaponid(i, 18))
                {
                    if(Skill.Instance.CastOnCritical())
                        Skill.Instance.UseSkill(ValueData.Instance.SkillField[i].ID, i);
                }
            }
        }
        else // 未暴擊
        {
            UICtrl.Instance.ShowDamage(_dmg, target.transform.position, false);
            if(ValueData.Instance.PassiveSkills[3])
                ValueData.Instance.GetRage(0.2f);
        }

        if (target.transform.tag == "Barrel" || target.transform.tag == "Enemy" || target.transform.tag == "Bell")
            target.transform.GetComponent<Enemy>().Hurt(-_dmg, _fidleid);
        if (AttackParticle != null)
            Instantiate(AttackParticle, target.transform.position, AttackParticle.transform.rotation);

        if (isBullet)
        {
            if(ValueData.Instance.isHaveweaponid(_fidleid, 11) && canSplit) // 武器11 投射物分裂
            {
                Quaternion forward = transform.rotation;
                Quaternion leftDirection = Quaternion.Euler(0, -30, 0) * forward;
                Quaternion rightDirection = Quaternion.Euler(0, 30, 0) * forward;
                Vector3 pos = transform.position;
                pos += ValueData.Instance.Player.transform.forward * 0.7f;
                pos.y = 0;
                Skill.Instance.UseSkill(thisSkill.ID, _fidleid, pos, leftDirection, 1, damageMultiplier);
                Skill.Instance.UseSkill(thisSkill.ID, _fidleid, pos, rightDirection, 1, damageMultiplier);
            }

            Destroy(this.gameObject);
        }
            
    }

    public int fidleid
    {
        get { return _fidleid; }
        set
        {
            if (value < 0)
                return;
            if(_fidleid != value)
            {
                _fidleid = value;
                thisSkill = ValueData.Instance.SkillField[fidleid];
                crit = thisSkill.Crit;
                if(dmg == 0)
                    dmg = thisSkill.Damage;//多一層變數，避免複寫回SkillFieldBase
            }
        }
    }

    // Pulses share the normal hit/crit/weapon/talent pipeline without a trigger collider.
    public void Pulse(int fieldId, float damage, float multiplier, List<GameObject> targets)
    {
        _fidleid = fieldId;
        thisSkill = ValueData.Instance.SkillField[fieldId];
        dmg = damage;
        crit = thisSkill.Crit;
        damageMultiplier = multiplier;
        Target.Clear();
        Target.AddRange(targets);
        foreach (var target in targets)
        {
            if (target == null || !target.activeInHierarchy) continue;
            if (ValueData.Instance.isHaveweaponid(fieldId, 27) && target.CompareTag("EnemyAttack")
                && target.TryGetComponent<EnemyAttack>(out var projectile) && projectile.enemyType == EnemyType.Ranged)
            {
                var root = target.transform.parent != null ? target.transform.parent.gameObject : target;
                root.SetActive(false);
                doDamage(target);
                Destroy(root);
                continue;
            }
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null && enemy.canBeHit && (enemy.Hp > 0f || enemy.CompareTag("Bell")))
                doDamage(target);
        }
        Target.Clear();
    }

    IEnumerator Bullet()
    {
        float speed = 5f * thisSkill.Speed;
        if (ValueData.Instance.isHaveweaponid(_fidleid, ValueData.TrackingWeaponId))
        {
            WeaponBase weapon = ValueData.Instance.Weapon[ValueData.TrackingWeaponId];
            homingRange = Mathf.Max(0f, weapon.HomingRange);
            homingTurnSpeed = Mathf.Max(0f, weapon.HomingTurnSpeed);
            if (homingRange > 0f && homingTurnSpeed > 0f)
                homingCandidates = new Collider[32];
        }
        while(this.gameObject)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            if (homingCandidates != null)
                SteerTowardNearestEnemy(Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void SteerTowardNearestEnemy(float deltaTime)
    {
        Vector3 origin = transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, homingRange, homingCandidates,
            ~0, QueryTriggerInteraction.Collide);
        Collider[] candidates = homingCandidates;
        // A full buffer may omit the closest enemy. Only allocate in crowded areas.
        if (count == candidates.Length)
        {
            candidates = Physics.OverlapSphere(origin, homingRange, ~0, QueryTriggerInteraction.Collide);
            count = candidates.Length;
        }

        Enemy nearest = null;
        float nearestDistance = homingRange * homingRange;
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = candidates[i].GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.isActiveAndEnabled || !enemy.CompareTag("Enemy")
                || enemy.Hp <= 0f || !enemy.canBeHit || passTarget.Contains(enemy.gameObject))
                continue;

            float distance = (enemy.transform.position - origin).sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        if (nearest == null)
            return;

        Vector3 direction = nearest.transform.position - origin;
        direction.y = 0f; // Keep projectiles on the combat plane.
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), homingTurnSpeed * deltaTime);
    }
    IEnumerator Range()
    {
        yield return new WaitForFixedUpdate();
        if (Target.Count > 0)
        { 
            foreach(GameObject target in Target)
            {
                doDamage(target);
            }
        }
    }
}
