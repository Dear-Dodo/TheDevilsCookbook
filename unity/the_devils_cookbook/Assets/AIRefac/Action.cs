using System.Threading;
using System.Threading.Tasks;

namespace TDC.AIRefac
{
    public abstract class Action
    {
        protected StateMachine Runner;

        public abstract Task Run(CancellationToken token);

        public Action(StateMachine runner)
        {
            Runner = runner;
        }
    }
}