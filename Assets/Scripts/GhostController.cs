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

    void Awake()
    {
        // Set initial state for the animator
        animator.speed = 0;
        animator.SetInteger("Direction", 2); // Facing down
    }

    void Update()
    {
        if (HUD.me.gameTime <= 0 || HUD.me.gameOverPanel.activeSelf)
        {
            // Pause animator if the game isn't running
            animator.speed = 0;
            return;
        }

        // Animator runs while the game is on
        animator.speed = 1;

        if (isLerping)
            return;

        // Decide direction
        Direction nextDir = ChooseDirection();

        // Animate
        animator.SetInteger("Direction", nextDir switch
        {
            Direction.Right => 0,
            Direction.Down => 1,
            Direction.Left => 2,
            Direction.Up => 3,
            _ => -1,
        });

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
        (int x, int y) = LevelGenerator.me.WorldToMapPos(transform.position + GetDirVector(direction));
        int w = LevelGenerator.me.realMapWidth;
        int h = LevelGenerator.me.realMapHeight;

        // Is this position inside the map?
        if (x < 0 || x >= w || y < 0 || y >= h)
            return false;

        // Is this position empty?
        return LevelGenerator.me.IsEmpty(x, y);
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
            if ((ghostID == 1 || ghostID == 3) && transform.position.y >= closestWall.y)
                inSpawn = false;
            else if ((ghostID == 2 || ghostID == 4) && transform.position.y <= closestWall.y)
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

        // If directions is now empty, backstep (this must be a dead end)
        if (possibleDirs.Count == 0)
            return OppositeDirection(curDir);

        // Unique ghost logic

        // Random direction that maximises distance from pacstudent
        if (ghostID == 1)
        {
            Vector3 playerPos = PacStudentController.me.transform.position;

            Direction bestDir = possibleDirs[0];
            float maxDist = (transform.position + GetDirVector(bestDir) - playerPos).sqrMagnitude;

            // Find maximum distance
            foreach (Direction dir in possibleDirs)
            {
                float dist = (transform.position + GetDirVector(dir) - playerPos).sqrMagnitude;

                if (dist > maxDist)
                {
                    maxDist = dist;
                    bestDir = dir;
                }
            }

            // Collect all directions with the same distance
            List<Direction> bestDirs = new();

            foreach (Direction dir in possibleDirs)
            {
                float dist = (transform.position + GetDirVector(dir) - playerPos).sqrMagnitude;

                // I'm using epsilon for approximation just in case
                if (Mathf.Abs(maxDist - dist) < Mathf.Epsilon)
                    bestDirs.Add(dir);
            }

            if (bestDirs.Count == 0)
                return bestDir;

            return bestDirs[Random.Range(0, bestDirs.Count)];
        }

        // Random direction that minimises distance to pacstudent
        else if (ghostID == 2)
        {
            Vector3 playerPos = PacStudentController.me.transform.position;

            Direction bestDir = possibleDirs[0];
            float minDist = (transform.position + GetDirVector(bestDir) - playerPos).sqrMagnitude;

            // Find minimum distance
            foreach (Direction dir in possibleDirs)
            {
                float dist = (transform.position + GetDirVector(dir) - playerPos).sqrMagnitude;

                if (dist < minDist)
                {
                    minDist = dist;
                    bestDir = dir;
                }
            }

            // Collect all directions with the same distance
            List<Direction> bestDirs = new();

            foreach (Direction dir in possibleDirs)
            {
                float dist = (transform.position + GetDirVector(dir) - playerPos).sqrMagnitude;

                // I'm using epsilon for approximation just in case
                if (Mathf.Abs(minDist - dist) < Mathf.Epsilon)
                    bestDirs.Add(dir);
            }

            if (bestDirs.Count == 0)
                return bestDir;

            return bestDirs[Random.Range(0, bestDirs.Count)];
        }

        // Random direction
        else if (ghostID == 3)
        {
            return possibleDirs[Random.Range(0, possibleDirs.Count)];
        }

        // Move clockwise around the edges of the map
        else if (ghostID == 4)
        {
            // Prioritise directions in clockwise order
            Direction[] priorityDirs = new Direction[]
            {
                Direction.Right,
                Direction.Down,
                Direction.Left,
                Direction.Up,
            };

            foreach (Direction dir in priorityDirs)
            {
                if (possibleDirs.Contains(dir))
                    return dir;
            }
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
    }
}
