using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TDC
{
    public class WindowGrabRegion : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform WindowTransform;
        private bool _MouseDown;
        private Vector2 _Offset;

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 MousePos = Mouse.current.position.ReadValue();
            Vector2 WindowPos = WindowTransform.position;
            _Offset = new Vector2(WindowPos.x - MousePos.x, WindowPos.y - MousePos.y);
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
                WindowTransform.position = Mouse.current.position.ReadValue() + _Offset;
            }
        }
    }
}
