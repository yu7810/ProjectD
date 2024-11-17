using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public GameObject hightlightVFX;
    [SerializeField]
    public PrizeBase Prize;//獎勵類型
    public int Level;//關卡編號

    private void Start()
    {
        hightlightVFX.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            hightlightVFX.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            hightlightVFX.SetActive(false);
    }
}
