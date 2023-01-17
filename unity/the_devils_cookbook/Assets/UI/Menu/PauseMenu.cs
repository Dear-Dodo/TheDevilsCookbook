using System;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.UI.Windowing;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public class PauseMenu : Window
    {
        [SerializeField, SerializedValueRequired]
        private Button _SettingsButton;

        [SerializeField, SerializedValueRequired]
        private Button _ExitToMenu;

        private WindowNode _Settings;

        private void Start()
        {
            _ExitToMenu.onClick.AddListener(OnExitToMenu);
            _SettingsButton.onClick.AddListener(OpenSettings);
        }

        public void OpenSettings()
        {
            _Settings = GameManager.WindowManager.OpenAdditive("Settings").Result;
            _Settings.OnClosed += () => _Settings = null;
        }

        public override Task OnOpen(bool shouldPlayAnimation)
        {
            GameManager.Timescale = 0;
            return Task.CompletedTask;
        }

        public override async Task<bool> OnClose(bool shouldPlayAnimation)
        {
            if (_Settings != null)
            {
                await _Settings.Close(true);
                _Settings = null;
                return false;
            }
            else
            {
                GameManager.Timescale = 1;
                return true;
            }
        }

        private void OnExitToMenu()
        {
            GameManager.Timescale = 1;
            try
            {
                GameManager.AudioManager.StopSound("TutorialMusic");
                GameManager.AudioManager.StopSound("LevelMusic");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            GameManager.SceneLoader?.LoadScene("Main Menu");
        }
    }
}