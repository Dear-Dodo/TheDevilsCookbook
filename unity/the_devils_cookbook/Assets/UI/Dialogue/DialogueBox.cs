using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDC.UI.Dialogue
{
    public class DialogueBox : MonoBehaviour, IPointerClickHandler
    {
        public Image CharacterPortrait;
        public TextMeshProUGUI CharacterName;
        private Transform _CharacterNameContainer;
        public TextMeshProUGUI DialogueText;

        public event Action ContinuePressed;

        private void Awake()
        {
            _CharacterNameContainer = CharacterName.transform.parent;
        }

        public void SetPortrait(Sprite sprite)
        {
            if (sprite == null)
            {
                CharacterPortrait.gameObject.SetActive(false);
                return;
            }
            CharacterPortrait.gameObject.SetActive(true);
            CharacterPortrait.sprite = sprite;
        }

        public void SetName(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName))
            {
                _CharacterNameContainer.gameObject.SetActive(false);
                return;
            }
            _CharacterNameContainer.gameObject.SetActive(true);
            CharacterName.text = speakerName;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            ContinuePressed?.Invoke();
        }
    }
}
