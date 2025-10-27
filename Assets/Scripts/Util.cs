using UnityEngine;

public static class Util
{
    public static string DisplayTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int millis = Mathf.FloorToInt(time % 1 * 100); // Only showing first two digits

        return $"{minutes:D2}:{seconds:D2}:{millis:D2}";
    }
}
