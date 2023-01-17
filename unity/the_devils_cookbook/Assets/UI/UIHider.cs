using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Manager;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TDC.UI
{
    public class UIHider : GameManagerSubsystem
    {

        private bool _IsInitialisedForScene = false;
        public bool IsUIHidden { get; private set; }

        private List<IHideable> _SceneHideables = new List<IHideable>();
        
        protected override Task OnInitialise()
        {
            GameManager.SceneLoader.OnSceneLoadFinished += InitialiseHideables;
            GameManager.PlayerControls.UI.Hide.performed += OnHideInputPerformed;
            InitialiseHideables(null);
            return Task.CompletedTask;
        }

        private void OnHideInputPerformed(InputAction.CallbackContext _) => ToggleHide();
        
        public bool ToggleHide()
        {
            IsUIHidden = !IsUIHidden;
            UpdateHideables();
            return IsUIHidden;
        }
        
        private void InitialiseHideables(SceneEntry _)
        {
            _SceneHideables = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(obj => obj.GetComponentsInChildren<IHideable>(true)).ToList();
            // _SceneHideables = Object.FindObjectsOfType<MonoBehaviour>().OfType<IHideable>().ToList();
        }

        private void UpdateHideables()
        {
            foreach (IHideable hideable in _SceneHideables)
            {
                hideable.SetHidden(IsUIHidden);
            }
        }
    }
}
