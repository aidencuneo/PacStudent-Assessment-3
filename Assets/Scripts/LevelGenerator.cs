using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public bool shouldGenerateLevel = true;
    public GameObject[] prefabs;

    // Level map to generate (top left corner)
    int[,] levelMap =
    {
        {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 7},
        {2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4},
        {2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 4},
        {2, 6, 4, 0, 0, 4, 5, 4, 0, 0, 0, 4, 5, 4},
        {2, 5, 3, 4, 4, 3, 5, 3, 4, 4, 4, 3, 5, 3},
        {2, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
        {2, 5, 3, 4, 4, 3, 5, 3, 3, 5, 3, 4, 4, 4},
        {2, 5, 3, 4, 4, 3, 5, 4, 4, 5, 3, 4, 4, 3},
        {2, 5, 5, 5, 5, 5, 5, 4, 4, 5, 5, 5, 5, 4},
        {1, 2, 2, 2, 2, 1, 5, 4, 3, 4, 4, 3, 0, 4},
        {0, 0, 0, 0, 0, 2, 5, 4, 3, 4, 4, 3, 0, 3},
        {0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 2, 5, 4, 4, 0, 3, 4, 4, 8},
        {2, 2, 2, 2, 2, 1, 5, 3, 3, 0, 4, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0},
    };

    void Start()
    {
        if (!shouldGenerateLevel)
            return;

        // Clear the manual level layout
        ClearLevel();

        // Generate the top left corner - to be used as a base for the rest of the level
        GenerateBase();

        // Mirror the base horizontally (children of this object will be used as the base)
        MirrorBaseHorizontally();

        // Mirror the top two segments vertically (the segments are the children of this object)
        MirrorBaseVertically();
    }

    Transform Spawn(GameObject prefab, Vector2 pos)
    {
        Transform obj = Instantiate(prefab).transform;
        obj.SetParent(transform);
        obj.localPosition = pos; // Set local position, because this object defines the top left corner
        return obj;
    }

    void SpawnAllFromMap(int[,] levelMap)
    {
        for (int y = 0; y < levelMap.GetLength(0); y++)
            for (int x = 0; x < levelMap.GetLength(1); x++)
                if (prefabs[levelMap[y, x]])
                    Spawn(prefabs[levelMap[y, x]], new(x, -y));
    }

    void ClearLevel()
    {
        for (int i = 0; i < transform.childCount; ++i)
            Destroy(transform.GetChild(i).gameObject);
    }

    void GenerateBase()
    {
        SpawnAllFromMap(levelMap);
    }

    void MirrorBaseHorizontally()
    {

    }

    void MirrorBaseVertically()
    {

    }
}
