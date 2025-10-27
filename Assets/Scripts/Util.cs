using UnityEngine;

public static class Util
{
    public static string DisplayTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        float seconds = time % 60;
        int millis = Mathf.FloorToInt(1000 * time);

        return $"{minutes:2}:{seconds:2}:{millis:2}";
    }
}
