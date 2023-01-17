using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace TDC
{
    public class EyeButton : MonoBehaviour, IPointerDownHandler
    {
        public RectTransform EyeImage;
        public Image EyelidImage;
        public Sprite OpenEye;
        public Sprite ClosedEye;

        public void OnPointerDown(PointerEventData eventData)
        {
            EyelidImage.sprite = ClosedEye;
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 posDelta = Mouse.current.position.ReadValue() - EyeImage.transform.parent.position.xy();
            EyeImage.anchoredPosition = posDelta.normalized * Mathf.Clamp(posDelta.magnitude, 0, 5);
            if (!Mouse.current.leftButton.isPressed)
            {
                EyelidImage.sprite = OpenEye;
            }
        }
    }
}
