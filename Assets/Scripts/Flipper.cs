using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : MonoBehaviour
{
    public Transform otherTransform;

    void Start()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            GameObject n = Instantiate(child.gameObject);
            n.transform.position = new(-child.position.x, child.position.y);

            float newAngle = child.rotation.eulerAngles.z switch
            {
                90 => 180,
                180 => 90,
                270 => 0,
                0 => 270,
                _ => child.rotation.eulerAngles.z,
            };

            if (child.name.Contains("Corner"))
                n.transform.rotation = Quaternion.Euler(0, 0, newAngle);

            n.transform.SetParent(otherTransform, true);
        }
    }
}
