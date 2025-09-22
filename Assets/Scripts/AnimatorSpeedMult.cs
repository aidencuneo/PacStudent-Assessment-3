using UnityEngine;

public class AnimatorSpeedMult : MonoBehaviour
{
    public float multiplier = 1f;

    void Start()
    {
        foreach (Animator animator in GetComponentsInChildren<Animator>())
            animator.speed = 0.75f;
    }
}
