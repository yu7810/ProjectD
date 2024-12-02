using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //public bool HitSlowMotion;
    public GameObject AttackParticle;
    private SkillFieldBase thisSkill;
    private int _fidleid = -1;
    private WeaponFieldBase thisWeapon;
    public float dmg;
    float crit;
    public GameObject[] passTarget;
    public bool isBullet; //投射物，會往前飛
    private Vector3 startPos;//投射物用，紀錄發射位置

    private void Start()
    {
        if (isBullet)
        {
            startPos = transform.position;
            StartCoroutine(Bullet());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Barrel" || other.transform.tag == "Enemy" || other.transform.tag == "Bell")
        {
            //黏刀，改時間效果不明顯，且會影響到粒子表現
            /*if (!HitSlowMotion) {
                StartCoroutine(SlowMotion(0.05f));
            }*/
            for(int i=0; i< passTarget.Length; i++)
            {
                if (passTarget[i] == other.gameObject)
                    return;
            }

            if (!other.transform.GetComponent<Enemy>().canBeHit)
                return;

            if(isBullet && ValueData.Instance.PassiveSkills[21]) //天賦21
            {
                if (startPos != Vector3.zero)
                {
                    float distance = Vector3.Distance(startPos, transform.position);
                    dmg += distance;
                }
            }

            //暴擊
            float randomvalue = Random.Range(0.01f, 1f);
            if (crit >= randomvalue) {
                dmg *= ValueData.Instance.CritDmg;
                UICtrl.Instance.ShowDamage(dmg, other.transform.position, true);
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
                UICtrl.Instance.ShowDamage(dmg, other.transform.position,false);
            other.transform.GetComponent<Enemy>().Hurt(dmg);

            GameObject P = Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
            Destroy(P, 1f);
            if (isBullet)
                Destroy(this.gameObject);
        }
        if (other.transform.tag == "EnemyAttack") {
            /*if (_UpgradeSystem.GetComponent<UpgradeSystem>().UpgradeList[4].Lv >= 1) {
                GameObject P = Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
                Destroy(P, 1f);
                Destroy(other.gameObject);
            }*/
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
                thisWeapon = ValueData.Instance.WeaponField[fidleid];
                thisSkill = ValueData.Instance.SkillField[fidleid];
                dmg = thisSkill.Damage;//多一層變數，避免複寫回SkillFieldBase
                crit = thisSkill.Crit;
            }
        }
    }

    IEnumerator Bullet()
    {
        float speed = 0.12f * thisSkill.Speed;
        while(this.gameObject)
        {
            yield return new WaitForSeconds(0.01f);
            transform.localPosition += new Vector3(0, 0, speed);
        }
    }


    /*
    IEnumerator SlowMotion(float time) {
        HitSlowMotion = true;
        Time.timeScale = 0.55f;
        yield return new WaitForSeconds(time);
        Time.timeScale = 1;
        HitSlowMotion = false;
    }*/

}
