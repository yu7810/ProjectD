using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaittoInstance : MonoBehaviour
{
    public float time;
    public GameObject target;

    private void OnEnable()
    {
        StartCoroutine(waittoActive());
    }

    IEnumerator waittoActive()
    {
        yield return new WaitForSecondsRealtime(time);
        target.SetActive(true);
    }
}
