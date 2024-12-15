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
    private WeaponFieldBase thisWeapon;
    public float dmg;
    float crit;
    public List<GameObject> passTarget = new List<GameObject>();
    public List<GameObject> Target = new List<GameObject>();

    public bool isBullet; //投射物，會往前飛
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
            if (ValueData.Instance.PassiveSkills[25])
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
        if(thisWeapon.ID == 8)
        {
            if (ValueData.Instance.SkillTag[thisSkill.ID].Contains(SkillTagType.Cold))
                _dmg *= 1 + (Target.Count * 0.2f);
        }

        //暴擊
        float randomvalue = Random.Range(0.01f, 1f);
        if (crit >= randomvalue)
        {
            _dmg *= ValueData.Instance.CritDmg;
            UICtrl.Instance.ShowDamage(_dmg, target.transform.position, true);
            //裝備5能力
            if (thisWeapon.ID == 5 && ValueData.Instance.SkillField[_fidleid].nowCD >= 0.3f)
            {
                float reducevalue = ValueData.Instance.SkillField[_fidleid].nowCD - 0.3f;
                ValueData.Instance.doCooldown(ValueData.Instance.SkillField[_fidleid], reducevalue);
            }
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

        GameObject P = Instantiate(AttackParticle, target.transform.position, AttackParticle.transform.rotation);
        if (isBullet)
            Destroy(this.gameObject);
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
                thisWeapon = ValueData.Instance.WeaponField[fidleid];
                thisSkill = ValueData.Instance.SkillField[fidleid];
                dmg = thisSkill.Damage;//多一層變數，避免複寫回SkillFieldBase
                crit = thisSkill.Crit;
            }
        }
    }

    IEnumerator Bullet()
    {
        float speed = 10f * thisSkill.Speed * ValueData.Instance.BulletSpeed;
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
