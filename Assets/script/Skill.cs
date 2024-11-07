using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public GameObject Player;
    public GameObject Skill_A;
    public ParticleSystem SmokeTrail;

    private void Start()
    {
        m_Rigidbody = Player.GetComponent<Rigidbody>();
    }

    public void UseSkill(int Skillid, int Weaponid) {
        switch (Skillid) {
            case 1:
                Use_A(Weaponid);
                break;
            case 2:
                Use_B(Weaponid);
                break;
        }
    }


    void Use_A(int Weaponid)
    {
        GameObject a = Instantiate(Skill_A, Player.transform.position, Player.transform.rotation);
        a.GetComponent<PlayerAttack>().thisWeapon = ValueData.Instance.Weapon[Weaponid];
        a.GetComponent<PlayerAttack>().thisSkill = ValueData.Instance.Skill[1];
        float _size = ValueData.Instance.Skill[1].Size;
        a.transform.localScale = new Vector3(_size, 1, _size);
    }

    void Use_B(int Weaponid)  //ฐ{มื
    {
        StartCoroutine(Flash());
    }
    IEnumerator Flash()
    {
        ValueData.Instance.canBehurt = false;
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
            m_Rigidbody.MovePosition(Player.transform.position + m_Input * Time.deltaTime * ValueData.Instance.Skill[2].Size);
            yield return new WaitForSeconds(0.01f);
        }
        SmokeTrail.Stop();
        ValueData.Instance.canBehurt = true;
    }

}
