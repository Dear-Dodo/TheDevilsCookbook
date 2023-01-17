using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDC.UI.Generic
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TooltipProxyBase : MonoBehaviour
    {
        public CanvasGroup Group { get; private set; }

        protected virtual void Awake()
        {
            this.Validate();
            Group = GetComponent<CanvasGroup>();
        }

        protected void OnEnable()
        {
            UpdatePosition();
        }

        protected void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            transform.position = mousePos;
        }
    }
}
