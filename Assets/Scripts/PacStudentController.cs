using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    enum InputType
    {
        None, W, A, S, D,
    }

    InputType lastInput = InputType.None;
    InputType currentInput = InputType.None;

    void Update()
    {

    }
}
