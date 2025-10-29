using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public Animator animator;
    public int ghostID = 1; // [1, 2, 3, 4]

    public enum GhostState
    {
        Normal,
        Scared,
        Dead,
    }

    enum Direction
    {
        None, Up, Down, Left, Right,
    }

    public GhostState state = GhostState.Normal;

    // Properties
    float speed => HUD.me.level == 1
        ? (state == GhostState.Normal ? 0.9f : 0.5f) * PacStudentController.me.speed // Level 1
        : 1.25f * PacStudentController.me.speed; // Level 2

    // Currently lerping between two cells?
    bool isLerping = false;

    Direction curDir = Direction.None;
    bool inSpawn = true;
    float lastTeleportAttempt = 0; // Level 2 feature

    // Properties
    bool playerInRange => !AwayFromPlayer(transform.position, 8);

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

        // Level 2 feature
        if (!playerInRange)
        {
            // Pause animator and don't move if the player is too far away
            animator.speed = 0;

            // Try teleporting (25% chance)
            if (Time.time - lastTeleportAttempt > 3)
            {
                if (Random.value < 0.25f)
                    TeleportRandom();

                lastTeleportAttempt = Time.time;
            }

            return;
        }

        // Animator runs while the game is on
        animator.speed = 1;

        // Change ghost state if needed
        if (HUD.me.scaredTime > 0 && state == GhostState.Normal)
        {
            animator.SetBool("Scared", true);
            state = GhostState.Scared;
        }

        else if (HUD.me.scaredTime <= 0 && state == GhostState.Scared)
        {
            animator.SetBool("Scared", false);
            state = GhostState.Normal;
        }

        // Recovery state
        if (HUD.me.scaredTime <= 3 && state == GhostState.Scared)
            animator.SetBool("Scared", HUD.me.scaredTime % 1 < 0.5f);

        if (isLerping || state == GhostState.Dead)
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
        if (ghostID == 1 || HUD.me.scaredTime > 0) // Ghost ID 1 OR scared timer on
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

            // Move in a circular motion depending on which edge we're in
            float absX = Mathf.Abs(transform.position.x);
            float absY = Mathf.Abs(transform.position.y);

            // Top edge
            if (absY >= absX && transform.position.y > 0)
            {
                if (possibleDirs.Contains(Direction.Up))
                    return Direction.Up;
                else if (possibleDirs.Contains(Direction.Right))
                    return Direction.Right;
                else if (possibleDirs.Contains(Direction.Down))
                    return Direction.Down;
                else
                    return Direction.Left;
            }

            // Right edge
            else if (absX >= absY && transform.position.x > 0)
            {
                if (possibleDirs.Contains(Direction.Right))
                    return Direction.Right;
                else if (possibleDirs.Contains(Direction.Down))
                    return Direction.Down;
                else if (possibleDirs.Contains(Direction.Left))
                    return Direction.Left;
                else
                    return Direction.Up;
            }

            // Bottom edge
            else if (absY >= absX && transform.position.y < 0)
            {
                if (possibleDirs.Contains(Direction.Down))
                    return Direction.Down;
                else if (possibleDirs.Contains(Direction.Left))
                    return Direction.Left;
                else if (possibleDirs.Contains(Direction.Up))
                    return Direction.Up;
                else
                    return Direction.Right;
            }

            // Left edge
            else // if (absX >= absY && transform.position.x < 0)
            {
                if (possibleDirs.Contains(Direction.Left))
                    return Direction.Left;
                else if (possibleDirs.Contains(Direction.Up))
                    return Direction.Up;
                else if (possibleDirs.Contains(Direction.Right))
                    return Direction.Right;
                else
                    return Direction.Down;
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

    public IEnumerator Die()
    {
        // Stop lerping
        StopAllCoroutines();
        isLerping = false;

        // Set state to dead
        state = GhostState.Dead;
        animator.SetBool("Dead", true);

        // Move towards spawn area
        Vector3 dir = -transform.position.normalized;
        float respawnSpeed = 5;

        while (transform.position.sqrMagnitude > 0.5f)
        {
            transform.position += dir * respawnSpeed * Time.deltaTime;
            yield return null;
        }

        transform.position = new();
        inSpawn = true;

        // Set state back to normal
        state = GhostState.Normal;
        animator.SetBool("Dead", false);
    }

    // Level 2 feature
    bool AwayFromPlayer(Vector3 pos, float distance)
    {
        return (pos - PacStudentController.me.transform.position).sqrMagnitude > distance * distance;
    }

    // Level 2 feature
    void TeleportRandom(int attempts = 10)
    {
        // Find a stretch of 5 blocks in any direction from the player, starting
        // behind, followed by the sides, then the front

        Vector2 playerPos = PacStudentController.me.transform.position;
        (int playerX, int playerY) = LevelGenerator.me.WorldToMapPos(playerPos);
        PacStudentController.InputType playerDir = PacStudentController.me.currentInput;

        List<Direction> possibleDirs = new();
        int i;

        // Left
        for (i = 1; i < 5; ++i)
            if (!LevelGenerator.me.IsEmpty(playerX - i, playerY))
                break;
        
        if (i == 5)
            possibleDirs.Add(Direction.Left);

        // Right
        for (i = 1; i < 5; ++i)
            if (!LevelGenerator.me.IsEmpty(playerX + i, playerY))
                break;
        
        if (i == 5)
            possibleDirs.Add(Direction.Right);

        // Up
        for (i = 1; i < 5; ++i)
            if (!LevelGenerator.me.IsEmpty(playerX, playerY - i))
                break;
        
        if (i == 5)
            possibleDirs.Add(Direction.Up);

        // Down
        for (i = 1; i < 5; ++i)
            if (!LevelGenerator.me.IsEmpty(playerX, playerY + i))
                break;
        
        if (i == 5)
            possibleDirs.Add(Direction.Down);

        // No valid directions?
        if (possibleDirs.Count == 0)
            return;

        // Choose a random direction
        Direction dir = possibleDirs[Random.Range(0, possibleDirs.Count)];

        if (dir == Direction.Left)
            transform.position = LevelGenerator.me.MapToWorldPos(playerX - 5, playerY);
        else if (dir == Direction.Right)
            transform.position = LevelGenerator.me.MapToWorldPos(playerX + 5, playerY);
        else if (dir == Direction.Up)
            transform.position = LevelGenerator.me.MapToWorldPos(playerX, playerY - 5);
        else if (dir == Direction.Down)
            transform.position = LevelGenerator.me.MapToWorldPos(playerX, playerY + 5);

        // int x, y;

        // for (int i = 0; i < attempts; ++i)
        // {
        //     x = Random.Range(0, LevelGenerator.me.realMapWidth - 1);
        //     y = Random.Range(0, LevelGenerator.me.realMapHeight - 1);
        //     Vector2 pos = LevelGenerator.me.MapToWorldPos(x, y);

        //     // If valid and far enough from the player, teleport here
        //     if (AwayFromPlayer(pos, 8) && LevelGenerator.me.IsEmpty(x, y))
        //     {
        //         transform.position = LevelGenerator.me.MapToWorldPos(x, y);
        //         break;
        //     }
        // }
    }
}
