using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public float speed = 5f;

    enum InputType
    {
        None, W, A, S, D,
    }

    InputType lastInput = InputType.None;
    InputType currentInput = InputType.None;

    // Currently lerping between two cells?
    bool isLerping = false;

    void Update()
    {
        LevelGenerator.me.GetCell(transform.position);

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

        // Store last input
        if (playerInput != InputType.None)
            lastInput = playerInput;

        // Debug.Log(playerInput + ", " + lastInput + ", " + currentInput);

        // Use last input if valid
        if (!isLerping)
        {
            if (CanMove(lastInput))
            {
                currentInput = lastInput;
                StartCoroutine(LerpToCell(transform.position + GetDirVector(currentInput)));
            }

            else if (CanMove(currentInput))
                StartCoroutine(LerpToCell(transform.position + GetDirVector(currentInput)));
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

    IEnumerator LerpToCell(Vector2 endPos)
    {
        isLerping = true;
        Vector3 startPos = transform.position;
        float duration = 1 / speed; // Speed is cells per second

        for (float s = Time.time; Time.time - s < duration;)
        {
            float t = (Time.time - s) / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPos;
        isLerping = false;
    }
}
