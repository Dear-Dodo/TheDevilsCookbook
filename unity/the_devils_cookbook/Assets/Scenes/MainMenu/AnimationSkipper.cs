using TDC.Core.Manager;
using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDC.Scenes.MainMenu
{
    public class AnimationSkipper : MonoBehaviour
    {
        [SerializeField, SerializedValueRequired]
        private Animator _Animator;

        private static readonly int _Skip = Animator.StringToHash("Skip");

        private async void Awake()
        {
            await GameManager.InitialisedAsync.WaitAsync();
            GameManager.AudioManager.PlaySound("MenuMusic");
            GameManager.AudioManager.SetParameter("MenuMusic", "SkipMenuCutscene", 0);
            GameManager.AudioManager.SetParameter("MenuMusic", "Fade", 1);
            RegisterClick();
        }

        private void RegisterClick()
        {
            GameManager.PlayerControls.UI.Click.performed += OnClick;
        }
        
        private void OnDestroy()
        {
            GameManager.PlayerControls.UI.Click.performed -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            _Animator.SetTrigger(_Skip);
            GameManager.AudioManager.SetParameter("MenuMusic", "SkipMenuCutscene", 1);
        }
    }
}
