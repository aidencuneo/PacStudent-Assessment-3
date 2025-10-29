using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    // Singleton
    public static CherryController me;

    public GameObject cherryPrefab;
    public GameObject stillCherryPrefab; // Level 2 feature

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
        // In level 2, choose a random spot on the map to place the stationary cherry (diamond)
        if (HUD.me.level == 2)
        {
            // 10 tries
            for (int i = 0; i < 10; ++i)
            {
                int x = Random.Range(0, LevelGenerator.me.realMapWidth);
                int y = Random.Range(0, LevelGenerator.me.realMapHeight);
                Vector3 pos = LevelGenerator.me.MapToWorldPos(x, y);

                // Is this space valid and far enough from the player?
                if (Util.AwayFromPlayer(pos, 8) && LevelGenerator.me.IsEmpty(x, y))
                {
                    // Spawn the diamond
                    lastCherry = Instantiate(stillCherryPrefab);
                    lastCherry.transform.position = pos;
                    break;
                }
            }
        }

        // Level 1
        else
        {
            lastCherry = Instantiate(cherryPrefab);
        }
    }

    public void DestroyCherry()
    {
        lastCherry = null;
        lastDestroyedTime = Time.time;
    }
}
