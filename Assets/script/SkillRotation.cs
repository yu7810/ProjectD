using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRotation : MonoBehaviour
{
    public float rotSpeed_X;
    public float rotSpeed_Y;
    public float rotSpeed_Z;
    public float globalSpeed = 1f;
    public GameObject Target;

    void Update()
    {
        transform.Rotate(new Vector3(rotSpeed_X, rotSpeed_Y, rotSpeed_Z) * globalSpeed * Time.deltaTime);
        transform.position = new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z);
    }

}
