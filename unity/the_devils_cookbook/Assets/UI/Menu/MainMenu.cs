using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.UI.Windowing;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public class MainMenu : Window
    {
        [SerializeField, SerializedValueRequired] private Button _PlayButton;
        [SerializeField, SerializedValueRequired] private Button[] _ShopButtons;
        [SerializeField, SerializedValueRequired] private Button _SettingsButton;
        [SerializeField, SerializedValueRequired] private Button _CreditsButton;
        //[SerializeField, SerializedValueRequired] private Button _ReadmeButton;
        [SerializeField, SerializedValueRequired] private Button _FeedbackButton;
        [SerializeField, SerializedValueRequired] private Button _QuitButton;

        [SerializeField, SerializedValueRequired]
        private Button _LevelBackButton;

        [SerializeField, SerializedValueRequired]
        private GameObject _MainMenuPanel;

        //[SerializeField, SerializedValueRequired]
        //private Readme _ReadmeCanvas;

        [SerializeField] private GameObject _CreditsCanvas;

        [SerializeField, SerializedValueRequired]
        private GameObject LevelSelectPanel;

        [SerializeField] private string _FeedbackURL = "https://forms.gle/jZbnWsYzENH3pcrC9";

        protected void Awake()
        {
            if (!SerializedFieldValidation.Validate(typeof(MainMenu), this, false)) return;
            _PlayButton.onClick.AddListener(Play);
            _SettingsButton.onClick.AddListener(() => _ = GameManager.WindowManager.OpenReplace("Settings"));
            _CreditsButton.onClick.AddListener(() => _CreditsCanvas.SetActive(true));
            _FeedbackButton.onClick.AddListener(() => Application.OpenURL(_FeedbackURL));
            //_ReadmeButton.onClick.AddListener(() => _ReadmeCanvas.gameObject.SetActive(true));
            _QuitButton.onClick.AddListener(Quit);

            foreach (Button shopButton in _ShopButtons)
            {
                shopButton.onClick.AddListener(async () => {
                    GameManager.AudioManager.SetParameterOverTime("MenuMusic", "fade", 0, 1);
                    await GameManager.SceneLoader.LoadScene("Shop");
                    GameManager.AudioManager.SetParameter("ShopMusic", "fade", 1);
                    GameManager.AudioManager.PlaySound("ShopMusic");
                });
            }
            _LevelBackButton.onClick.AddListener(LevelSelectBack);

            LevelSelectPanel.SetActive(false);
        }

        protected void Start()
        {
            //_ReadmeCanvas.StartTween();
            //_ReadmeCanvas.gameObject.SetActive(true);
        }

        private void SetButtonsInteractable(bool state)
        {
            _PlayButton.interactable = state;
            _SettingsButton.interactable = state;
            _CreditsButton.interactable = state;
            //_ReadmeButton.interactable = state;
            _FeedbackButton.interactable = state;
            _QuitButton.interactable = state;
        }

        private void Quit()
        {
            Application.Quit();
        }

        private void Play()
        {
            SetButtonsInteractable(false);
            LevelSelectPanel.SetActive(true);
            _MainMenuPanel.SetActive(false);
        }

        private void LevelSelectBack()
        {
            LevelSelectPanel.SetActive(false);
            _MainMenuPanel.SetActive(true);
            SetButtonsInteractable(true);
        }

        public override async Task OnOpen(bool shouldPlayAnimation)
        {
            await Task.CompletedTask;
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            return Task.FromResult(true);
        }
    }
}