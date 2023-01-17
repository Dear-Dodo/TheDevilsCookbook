using System.IO;
using TDC.Core.Type;
using TDC.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public class Readme : MonoBehaviour, IApplicationStartHandler
    {
        [SerializeField, SerializedValueRequired]
        private TextMeshProUGUI _ReadmeText;

        [SerializeField, SerializedValueRequired]
        private Image _CloseButtonContainer;

        [SerializeField, SerializedValueRequired]
        private Button _CloseButton;

        [SerializeField] private float _TweenDuration = 0.5f;

        public void StartTween()
        {
            // Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
            // foreach (Graphic graphic in graphics)
            // {
            //     float targetAlpha = graphic.color.a;
            //     graphic.color *= new Color(1, 1, 1, 0);
            //     graphic.DOFade(targetAlpha, _TweenDuration);
            // }
        }

        public void ApplicationStart()
        {
            if (_ReadmeText != null)
            {
                this.Validate();
                string readmeText = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "README"));
                _ReadmeText.text = TMPTextParser.Parse(readmeText, new TMPTextParser.ParserSettings());
            }
            _CloseButton.onClick.AddListener(() => gameObject.SetActive(false));
            // _ReadmeText.GetComponent<TMPHyperlinkHandler>().Initialise();
        }
    }
}