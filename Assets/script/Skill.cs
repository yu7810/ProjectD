using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    private static Skill instance;
    Rigidbody m_Rigidbody;
    public GameObject Skill_A;
    public ParticleSystem SmokeTrail;

    void Awake()
    {
        // �p�G��ҩ|���s�b�A�]�m�����󬰰ߤ@��ҨëO�d�b����������
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �p�G�w�g�s�b�@�ӹ�ҡA�P����e����
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        m_Rigidbody = ValueData.Instance.Player.GetComponent<Rigidbody>();
    }

    public void UseSkill(int Skillid, int Fieldid) {
        switch (Skillid) {
            case 1:
                Use_A(Fieldid);
                break;
            case 2:
                Use_B(Fieldid);
                break;
        }
    }


    void Use_A(int Fieldid)
    {
        GameObject a = Instantiate(Skill_A, ValueData.Instance.Player.transform.position, ValueData.Instance.Player.transform.rotation);
        a.transform.Find("Collider").gameObject.GetComponent<PlayerAttack>().fidleid = Fieldid;
        float _size = ValueData.Instance.SkillField[Fieldid].Size;
        a.transform.localScale = new Vector3(_size, 1, _size);
    }

    void Use_B(int Weaponid)  //�{��
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
            m_Input = ValueData.Instance.Player.transform.forward;
        for (int i = 0; i < 20; i++)
        {
            m_Rigidbody.MovePosition(ValueData.Instance.Player.transform.position + m_Input * Time.deltaTime * ValueData.Instance.Skill[2].Size);
            yield return new WaitForSeconds(0.01f);
        }
        SmokeTrail.Stop();
        ValueData.Instance.canBehurt = true;
    }

}
