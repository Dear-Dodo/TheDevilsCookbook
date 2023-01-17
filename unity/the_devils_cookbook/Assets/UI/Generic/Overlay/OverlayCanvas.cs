using TDC.Core.Manager;
using TDC.Core.Type;

namespace TDC.UI.Generic.Overlay
{
    [SingletonInitializeOnRuntime, Persistent, AddressablePrefab("OverlayCanvas")]
    public class OverlayCanvas : Singleton<OverlayCanvas>
    {
        protected override async void Awake()
        {
            base.Awake();
            await GameManager.InitialisedAsync.WaitAsync();
            GameManager.SceneLoader.OnSceneLoadStarted += OnSceneChange;
        }

        private void OnSceneChange(SceneEntry entry)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
