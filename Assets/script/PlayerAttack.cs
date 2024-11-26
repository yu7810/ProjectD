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

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Barrel" || other.transform.tag == "Enemy")
        {
            //黏刀，改時間效果不明顯，且會影響到粒子表現
            /*if (!HitSlowMotion) {
                StartCoroutine(SlowMotion(0.05f));
            }*/
            if (!other.transform.GetComponent<Enemy>().canBeHit)
                return;

            float dmg = thisSkill.Damage;//多一層變數，避免複寫回SkillFieldBase
            //暴擊
            float randomvalue = Random.Range(0.01f, 1f);
            if (thisSkill.Crit >= randomvalue) {
                dmg *= ValueData.Instance.CritDmg;
                UICtrl.Instance.ShowDamage(dmg, other.transform.position, true);
                //裝備5能力
                if (thisWeapon.ID == 5 && ValueData.Instance.SkillField[_fidleid].nowCD >= 0.3f)
                {
                    float reducevalue = ValueData.Instance.SkillField[_fidleid].nowCD - 0.3f;
                    ValueData.Instance.doCooldown(ValueData.Instance.SkillField[_fidleid], reducevalue);
                }
                    
            }
            else
                UICtrl.Instance.ShowDamage(dmg, other.transform.position,false);
            other.transform.GetComponent<Enemy>().Hurt(dmg);
            

            GameObject P = Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
            Destroy(P, 1f);
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
            }
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
