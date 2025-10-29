using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    // Singleton
    public static PacStudentController me;

    // References
    public Animator animator;
    public AudioSource audioSource;
    public ParticleSystem dustParticleSystem;
    public ParticleSystem wallImpactParticleSystem;
    public ParticleSystem deathParticleSystem;
    public AudioClip footstepClip;
    public AudioClip rockEatClip;
    public AudioClip diamondCollectClip;
    public AudioClip wallImpactClip;
    public AudioClip deathClip;
    public GameObject flashlightMask; // Level 2 feature

    public float speed = 5f;

    public enum InputType // public because it's used in level 2
    {
        None, W, A, S, D,
    }

    InputType lastInput = InputType.None;
    public InputType currentInput = InputType.None; // public because it's used in level 2

    // Currently lerping between two cells?
    bool isLerping = false;

    // Just collided with a wall and stopped?
    bool justCollided = false;

    bool isDead = false;

    void Awake()
    {
        me = this;

        // Pause animator to begin with
        animator.speed = 0;
    }

    void Update()
    {
        if (isDead || HUD.me.gameTime <= 0 || HUD.me.gameOverPanel.activeSelf)
            return;

        // Get current input
        InputType playerInput = InputType.None;

        if (Input.GetKey("w"))
            playerInput = InputType.W;
        else if (Input.GetKey("a"))
            playerInput = InputType.A;
        else if (Input.GetKey("s"))
            playerInput = InputType.S;
        else if (Input.GetKey("d"))
            playerInput = InputType.D;

        // Store last input and reset collision
        if (playerInput != InputType.None && playerInput != lastInput)
        {
            lastInput = playerInput;
            justCollided = false;
        }

        // Debug.Log(playerInput + ", " + lastInput + ", " + currentInput);

        if (!isLerping)
        {
            // Use last input if valid
            if (CanMove(lastInput) && lastInput != InputType.None)
            {
                currentInput = lastInput;
                StartCoroutine(LerpToCell(transform.position + GetDirVector(currentInput)));
                PlayEffects(currentInput);
            }

            // Otherwise use current input if valid
            else if (CanMove(currentInput) && currentInput != InputType.None)
            {
                StartCoroutine(LerpToCell(transform.position + GetDirVector(currentInput)));
                PlayEffects(currentInput);
            }

            // Travelling somewhere but path is obstructed
            else if (currentInput != InputType.None && !justCollided)
            {
                PlaySound(wallImpactClip);
                justCollided = true;

                // Play wall impact particles
                wallImpactParticleSystem.Play();
            }

            // Update animator when changing direction
            int animDir = currentInput switch
            {
                InputType.D => 0,
                InputType.S => 1,
                InputType.A => 2,
                InputType.W => 3,
                _ => -1,
            };

            if (animDir != -1)
                animator.SetInteger("Direction", animDir);

            // Pause animator if not moving
            animator.speed = 0;
        }

        else
        {
            // Reset animator speed when lerping
            animator.speed = 1;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Diamond"))
        {
            HUD.me.score += 100;
            Destroy(other.gameObject);
        }

        else if (other.CompareTag("Rock"))
        {
            HUD.me.score += 10;
            Destroy(other.gameObject);

            // Reduce pellet count and check for win
            if (--LevelGenerator.me.pelletCount <= 0)
                StartCoroutine(HUD.me.GameOver());
        }

        else if (other.CompareTag("PowerPellet"))
        {
            HUD.me.score += 50;
            Destroy(other.gameObject);

            StartCoroutine(HUD.me.ScareGhosts());

            // Level 2 feature
            if (HUD.me.level == 2)
                StartCoroutine(EnableLight());
        }

        else if (other.CompareTag("Ghost"))
        {
            GhostController ghost = other.GetComponent<GhostController>();

            // Ghosts can not be eaten in level 2
            if (ghost.state == GhostController.GhostState.Scared && HUD.me.level == 1)
            {
                HUD.me.score += 300;
                AudioPlayer.me.PlayGhostDeadMusic();

                StartCoroutine(ghost.Die());
            }

            else if (ghost.state == GhostController.GhostState.Normal)
            {
                // Play death sound and animation before respawning
                StartCoroutine(Die());
            }
        }
    }

    IEnumerator Die()
    {
        isDead = true;

        // Lose a life
        --HUD.me.lives;

        // Play death sound, animation and particle effect
        PlaySound(deathClip);
        deathParticleSystem.Play();
        animator.speed = 1;
        animator.SetBool("Dead", true);

        yield return new WaitForSeconds(3f);

        animator.SetBool("Dead", false);
        animator.SetInteger("Direction", 0); // Facing right (after respawn)

        // Respawn (if game isn't fully over)
        if (HUD.me.lives <= 0)
            yield break;

        lastInput = InputType.None;
        currentInput = InputType.None;
        isLerping = false;
        StopAllCoroutines();
        transform.position = LevelGenerator.me.MapToWorldPos(1, 1);

        isDead = false;
    }

    Vector3 GetDirVector(InputType direction)
    {
        return direction switch
        {
            InputType.W => new(0, 1),
            InputType.A => new(-1, 0),
            InputType.S => new(0, -1),
            InputType.D => new(1, 0),
            _ => new(),
        };
    }

    bool CanMove(InputType direction)
    {
        return LevelGenerator.me.IsEmpty(transform.position + GetDirVector(direction));
    }

    // int GetCellType(InputType direction)
    // {
    //     return LevelGenerator.me.GetCell(transform.position + GetDirVector(direction));
    // }

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

    void PlayEffects(InputType direction)
    {
        // Get object at next position
        GameObject obj = Util.GetObjAtPos(transform.position + GetDirVector(direction));
        string tag = obj == null ? "" : obj.tag;

        AudioClip clip = tag switch
        {
            "Rock" => rockEatClip,
            "PowerPellet" => rockEatClip,
            _ => footstepClip,
        };

        PlaySound(clip);

        // Play dust particles
        dustParticleSystem.Play();
    }

    void PlaySound(AudioClip clip, float volume = 0.5f)
    {
        audioSource.PlayOneShot(clip, volume);
    }

    // Level 2 feature
    IEnumerator EnableLight()
    {
        Vector3 originalScale = flashlightMask.transform.localScale;
        flashlightMask.transform.localScale = new(100, 100);

        while (HUD.me.scaredTime > 0)
            yield return null;

        flashlightMask.transform.localScale = originalScale;
    }
}
