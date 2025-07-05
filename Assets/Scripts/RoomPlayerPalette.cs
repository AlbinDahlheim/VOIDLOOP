using UnityEngine;

public class RoomPlayerPalette : MonoBehaviour
{
    public Color outlineColor;

    void Start()
    {
        if (FindAnyObjectByType<PlayerBehavior>() != null)
        {
            FindAnyObjectByType<PlayerBehavior>().PaletteSwap(outlineColor);
        }
    }
}
