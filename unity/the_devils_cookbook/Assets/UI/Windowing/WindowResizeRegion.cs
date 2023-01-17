using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TDC
{
    public class WindowResizeRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform WindowTransform;
        public bool Horizontal;
        public bool Vertical;
        public Texture2D CursorTexture;
        public Vector2 Hotspot;
        public float MinSizeX;
        public float MinSizeY;

        public Action OnResize;

        private bool _MouseDown;
        private Vector3[] _Corners = new Vector3[4];
        public void OnPointerEnter(PointerEventData eventData)
        {
            //Cursor.SetCursor(CursorTexture, Hotspot, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Cursor.SetCursor(PlayerSettings.defaultCursor, PlayerSettings.cursorHotspot, CursorMode.Auto);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            
            WindowTransform.GetWorldCorners(_Corners);
            _MouseDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _MouseDown = false;
        }

        void Update()
        {
            if (_MouseDown)
            {
                if (Horizontal)
                {
                    WindowTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(Mouse.current.position.ReadValue().x - _Corners[1].x, MinSizeX));
                }
                if (Vertical)
                {
                    WindowTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(_Corners[1].y - Mouse.current.position.ReadValue().y, MinSizeY));
                }
                if (Horizontal || Vertical)
                {
                    OnResize?.Invoke();
                }
            }
        }

        
    }
}
