using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    //public GameObject PlayerCtrl;
    //public GameObject UICtrl;
    public bool HitSlowMotion;
    public GameObject AttackParticle;
    public int Skill_ID;//1=�ޯ�A�A�̦�����
    float Dmg;

    void Start()
    {
        HitSlowMotion = false;
    }

    private void OnEnable()
    {
        //�̧ޯ�ID�M�w�ˮ`
        if (Skill_ID == 1)
            Dmg = UICtrl.Attack * UICtrl.Skill_A_DmgAdd;
        else
            Dmg = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.tag == "Barrel" || other.gameObject.transform.tag == "Enemy")
        {
            //�H�M�A��ɶ��ĪG������A�B�|�v�T��ɤl��{
            /*if (!HitSlowMotion) {
                StartCoroutine(SlowMotion(0.05f));
            }*/
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

    }

    IEnumerator SlowMotion(float time) {
        HitSlowMotion = true;
        Time.timeScale = 0.55f;
        yield return new WaitForSeconds(time);
        Time.timeScale = 1;
        HitSlowMotion = false;
    }

}
