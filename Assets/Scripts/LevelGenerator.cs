using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    (int, int, int, int) GetVonNeumannNeighbours(int x, int y)
    {
        // Coords go in reverse if they exceed the map, to replicate mirroring
        int top = y - 1 >= 0 ? levelMap[y - 1, x] : 0;
        int right = x + 1 < levelMap.GetLength(1) ? levelMap[y, x + 1] : levelMap[y, x]; // Note [y, x] for horizontal
        int bottom = y + 1 < levelMap.GetLength(0) ? levelMap[y + 1, x] : levelMap[y - 1, x]; // Note [y - 1, x] for vertical
        int left = x - 1 >= 0 ? levelMap[y, x - 1] : 0;

        return (top, right, bottom, left);
    }

    float FindRotation(int x, int y)
    {
        // Out of bounds
        if (x < 0 || y < 0 || x >= levelMap.GetLength(1) || y >= levelMap.GetLength(0))
            return 0;

        // The type of this object
        int type = levelMap[y, x];

        // The types of each von neumann neighbour
        var (top, right, bottom, left) = GetVonNeumannNeighbours(x, y);

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

            // More preferable conditions are higher in if chains

            // Corners
            if (type == 1 || type == 3)
            {
                // Walls only count if they are facing the correct direction
                if (left == 2 && FindRotation(x - 1, y) != 0)
                    left = 0;
                if (right == 2 && FindRotation(x + 1, y) != 0)
                    right = 0;
                if (top == 2 && FindRotation(x, y - 1) != 90)
                    top = 0;
                if (bottom == 2 && FindRotation(x, y + 1) != 90)
                    bottom = 0;

                // Wall connections
                if (left == 2 && top == 2)
                    return 180;
                else if (left == 2 && bottom == 2)
                    return 270;
                else if (right == 2 && top == 2)
                    return 90;
                else if (right == 2 && bottom == 2)
                    return 0;

                // Wall and corner connections
                else if ((left == 1 && top == 2) || (left == 2 && top == 1))
                    return 180;
                else if ((left == 1 && bottom == 2) || (left == 2 && bottom == 1))
                    return 270;
                else if ((right == 1 && top == 2) || (right == 2 && top == 1))
                    return 90;
                else if ((right == 1 && bottom == 2) || (right == 2 && bottom == 1))
                    return 0;

                // Corner connections
                else if (left == 1 && top == 1)
                    return 180;
                else if (left == 1 && bottom == 1)
                    return 270;
                else if (right == 1 && top == 1)
                    return 90;
                else if (right == 1 && bottom == 1)
                    return 0;
            }

            // Walls
            else if (type == 2 || type == 4 || type == 8)
            {
                // Treat T junctions as a corner
                top = top == 7 ? 1 : top;
                bottom = bottom == 7 ? 1 : bottom;
                left = left == 7 ? 1 : left;
                right = right == 7 ? 1 : right;

                // Wall connections
                if (top == 2 && bottom == 2)
                    return 90;
                else if (left == 2 && right == 2)
                    return 0;

                // Wall and corner connections
                else if ((top == 1 && bottom == 2) || (top == 2 && bottom == 1))
                    return 90;
                else if ((left == 1 && right == 2) || (left == 2 && right == 1))
                    return 0;

                // Corner connections
                else if (top == 1 && bottom == 1)
                    return 90;
                else if (left == 1 && right == 1)
                    return 0;
            }
        }

        // T junctions
        else if (type == 7)
        {
            // Treat other T junctions as single walls, since that's how they join visually
            top = top == 7 ? 1 : top;
            bottom = bottom == 7 ? 1 : bottom;
            left = left == 7 ? 1 : left;
            right = right == 7 ? 1 : right;

            // It is assumed that there are three connecting walls: one double and two single

            // Find connected double wall (index refers to number of clockwise turns from top)
            int doubleIndex = -1;

            if (top == 2)
                doubleIndex = 0;
            else if (right == 2)
                doubleIndex = 1;
            else if (bottom == 2)
                doubleIndex = 2;
            else if (left == 2)
                doubleIndex = 3;

            // Find empty space (specifically, the one that isn't a wall)
            int spaceIndex = -1;

            if (top != 2 && top != 4)
                spaceIndex = 0;
            else if (right != 2 && right != 4)
                spaceIndex = 1;
            else if (bottom != 2 && bottom != 4)
                spaceIndex = 2;
            else if (left != 2 && left != 4)
                spaceIndex = 3;

            // If space is one index above double wall (one clockwise turn)
            if (spaceIndex == (doubleIndex + 1) % 4)
            {
                return 90 - (spaceIndex * 90);
            }

            // If space is one index below double wall (one anticlockwise turn)
            else if (doubleIndex == (spaceIndex + 1) % 4)
            {
                return 90 - (doubleIndex * 90);
            }

            // Otherwise, the default rotation will be 90
            return 90;
        }

        // Default rotation for this piece
        return 0;
    }

    void ClearLevel()
    {
        for (; transform.childCount > 0;)
        {
            Transform t = transform.GetChild(0);

            // Unparenting because destruction won't occur until the end of the frame
            t.SetParent(null);

            Destroy(t.gameObject);
        }
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
        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            // Skip this transform
            if (child == transform)
                continue;

            Transform newChild = Spawn(
                child.gameObject,
                new(2 * levelMap.GetLength(1) - child.localPosition.x - 1, child.localPosition.y));

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
            {
                newChild.rotation = Quaternion.Euler(0, 0, -child.rotation.z);
                newChild.localScale = new(-newChild.localScale.x, newChild.localScale.y, 1);
            }
        }
    }

    void MirrorBaseVertically()
    {
        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            // Skip this transform
            if (child == transform)
                continue;

            Vector3 newPos = new(child.localPosition.x, -2 * levelMap.GetLength(0) - child.localPosition.y + 2);

            // Skip overlapping objects
            if (newPos == child.localPosition)
                continue;

            Transform newChild = Spawn(child.gameObject, newPos);

            float newAngle = child.rotation.eulerAngles.z switch
            {
                0 => 90,
                90 => 0,
                180 => 270,
                270 => 180,
                _ => child.rotation.eulerAngles.z,
            };

            if (child.name.Contains("Corner"))
                newChild.rotation = Quaternion.Euler(0, 0, newAngle);

            else if (child.name.Contains("Junction"))
            {
                newChild.rotation = Quaternion.Euler(0, 0, child.rotation.z);
                newChild.localScale = new(newChild.localScale.x, -newChild.localScale.y, 1);
            }
        }
    }
}
