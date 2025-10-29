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
            List<Vector2> walls;
            
            if (ghostID == 1 || ghostID == 3)
                walls = LevelGenerator.me.ghostExitWalls.FindAll(pos => pos.y > 0);
            else
                walls = LevelGenerator.me.ghostExitWalls.FindAll(pos => pos.y < 0);

            // Get closest wall in the x direction
            float minDist = Mathf.Infinity;
            Vector2 closestWall = new();

            foreach (Vector2 pos in walls)
            {
                float dist = Mathf.Abs(pos.x - transform.position.x);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestWall = pos;
                }
            }

            // Are we out of spawn yet?
            if ((ghostID == 1 || ghostID == 3) && transform.position.y > closestWall.y)
                inSpawn = false;
            else if ((ghostID == 2 || ghostID == 4) && transform.position.y < closestWall.y)
                inSpawn = false;

            // Move towards closest wall
            if (transform.position.x < closestWall.x)
                return Direction.Right;
            else if (transform.position.x > closestWall.x)
                return Direction.Left;

            // Move up or down to leave the spawn area
            else if (ghostID == 1 || ghostID == 3)
                return Direction.Up;
            else if (ghostID == 2 || ghostID == 4)
                return Direction.Down;
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

        // Unique ghost logic

        // Random direction that maximises distance from pacstudent
        if (ghostID == 1)
        {
            
        }

        // Random direction that minimises distance to pacstudent
        else if (ghostID == 2)
        {
            
        }

        // Random direction
        else if (ghostID == 3)
        {
            return possibleDirs[Random.Range(0, possibleDirs.Count)];
        }

        // Move clockwise
        else if (ghostID == 4)
        {
            
        }

        return Direction.Right;
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
