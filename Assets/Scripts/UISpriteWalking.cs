using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteWalking : MonoBehaviour
{
    public Image image;
    public Sprite[] upFrames;
    public Sprite[] rightFrames;
    public Sprite[] downFrames;
    public Sprite[] leftFrames;
    public float frameDelay = 0.1f; // Default frame delay (100ms) from Aseprite

    IEnumerator Start()
    {
        while (true)
        {
            for (int i = 0 ;; ++i, i %= upFrames.Length)
            {
                image.sprite = upFrames[i];
                yield return new WaitForSeconds(frameDelay);
            }
        }
    }

    void Update()
    {
        // Vector2 lerp;

        // if (time < 5)
        // {
        //     lerp = Vector2.LerpUnclamped(startPos, rightPos, time / 5);
        //     animator.SetInteger("Direction", 0);
        // }

        // else if (time < 5 + 4)
        // {
        //     lerp = Vector2.LerpUnclamped(rightPos, bottomPos, (time - 5) / 4);
        //     animator.SetInteger("Direction", 1);
        // }

        // else if (time < 5 + 4 + 5)
        // {
        //     lerp = Vector2.LerpUnclamped(bottomPos, leftPos, (time - 5 - 4) / 5);
        //     animator.SetInteger("Direction", 2);
        // }

        // else
        // {
        //     lerp = Vector2.LerpUnclamped(leftPos, startPos, (time - 5 - 4 - 5) / 4);
        //     animator.SetInteger("Direction", 3);
        // }

        // // Walk sound (also scales with speed variable)
        // if ((int) time != lastSoundPlayed)
        // {
        //     audioSource.Play();
        //     lastSoundPlayed = (int) time;
        // }

        // transform.position = lerp;
        // time += speed * Time.deltaTime;
        // time %= 5 + 4 + 5 + 4;
    }
}
