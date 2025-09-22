using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public bool shouldGenerateLevel = true;
    public GameObject[] prefabs;

    // Tile list
    // 0 => empty
    // 1 => outside corner
    // 2 => outside wall
    // 3 => inside corner
    // 4 => inside wall
    // 5 => pellet
    // 6 => power pellet
    // 7 => t junction
    // 8 => ghost exit wall

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

    int MergeSimilar(int type)
    {
        return type switch
        {
            3 => 1,
            4 => 2,
            8 => 2,
            _ => type,
        };
    }

    bool IsConnectible(int type) =>
        type == 1 || type == 2 || type == 7;

    float FindRotation(int x, int y)
    {
        // The type of this object
        int type = levelMap[y, x];

        // The types of each von neumann neighbour (coords go in reverse if they exceed the map, to replicate mirroring)
        int left = x - 1 >= 0 ? levelMap[y, x - 1] : 0;
        int right = x + 1 < levelMap.GetLength(1) ? levelMap[y, x + 1] : levelMap[y, x - 1];
        int bottom = y + 1 < levelMap.GetLength(0) ? levelMap[y + 1, x] : levelMap[y - 1, x];
        int top = y - 1 >= 0 ? levelMap[y - 1, x] : 0;

        // All corners and walls
        if (type >= 1 && type <= 4 || type == 8)
        {
            // Merge together functionally similar pieces
            // (inside and outside, ghost exit walls and normal walls)
            left = MergeSimilar(left);
            right = MergeSimilar(right);
            bottom = MergeSimilar(bottom);
            top = MergeSimilar(top);

            // Remaining types: [0, 1, 2, 5, 6, 7]
            // [5, 6, 7] are not significant here, so we are left with [0, 1, 2]
            // 3 ** 4 = 81 possibilities for each

            // Corners
            if (type == 1 || type == 3)
            {
                if (IsConnectible(left) && IsConnectible(top))
                    return 180;
                else if (IsConnectible(left) && IsConnectible(bottom))
                    return 270;
                else if (IsConnectible(right) && IsConnectible(top))
                    return 90;
            }

            // Walls
            else if (type == 2 || type == 4 || type == 8)
            {
                if (IsConnectible(top) && IsConnectible(bottom))
                    return 90;
            }
        }

        return 0;
    }

    void ClearLevel()
    {
        for (int i = 0; i < transform.childCount; ++i)
            Destroy(transform.GetChild(i).gameObject);
    }

    void GenerateBase()
    {
        for (int y = 0; y < levelMap.GetLength(0); y++)
            for (int x = 0; x < levelMap.GetLength(1); x++)
            {
                if (!prefabs[levelMap[y, x]])
                    continue;

                // Spawn the object
                Transform obj = Spawn(prefabs[levelMap[y, x]], new(x, -y));

                // Determine its rotation
                obj.rotation = Quaternion.Euler(0, 0, FindRotation(x, y));
            }
    }

    void MirrorBaseHorizontally()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            Transform newChild = Spawn(
                child.gameObject,
                new(child.position.x + levelMap.GetLength(1), child.position.y));

            float newAngle = child.rotation.eulerAngles.z switch
            {
                90 => 180,
                180 => 90,
                270 => 0,
                0 => 270,
                _ => child.rotation.eulerAngles.z,
            };

            if (child.name.Contains("Corner"))
                newChild.rotation = Quaternion.Euler(0, 0, newAngle);
            else if (child.name.Contains("Junction"))
                newChild.localScale = new(-1, 1, 1);
        }
    }

    void MirrorBaseVertically()
    {

    }
}
