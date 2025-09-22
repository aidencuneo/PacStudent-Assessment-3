using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PacStudentWalk : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public float speed = 1;

    Vector2 startPos;
    Vector2 rightPos; // Pos after moving right
    Vector2 bottomPos; // Pos after moving down
    Vector2 leftPos; // Pos after moving left
    float time = 0;
    int lastSoundPlayed = -1;

    void Awake()
    {
        startPos = transform.position;
        rightPos = startPos + new Vector2(5, 0);
        bottomPos = rightPos - new Vector2(0, 4);
        leftPos = bottomPos - new Vector2(5, 0);
    }

    void Update()
    {
        Vector2 lerp;

        if (time < 5)
        {
            lerp = Vector2.LerpUnclamped(startPos, rightPos, time / 5);
            animator.SetInteger("Direction", 0);
        }

        else if (time < 5 + 4)
        {
            lerp = Vector2.LerpUnclamped(rightPos, bottomPos, (time - 5) / 4);
            animator.SetInteger("Direction", 1);
        }

        else if (time < 5 + 4 + 5)
        {
            lerp = Vector2.LerpUnclamped(bottomPos, leftPos, (time - 5 - 4) / 5);
            animator.SetInteger("Direction", 2);
        }

        else
        {
            lerp = Vector2.LerpUnclamped(leftPos, startPos, (time - 5 - 4 - 5) / 4);
            animator.SetInteger("Direction", 3);
        }

        // Walk sound (also scales with speed variable)
        if ((int) time != lastSoundPlayed)
        {
            audioSource.Play();
            lastSoundPlayed = (int) time;
        }

        transform.position = lerp;
        time += speed * Time.deltaTime;
        time %= 5 + 4 + 5 + 4;
    }
}
