using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyThis : MonoBehaviour
{
    public float LifeTime;
    private void OnEnable()
    {
        Destroy(gameObject, LifeTime);
    }
}
