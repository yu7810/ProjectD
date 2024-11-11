using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    //public bool HitSlowMotion;
    public GameObject AttackParticle;
    public SkillFieldBase thisSkill;


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
                Debug.Log(dmg + "暴擊!!" + randomvalue);
            }
            else
                Debug.Log(dmg);
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

    /*
    IEnumerator SlowMotion(float time) {
        HitSlowMotion = true;
        Time.timeScale = 0.55f;
        yield return new WaitForSeconds(time);
        Time.timeScale = 1;
        HitSlowMotion = false;
    }*/

}
