using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public int ghostID = 1; // [1, 2, 3, 4]

    enum GhostState
    {
        Normal,
        Scared,
        Dead,
    }

    GhostState state = GhostState.Normal;

    // Properties
    float speed => (state == GhostState.Normal ? 0.9f : 0.5f) * PacStudentController.me.speed;

    // Currently lerping between two cells?
    bool isLerping = false;

    void Update()
    {
        
    }

    IEnumerator LerpToCell(Vector2 endPos)
    {
        isLerping = true;
        Vector3 startPos = transform.position;
        float duration = 1 / speed; // Speed is cells per second

        for (float s = Time.time; Time.time - s < duration;)
        {
            float t = (Time.time - s) / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        isLerping = false;

        // Teleporting
        (int x, int y) = LevelGenerator.me.WorldToMapPos(transform.position);

        int w = LevelGenerator.me.realMapWidth;
        int h = LevelGenerator.me.realMapHeight;

        if (x < 0)
            transform.position = LevelGenerator.me.MapToWorldPos(w, y);
        else if (x >= w)
            transform.position = LevelGenerator.me.MapToWorldPos(-1, y);

        if (y < 0)
            transform.position = LevelGenerator.me.MapToWorldPos(x, h);
        else if (y >= h)
            transform.position = LevelGenerator.me.MapToWorldPos(x, -1);
    }
}
