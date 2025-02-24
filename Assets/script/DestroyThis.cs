using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyThis : MonoBehaviour
{
    public float LifeTime;
    public ParticleSystem particel;

    private void OnEnable()
    {
        if (particel)
        {
            var mainModule = particel.main;
            mainModule.startLifetime = LifeTime;
        }
            
        Destroy(gameObject, LifeTime);
    }
}
