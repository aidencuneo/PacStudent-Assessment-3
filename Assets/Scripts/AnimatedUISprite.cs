using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedUISprite : MonoBehaviour
{
    public Image image;
    public Sprite[] frames;
    public float frameDelay = 0.1f; // Default frame delay (100ms) from Aseprite

    IEnumerator Start()
    {
        for (int i = 0 ;; ++i, i %= frames.Length)
        {
            image.sprite = frames[i];
            yield return new WaitForSeconds(frameDelay);
        }
    }
}
