using System;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Extension;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.UI.Windowing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public class SettingsMenu : Window
    {
        [SerializeField, SerializedValueRequired]
        private TMP_Dropdown _DashMode;

        [SerializeField, SerializedValueRequired]
        private Toggle _VSync;

        [SerializeField, SerializedValueRequired]
        private TMP_InputField _FPSLimit;

        [SerializeField, SerializedValueRequired]
        private Slider _MasterVolume;

        [SerializeField, SerializedValueRequired]
        private Slider _MusicVolume;

        [SerializeField, SerializedValueRequired]
        private Slider _SFXVolume;

        [SerializeField, SerializedValueRequired]
        private Button _Close;

        [SerializeField, SerializedValueRequired]
        private TMP_Dropdown _WindowMode;

        [SerializeField, SerializedValueRequired]
        private TMP_Dropdown _WindowResolution;

        [SerializeField, SerializedValueRequired]
        private TMP_Dropdown _Quality;

        public override Task OnOpen(bool shouldPlayAnimation)
        {
            return Task.CompletedTask;
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            return Task.FromResult(true);
        }

        private void Start()
        {
            _DashMode.ClearOptions();
            _DashMode.AddOptions(Enum.GetNames(typeof(UserSettings.DashDirectionMode)).ToList());
            _DashMode.onValueChanged.AddListener(OnDashModeSet);
            _DashMode.SetValueWithoutNotify((int)GameManager.UserSettings.DashMode);

            _WindowMode.ClearOptions();
            _WindowMode.AddOptions(Enum.GetNames(typeof(FullScreenMode)).ToList());
            _WindowMode.onValueChanged.AddListener(OnWindowModeSet);
            _WindowMode.SetValueWithoutNotify((int)Screen.fullScreenMode);

            _WindowResolution.ClearOptions();
            _WindowResolution.AddOptions(Screen.resolutions.Select(x => $"{x.width} x {x.height}").ToList());
            _WindowResolution.onValueChanged.AddListener(OnWindowResolutionSet);
            _WindowResolution.SetValueWithoutNotify(Screen.resolutions.FirstIndexOf(x => x.height == Screen.currentResolution.height && x.width == Screen.currentResolution.width));

            _Quality.ClearOptions();
            _Quality.AddOptions(QualitySettings.names.ToList());
            _Quality.onValueChanged.AddListener(QualitySettings.SetQualityLevel);
            _Quality.SetValueWithoutNotify(QualitySettings.GetQualityLevel());

            _VSync.SetIsOnWithoutNotify(GameManager.UserSettings.VSync);
            _VSync.onValueChanged.AddListener(OnVSyncSet);

            _FPSLimit.text = GameManager.UserSettings.FrameRateLimit.ToString();
            _FPSLimit.interactable = !_VSync.isOn;
            _FPSLimit.onSubmit.AddListener(OnFPSLimitSet);

            _MasterVolume.value = GameManager.AudioManager.MasterVolume;
            _MasterVolume.onValueChanged.AddListener((volume) => GameManager.AudioManager.MasterVolumeLevel(volume));

            _MusicVolume.value = GameManager.AudioManager.MusicVolume;
            _MusicVolume.onValueChanged.AddListener((volume) => GameManager.AudioManager.MusicVolumeLevel(volume));

            _SFXVolume.value = GameManager.AudioManager.SFXVolume;
            _SFXVolume.onValueChanged.AddListener((volume) => GameManager.AudioManager.SFXVolumeLevel(volume));

            _Close.onClick.AddListener(() => _ = GameManager.WindowManager.Close(this));
        }

        private void OnDashModeSet(int value)
        {
            GameManager.UserSettings.DashMode = (UserSettings.DashDirectionMode)value;
            Debug.Log($"DashMode set to {GameManager.UserSettings.DashMode}");
        }

        private void OnWindowModeSet(int value)
        {
            Screen.SetResolution(Screen.width, Screen.height, (FullScreenMode)value);
            Debug.Log($"Fullscreen Mode set to {Screen.fullScreenMode}");
        }

        private void OnWindowResolutionSet(int value)
        {
            var resolution = Screen.resolutions[value];
            if (GameManager.UserSettings.VSync)
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, GameManager.UserSettings.FrameRateLimit);
            else Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        }

        private void OnVSyncSet(bool value)
        {
            _FPSLimit.interactable = !value;
            GameManager.UserSettings.VSync = value;
        }

        private void OnFPSLimitSet(string input)
        {
            int value = int.Parse(input);
            int clamped = Mathf.Max(value, UserSettings.MinFrameRateLimit);
            if (value != clamped) _FPSLimit.text = clamped.ToString();
            GameManager.UserSettings.FrameRateLimit = clamped;
        }
    }
}