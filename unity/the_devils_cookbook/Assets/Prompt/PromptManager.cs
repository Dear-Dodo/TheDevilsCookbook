using TDC.Core.Manager;
using TDC.UI.HUD.Prompts;
using UnityEngine;

namespace TDC.Prompts
{
    public class PromptManager : MonoBehaviour
    {
        public PromptData CurrentPrompt;
        public PromptWidget Widget;
        public PromptChevron Chevron;

        public System.Action<PromptData> OnPromptStart;
        public System.Action<PromptData> OnPromptEnd;

        public GameObject Hud;

        private GameObject _Target;

        public void Open(PromptData data, GameObject target)
        {
            CurrentPrompt = data;
            _Target = target;

            if (Chevron != null)
            {
                Chevron.gameObject.SetActive(false);
            }

            Transform parent = Widget.transform.parent;

            Widget.gameObject.SetActive(true);
            Widget.Open(data);
            OnPromptStart?.Invoke(data);

            Widget.transform.SetAsLastSibling();

            Time.timeScale = 0;
            GameManager.PlayerControls.Disable();
        }

        public void Close()
        {
            if (Widget == null) return;

            if (_Target != null)
                Chevron.Target = _Target;

            if (CurrentPrompt != null)
            {
                Chevron.Offset = CurrentPrompt.ChevronOffset;
                OnPromptEnd?.Invoke(CurrentPrompt);
            }

            Widget.gameObject.SetActive(false);
            Chevron.gameObject.SetActive(true);

            Time.timeScale = 1.0f;
            GameManager.PlayerControls.Enable();
        }
    }
}