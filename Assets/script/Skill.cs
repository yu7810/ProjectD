using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public UICtrl UIctrl;
    public GameObject Player;
    public GameObject Skill_A;

    public void Use_A()
    {
        if (UIctrl.Skill_A_CD > 0)
            return;
        //©I•sßﬁØ‡CD
        GameObject a = Instantiate(Skill_A, Player.transform.position, Player.transform.rotation);
        float size = UICtrl.Skill_A_Size;
        a.transform.localScale = new Vector3(size, 1, size);
    }
}
