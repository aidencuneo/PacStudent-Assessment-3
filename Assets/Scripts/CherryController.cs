using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    // Singleton
    public static CherryController me;

    public GameObject cherryPrefab;

    float lastDestroyedTime = 0;
    GameObject lastCherry = null;

    void Start()
    {
        me = this;
        lastDestroyedTime = Time.time;
    }

    void Update()
    {
        // Spawn new cherries
        if (Time.time - lastDestroyedTime >= 5 && lastCherry == null)
            SpawnCherry();
    }

    void SpawnCherry()
    {
        lastCherry = Instantiate(cherryPrefab);
    }

    public void DestroyCherry()
    {
        Destroy(lastCherry);
        lastCherry = null;
        lastDestroyedTime = Time.time;

        // Add points?
    }
}
