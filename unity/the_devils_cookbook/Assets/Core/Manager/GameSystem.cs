using System.Threading.Tasks;

namespace TDC.Core.Manager
{
    public abstract class GameSystem
    {
        public abstract Task OnConstruct();

        public abstract Task OnLevelStart();

        public abstract Task OnGameStart();

        public abstract Task OnUpdateSystem();

        public abstract Task OnGameStop();

        public abstract Task OnLevelEnd(bool win);

        public abstract Task OnDestruct();
    }
}