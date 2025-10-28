using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cherry : MonoBehaviour
{
    public Vector3 middleOfMap = new();
    public float speed = 5f;

    IEnumerator Start()
    {
        // Choose start and end positions
        int width = LevelGenerator.me.realMapWidth;
        int height = LevelGenerator.me.realMapHeight;

        int x, y;

        // Choose side of the map
        switch (Random.Range(0, 4))
        {
            // Left
            case 0:
                y = Random.Range(0, height - 1);
                transform.position = LevelGenerator.me.MapToWorldPos(0, y);
                break;

            // Right
            case 1:
                y = Random.Range(0, height - 1);
                transform.position = LevelGenerator.me.MapToWorldPos(width, y);
                break;

            // Top
            case 2:
                x = Random.Range(0, width - 1);
                transform.position = LevelGenerator.me.MapToWorldPos(x, 0);
                break;

            // Bottom
            case 3:
                x = Random.Range(0, width - 1);
                transform.position = LevelGenerator.me.MapToWorldPos(x, height);
                break;
        }

        Vector3 direction = (middleOfMap - transform.position).normalized;

        // Move cherry at a fixed speed until it is out of bounds
        while (InBounds())
        {
            transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        CherryController.me.DestroyCherry();
    }

    bool InBounds()
    {
        // Convert x and y to map coords
        (int x, int y) = LevelGenerator.me.WorldToMapPos(transform.position);
        int w = LevelGenerator.me.realMapWidth;
        int h = LevelGenerator.me.realMapHeight;

        // Includes a one unit buffer around the map
        return x >= -1 && y >= -1 && x <= w + 1 && y <= h + 1;
    }
}
