using Nito.AsyncEx;
using System.Threading.Tasks;

namespace TDC.Core.Manager
{
    public abstract class GameManagerSubsystem
    {
        public static AsyncManualResetEvent InitialisedAsync { get; } = new AsyncManualResetEvent(false);

        public async void Initialise()
        {
            await GameManager.InitialisedAsync.WaitAsync();
            GameManager.SceneLoader.OnSceneLoadStarted += _ => Reset();
            await OnInitialise();
            InitialisedAsync.Set();
        }

        protected virtual Task OnInitialise() => Task.CompletedTask;

        protected virtual void Reset()
        { /*void*/ }
    }
}