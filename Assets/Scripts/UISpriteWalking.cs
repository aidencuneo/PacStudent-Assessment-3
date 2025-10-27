using UnityEngine;
using UnityEngine.UI;

public class UISpriteWalking : MonoBehaviour
{
    public RectTransform rect;
    public AnimatedUISprite animator;
    public Sprite[] upFrames;
    public Sprite[] rightFrames;
    public Sprite[] downFrames;
    public Sprite[] leftFrames;
    public float speed = 1;
    public float time = 0;
    public Transform[] corners; // From top left, continuing clockwise

    void Update()
    {
        Vector2 lerp;

        if (time < 11)
        {
            lerp = Vector2.LerpUnclamped(corners[0].position, corners[1].position, time / 11);
            animator.frames = rightFrames;
        }

        else if (time < 11 + 6)
        {
            lerp = Vector2.LerpUnclamped(corners[1].position, corners[2].position, (time - 11) / 6);
            animator.frames = downFrames;
        }

        else if (time < 11 + 6 + 11)
        {
            lerp = Vector2.LerpUnclamped(corners[2].position, corners[3].position, (time - 11 - 6) / 11);
            animator.frames = leftFrames;
        }

        else
        {
            lerp = Vector2.LerpUnclamped(corners[3].position, corners[0].position, (time - 11 - 6 - 11) / 6);
            animator.frames = upFrames;
        }

        transform.position = lerp;
        time += speed * Time.deltaTime;
        time %= 11 + 6 + 11 + 6;
    }
}
