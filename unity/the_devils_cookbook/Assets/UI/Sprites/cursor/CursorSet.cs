using UnityEngine;

public class CursorSet : MonoBehaviour
{
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    void Awake()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

}