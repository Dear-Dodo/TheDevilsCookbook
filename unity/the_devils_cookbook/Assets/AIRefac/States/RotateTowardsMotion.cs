using UnityEngine;
using TDC.AIRefac.Actions;
using System.Threading.Tasks;
using System.Threading;

namespace TDC.AIRefac.States
{
    public class RotateTowardsMotion : State
    {
        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            while (!token.IsCancellationRequested && Application.isPlaying)
            {
                var rotateAction = new RotateTowardsTarget(runner, runner.AttachedAgent.transform.position + runner.AttachedAgent.MomentumController.GetTarget());
                await rotateAction.Run(token);
            }
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
