using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : MonoBehaviour
{
    // This script acts as a helper for generating the manual level layout
    // First, I run ManualLevelHelper to generate the top left corner, then I manually rotate the
    // pieces correctly, and finally, I run this script to flip it across the x or y axis

    public Transform otherTransform;

    void Start()
    {
        FlipHorizontal();
        // FlipVertical();
    }

    void FlipHorizontal()
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

    void FlipVertical()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            GameObject n = Instantiate(child.gameObject);
            n.transform.position = new(child.position.x, -child.position.y);

            float newAngle = child.rotation.eulerAngles.z switch
            {
                0 => 90,
                90 => 0,
                180 => 270,
                270 => 180,
                _ => child.rotation.eulerAngles.z,
            };

            if (child.name.Contains("Corner"))
                n.transform.rotation = Quaternion.Euler(0, 0, newAngle);

            n.transform.SetParent(otherTransform, true);
        }
    }
}
