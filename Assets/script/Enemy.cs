using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //public string Type;
    public float Hp;
    public float maxHp;
    public float Attack;
    public float EXP;//怪物提供的經驗值

    public GameObject Mesh;
    private Color C;
    private GameObject UICtrl;

    private void OnEnable(){
        UICtrl = GameObject.Find("UICtrl");
        Hp = maxHp;
        if (gameObject.tag == "Barrel"){
            C = Mesh.transform.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
        }
        else if (gameObject.tag == "Enemy") {
            C = Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.GetColor("_EmissionColor");
        }
    }

    public void Die() {
        UICtrl.GetComponent<UICtrl>().GetEXP(EXP);
        Destroy(transform.gameObject);
    }

    public void beAttack() {
        StartCoroutine(beAttackEffect());
    }

    IEnumerator beAttackEffect() {
        //桶子是用MeshRenderer，怪物是用SkinnedMeshRenderer
        if (gameObject.tag == "Barrel")
        {
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.188f, 0.188f, 0.188f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", C);
        }
        else if (gameObject.tag == "Enemy"){
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.65f, 0.65f, 0.65f));
            yield return new WaitForSeconds(0.2f);
            Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", C);
        }
    }

}
