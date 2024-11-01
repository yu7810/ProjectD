using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public ValueData valuedata;
    Rigidbody m_Rigidbody;
    public GameObject Player;
    public GameObject Skill_A;
    public ParticleSystem SmokeTrail;

    private void Start()
    {
        m_Rigidbody = Player.GetComponent<Rigidbody>();
    }

    public void UseSkill(int ID) {
        switch (ID) {
            case 1:
                Use_A();
                break;
            case 2:
                Use_B();
                break;
        }
    }


    void Use_A()
    {
        GameObject a = Instantiate(Skill_A, Player.transform.position, Player.transform.rotation);
        a.transform.localScale = new Vector3(valuedata.Skill[1].Size , 1f , valuedata.Skill[1].Size);
    }

    void Use_B()  //ฐ{มื
    {
        StartCoroutine(Flash());
    }
    IEnumerator Flash()
    {
        valuedata.canBehurt = false;
        SmokeTrail.Play();
        /*if (UpgradeSystem.GetComponent<UpgradeSystem>().UpgradeList[11].Lv > 0)
        {
            Health(0.5f);
        }*/
        Vector3 m_Input;
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            m_Input = Player.transform.forward;
        for (int i = 0; i < 20; i++)
        {
            m_Rigidbody.MovePosition(Player.transform.position + m_Input * Time.deltaTime * valuedata.Skill[2].Size);
            yield return new WaitForSeconds(0.01f);
        }
        SmokeTrail.Stop();
        valuedata.canBehurt = true;
    }

}
