using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public ValueData valuedata;
    public GameObject Player;
    public GameObject Skill_A;

    public void UseSkill(int ID) {
        switch (ID) {
            case 1:
                //Debug.Log("�ϥΡG" + valuedata.Skill[ID].Name);
                Use_A();
                break;
        }
    }


    void Use_A()
    {
        GameObject a = Instantiate(Skill_A, Player.transform.position, Player.transform.rotation);
        a.transform.localScale = new Vector3(valuedata.Skill[1].Size , 1f , valuedata.Skill[1].Size);
    }
}
