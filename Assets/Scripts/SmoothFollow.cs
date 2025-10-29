using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public static SmoothFollow me;
    public Transform target;

    Rigidbody2D rb;
    public Camera cam;

    void Awake()
    {
        me = this;
        cam = GetComponent<Camera>();
        rb = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (!target)
            return;

        Vector3 targetPos = target.position;
        targetPos.z = -10;

        transform.position = 0.8f * transform.position + 0.2f * targetPos;
    }
}
