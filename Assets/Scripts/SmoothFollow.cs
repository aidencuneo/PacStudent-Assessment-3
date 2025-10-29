using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public float speed = 0.05f;

    void LateUpdate()
    {
        if (!target)
            return;

        Vector3 targetPos = target.position;
        targetPos.z = -10;

        transform.position = (1 - speed) * transform.position + speed * targetPos;
    }
}
