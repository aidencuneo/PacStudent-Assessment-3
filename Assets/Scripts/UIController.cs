using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TMP_Text level1HighscoreText;
    public TMP_Text level2HighscoreText;
    public TMP_Text level1TimeText;
    public TMP_Text level2TimeText;

    void Start()
    {
        // Load level 1 and 2 highscores and times when I get to that part
        UpdateHighscores();
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("InnovationScene");
    }

    void UpdateHighscores()
    {
        int level1Highscore = PlayerPrefs.GetInt("level1_highscore", 0);
        int level2Highscore = PlayerPrefs.GetInt("level2_highscore", 0);
        float level1Time = PlayerPrefs.GetFloat("level1_time", 0);
        float level2Time = PlayerPrefs.GetFloat("level2_time", 0);

        string level1TimeStr = Util.DisplayTime(level1Time);
        string level2TimeStr = Util.DisplayTime(level2Time);

        level1HighscoreText.text = $"Highscore: <color=#ffdf12>{level1Highscore:D6}</color>";
        level2HighscoreText.text = $"Highscore: <color=#ffdf12>{level2Highscore:D6}</color>";
        level1TimeText.text = $"Time: <color=#ffdf12>{level1TimeStr}</color>";
        level2TimeText.text = $"Time: <color=#ffdf12>{level2TimeStr}</color>";
    }
}
