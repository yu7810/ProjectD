using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCount : MonoBehaviour
{

    public GameObject UICtrl;
    public GameObject Player;
    public GameObject Enemy_A;
    public GameObject Enemy_B;
    public GameObject Enemy_C;
    public int Count;//已生敵人數量
    public ValueData valuedata;


    private void OnEnable(){
        Count = 0;
        StartCoroutine(InstanceCount());
    }

    IEnumerator InstanceCount() {
        if (valuedata.EnemyTimer <= 0) {
            yield return new WaitForSeconds(1);
            yield break;
        }
        yield return new WaitForSeconds(valuedata.EnemyTimer);
        int m = Random.Range(0, 4);
        float x;
        float z;
        if (m == 0){
            x = Random.Range(-10, 10);
            z = 10;
        }
        else if (m == 1) {
            x = Random.Range(-10, 10);
            z = -10;
        }
        else if (m == 2){
            x = 10;
            z = Random.Range(-10, 10);
        }
        else{
            x = -10;
            z = Random.Range(-10, 10);
        }
        GameObject a = Instantiate(Enemy_A, new Vector3(x,0.5f,z) , Enemy_A.transform.rotation);
        a.GetComponent<Enemy>().target = Player.transform;
        Count += 1;

        //遠程怪
        if (Count >= 30 && (Count % 10) == 0) {
            GameObject b = Instantiate(Enemy_B, new Vector3(x, 0.5f, z), Enemy_B.transform.rotation);
            b.GetComponent<Enemy>().target = Player.transform;
            if (valuedata.EnemyTimer > 0.2f)
                valuedata.EnemyTimer -= 0.05f;
            else
                valuedata.EnemyTimer = 0.2f;
            Count += 1;
        }

        //坦克怪
        if (Count >= 100 && valuedata.EnemyTimer == 0.2f)
        {
            GameObject c = Instantiate(Enemy_C, new Vector3(x, 0.5f, z), Enemy_C.transform.rotation);
            valuedata.EnemyTimer = 0.3f;
            c.GetComponent<Enemy>().target = Player.transform;
            Count += 1;
        }

        //StartCoroutine(InstanceCount());
    }

}
