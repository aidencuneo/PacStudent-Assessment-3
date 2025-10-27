using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text gameTimeText;
    public Image[] lifeImages;

    float startTime = 0;

    // Property values
    float scoreValue = 0;
    float gameTimeValue = 0;
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

    public float gameTime
    {
        get => gameTimeValue;

        set
        {
            gameTimeValue = value;

            string displayedTime = Util.DisplayTime(gameTimeValue);
            gameTimeText.text = $"Time: <color=#ffdf12>{displayedTime}</color>";
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
        startTime = Time.unscaledTime;

        // Test different values on screen
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

    void Update()
    {
        gameTime = Time.unscaledTime - startTime;
        Debug.Log(Util.DisplayTime(Time.unscaledTime));
    }

    public void ExitLevel()
    {
        SceneManager.LoadScene("StartScene");
    }
}
