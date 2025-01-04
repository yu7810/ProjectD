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
    float crit;
    public List<GameObject> passTarget = new List<GameObject>();
    public List<GameObject> Target = new List<GameObject>();

    public bool isBullet; //投射物，會往前飛
    public bool canSplit = true; // 投射物可以分裂
    private Vector3 startPos;//投射物用，紀錄發射位置

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
            if (ValueData.Instance.PassiveSkills[25]) // 天賦25
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
        else if (other.transform.tag == "EnemyAttack")
        {
            if (ValueData.Instance.PassiveSkills[32]) // 天賦32
            {
                if (!ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Attack))
                    return;
                if (other.GetComponent<EnemyAttack>().enemyType == EnemyType.Ranged)
                {
                    Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
                    Destroy(other.transform.parent.gameObject);
                }
            }
        }


    }

    void doDamage(GameObject target)
    {
        float _dmg = dmg; // 多一層，避免傷害疊加

        if (isBullet && ValueData.Instance.PassiveSkills[21]) //天賦21
        {
            if (startPos != Vector3.zero)
            {
                float distance = Vector3.Distance(startPos, transform.position);
                _dmg += distance * 2;
            }
        }
        if(ValueData.Instance.isHaveweaponid(_fidleid,8)) // 武器8
        {
            if (ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Cold))
                _dmg *= 1 + ((Target.Count -1) * 0.2f);
        }

        //暴擊
        float randomvalue = Random.Range(0.01f, 1f);
        if (crit >= randomvalue)
        {
            _dmg *= thisSkill.CritDmg;
            UICtrl.Instance.ShowDamage(_dmg, target.transform.position, true);

            //天賦16能力
            if (ValueData.Instance.PassiveSkills[16])
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != _fidleid)
                        ValueData.Instance.doCooldown(ValueData.Instance.SkillField[i], 1f);
                }
            }
        }
        else
            UICtrl.Instance.ShowDamage(_dmg, target.transform.position, false);
        target.transform.GetComponent<Enemy>().Hurt(_dmg, _fidleid);

        Instantiate(AttackParticle, target.transform.position, AttackParticle.transform.rotation);

        if (isBullet)
        {
            if(ValueData.Instance.PassiveSkills[17] && canSplit) // 天賦17 (投射物分裂)
            {
                Quaternion forward = transform.rotation;
                Quaternion leftDirection = Quaternion.Euler(0, -30, 0) * forward;
                Quaternion rightDirection = Quaternion.Euler(0, 30, 0) * forward;
                Vector3 pos = transform.position;
                pos += ValueData.Instance.Player.transform.forward * 0.7f;
                pos.y = 0;
                Skill.Instance.UseSkill(thisSkill.ID, _fidleid, pos, leftDirection, 1);
                Skill.Instance.UseSkill(thisSkill.ID, _fidleid, pos, rightDirection, 1);
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
                dmg = thisSkill.Damage;//多一層變數，避免複寫回SkillFieldBase
                crit = thisSkill.Crit;
            }
        }
    }

    IEnumerator Bullet()
    {
        float speed = 5f * thisSkill.Speed * ValueData.Instance.BulletSpeed;
        while(this.gameObject)
        {
            yield return new WaitForSeconds(0.01f);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
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
