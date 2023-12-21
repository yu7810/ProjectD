using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCount : MonoBehaviour
{

    public GameObject UICtrl;
    public GameObject Player;
    public GameObject Enemy_A;
    

    private void OnEnable(){
        StartCoroutine(InstanceCount());
    }

    IEnumerator InstanceCount() {
        if (UICtrl.GetComponent<UICtrl>().EnemyTimer <= 0) {
            yield return new WaitForSeconds(1f);
            yield break;
        }
        yield return new WaitForSeconds(UICtrl.GetComponent<UICtrl>().EnemyTimer);
        int m = Random.Range(0, 4);
        float x;
        float z;
        if (m == 0){
            x = Random.Range(9, 10);
            z = Random.Range(9, 10);
        }
        else if (m == 1) {
            x = Random.Range(9, 10);
            z = Random.Range(-9, -10);
        }
        else if (m == 2){
            x = Random.Range(-9, -10);
            z = Random.Range(9, 10);
        }
        else{
            x = Random.Range(-9, -10);
            z = Random.Range(-9, -10);
        }
        GameObject a = Instantiate(Enemy_A, new Vector3(x,0.5f,z) , Enemy_A.transform.rotation);
        a.GetComponent<Move>().target = Player.transform;
        StartCoroutine(InstanceCount());
    }

}
