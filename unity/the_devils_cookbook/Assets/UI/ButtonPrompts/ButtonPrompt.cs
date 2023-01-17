using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace TDC.UI
{
    public class ButtonPrompt : MonoBehaviour, IHideable
    {
        public TextMeshProUGUI Button;
        public Image ButtonBackground;
        public TextMeshProUGUI Prompt;
        public Button ClickButton;

        /// <summary>
        /// Is hidden by normal use.
        /// </summary>
        private bool _IsActive;
        /// <summary>
        /// Is hidden via 'hide' key?
        /// </summary>
        private bool _IsHidden;
        
        public void SetButton(InputAction input)
        {
            Button.text = input.bindings[0].ToDisplayString();
            ButtonBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1.5f + Button.text.Length);
        }
        public void SetPrompt(string prompt) => Prompt.text = prompt;
        
        public void SetActive(bool isActive)
        {
            _IsActive = isActive;
            UpdateActiveState();
        }
        
        private void UpdateActiveState()
        {
            gameObject.SetActive(_IsActive && !_IsHidden);
        }
        
        public void SetHidden(bool isHidden)
        {
            _IsHidden = isHidden;
            UpdateActiveState();
        }
    }
}