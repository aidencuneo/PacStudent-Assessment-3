using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public TMP_Text scoreText;
    public Image[] lifeImages;

    // Property values
    float scoreValue = 0;
    int livesValue = 3;

    // Properties
    public float score
    {
        get => scoreValue;

        set
        {
            scoreValue = value;
            scoreText.text = $"Score: <color=#ffdf12>{scoreValue:000000}</color>";
        }
    }

    public int lives
    {
        get => livesValue;

        set
        {
            livesValue = value;

            for (int i = 0; i < lifeImages.Length; ++i)
                lifeImages[i].enabled = i < livesValue;

            if (livesValue <= 0)
            {
                // Die?
            }
        }
    }

    IEnumerator Start()
    {
        score = 639;

        lives = 3;
        yield return new WaitForSeconds(2);
        lives = 2;
        yield return new WaitForSeconds(2);
        lives = 1;
        yield return new WaitForSeconds(2);
        lives = 0;
        yield return new WaitForSeconds(2);

        while (true)
        {
            score = Random.Range(0, 100000);
            yield return new WaitForSeconds(1);
        }
    }

    public void ExitLevel()
    {
        SceneManager.LoadScene("StartScene");
    }
}
