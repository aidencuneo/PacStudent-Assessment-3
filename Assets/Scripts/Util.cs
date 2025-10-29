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

    public static GameObject GetObjAtPos(Vector2 pos)
    {
        // Raycast at pos
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

        if (hit.collider != null)
            return hit.collider.gameObject;

        return null;
    }
}
