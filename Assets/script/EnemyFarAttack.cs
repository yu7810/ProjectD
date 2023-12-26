using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFarAttack : MonoBehaviour
{
    public GameObject Bullet;
    private void OnEnable()
    {
        StartCoroutine(FarAttack());
    }

    IEnumerator FarAttack()
    {
        yield return new WaitForSeconds(3f);
        Instantiate(Bullet, transform.position, transform.rotation);
        StartCoroutine(FarAttack());
    }
}
