using TDC.Prompts;
using TMPro;
using UnityEngine;

namespace TDC.UI.HUD.Prompts
{
    public class PromptWidget : MonoBehaviour
    {
        public PromptData CurrentPrompt;
        public TextMeshProUGUI ContentText;

        public void Open(PromptData data)
        {
            CurrentPrompt = data;
            ContentText.text = data.Content;
        }
    }
}