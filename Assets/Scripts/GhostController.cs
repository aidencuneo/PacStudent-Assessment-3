using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public Animator animator;
    public int ghostID = 1; // [1, 2, 3, 4]

    enum GhostState
    {
        Normal,
        Scared,
        Dead,
    }

    enum Direction
    {
        None, Up, Down, Left, Right,
    }

    GhostState state = GhostState.Normal;

    // Properties
    float speed => (state == GhostState.Normal ? 0.9f : 0.5f) * PacStudentController.me.speed;

    // Currently lerping between two cells?
    bool isLerping = false;

    Direction curDir = Direction.None;
    bool inSpawn = true;
    List<Vector2> upperExitWalls = new();
    List<Vector2> lowerExitWalls = new();

    void Start()
    {
        // Locate ghost exit walls
        GameObject[] walls = GameObject.FindGameObjectsWithTag("GhostExitWall");
        Debug.Log(walls.Length);

        foreach (GameObject wall in walls)
        {
            Vector2 pos = wall.transform.position;

            // Is it an upper or lower wall?
            if (pos.y > transform.position.y)
                upperExitWalls.Add(pos);
            else
                lowerExitWalls.Add(pos);
        }
    }

    void Update()
    {
        if (HUD.me.gameTime <= 0)
            return;

        if (isLerping)
            return;

        // Decide direction
        Direction nextDir = ChooseDirection();

        // Move
        StartCoroutine(LerpToCell(transform.position + GetDirVector(nextDir)));

        // Remember current direction
        curDir = nextDir;
    }

    Vector3 GetDirVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new(0, 1),
            Direction.Down => new(0, -1),
            Direction.Left => new(-1, 0),
            Direction.Right => new(1, 0),
            _ => new(),
        };
    }

    bool CanMove(Direction direction)
    {
        return LevelGenerator.me.IsEmpty(transform.position + GetDirVector(direction));
    }

    Direction OppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.None,
        };
    }

    Direction ChooseDirection()
    {
        // Move x first, then y
        if (inSpawn)
        {
            Debug.Log(upperExitWalls.Count + ", " + lowerExitWalls.Count);
        }

        // Get all possibilities
        List<Direction> possibleDirs = new()
        {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right,
        };

        // Remove opposite direction (no backstep)
        possibleDirs.Remove(OppositeDirection(curDir));

        // Remove directions that can't be moved to
        possibleDirs.RemoveAll(dir => !CanMove(dir));

        return Direction.Up;
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
