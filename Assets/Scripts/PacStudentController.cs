using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public ParticleSystem dustParticleSystem;
    public ParticleSystem wallImpactParticleSystem;
    public AudioClip footstepClip;
    public AudioClip rockEatClip;
    public AudioClip diamondCollectClip;
    public AudioClip wallImpactClip;
    public AudioClip deathClip;

    public float speed = 5f;

    enum InputType
    {
        None, W, A, S, D,
    }

    InputType lastInput = InputType.None;
    InputType currentInput = InputType.None;

    // Currently lerping between two cells?
    bool isLerping = false;

    // Just collided with a wall and stopped?
    bool justCollided = false;

    void Update()
    {
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
        if (playerInput != InputType.None)
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

    int GetCellType(InputType direction)
    {
        return LevelGenerator.me.GetCell(transform.position + GetDirVector(direction));
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

    void PlayEffects(InputType direction)
    {
        // Get next cell
        int type = GetCellType(direction);

        AudioClip clip = type switch
        {
            5 => rockEatClip,
            6 => rockEatClip,
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
}
