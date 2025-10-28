using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    // Singleton
    public static HUD me;

    // References
    public TMP_Text scoreText;
    public TMP_Text gameTimeText;
    public TMP_Text scaredTimeText;
    public Image[] lifeImages;
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    float startTime = -1;

    // Property values
    float scoreValue = 0;
    float gameTimeValue = 0;
    int scaredTimeValue = 0;
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

    public int scaredTime
    {
        get => scaredTimeValue;

        set
        {
            scaredTimeValue = value;

            string display = scaredTimeValue > 0 ? "" + scaredTimeValue : "";
            scaredTimeText.text = $"Scared Time: <color=#ffdf12>{display}</color>";
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
        me = this;

        for (int i = 3; i >= 0; --i)
        {
            if (i > 0)
                countdownText.text = i.ToString();
            else
                countdownText.text = "GO!";

            yield return new WaitForSeconds(1);
        }

        countdownPanel.SetActive(false);

        // Set this variable when the game actually starts
        startTime = Time.unscaledTime;

        // // Test different values on screen
        // lives = 3;
        // yield return new WaitForSeconds(2);
        // lives = 2;
        // yield return new WaitForSeconds(2);
        // lives = 1;
        // yield return new WaitForSeconds(2);
        // lives = 0;
        // yield return new WaitForSeconds(2);
    }

    void Update()
    {
        if (startTime != -1)
            gameTime = Time.unscaledTime - startTime;
    }

    public void ExitLevel()
    {
        SceneManager.LoadScene("StartScene");
    }

    public IEnumerator ScareGhosts(int duration = 10)
    {
        AudioPlayer.me.PlayScaredMusic();

        for (scaredTime = duration; scaredTime >= 0; --scaredTime)
            yield return new WaitForSeconds(1);

        AudioPlayer.me.PlayRegularMusic();
    }
}
