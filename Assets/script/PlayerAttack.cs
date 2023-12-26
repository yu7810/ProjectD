using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    //public bool HitSlowMotion;
    public GameObject AttackParticle;
    public int Skill_ID;//1=技能A，依此類推
    float Dmg;
    public GameObject _UpgradeSystem;

    private void OnEnable()
    {
        _UpgradeSystem = GameObject.Find("Upgrade System");
    }

    void DmgUpdate()
    {
        //依技能ID決定數值
        if (Skill_ID == 1)
            Dmg = UICtrl.Attack * UICtrl.Skill_A_DmgAdd;
        else if (Skill_ID == 2)
            Dmg = UICtrl.Attack * UICtrl.Skill_B_DmgAdd;
        else
            Dmg = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.tag == "Barrel" || other.gameObject.transform.tag == "Enemy")
        {
            //黏刀，改時間效果不明顯，且會影響到粒子表現
            /*if (!HitSlowMotion) {
                StartCoroutine(SlowMotion(0.05f));
            }*/
            if (!other.transform.GetComponent<Enemy>().canBeHit)
                return;
            DmgUpdate();
            if (other.transform.GetComponent<Enemy>().Hp > Dmg) {
                other.transform.GetComponent<Enemy>().Hp -= Dmg;
                other.transform.GetComponent<Enemy>().beAttack();
            }
            else
            {
                other.transform.GetComponent<Enemy>().Hp = 0;
                other.transform.GetComponent<Enemy>().Die();
            }
            GameObject P = Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
            Destroy(P, 1f);
        }
        if (other.gameObject.transform.tag == "EnemyAttack") {
            if (_UpgradeSystem.GetComponent<UpgradeSystem>().UpgradeList[4].Lv >= 1) {
                GameObject P = Instantiate(AttackParticle, other.transform.position, AttackParticle.transform.rotation);
                Destroy(P, 1f);
                Destroy(other.gameObject);
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
