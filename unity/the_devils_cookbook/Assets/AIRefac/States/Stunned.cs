using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac.Actions;
using TDC.Core.Extension;
using TDC.Core.Manager;
using UnityEngine;
using Utility;

namespace TDC.AIRefac.States
{
    public class Stunned : State
    {
        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            if (runner.AttachedAgent.Animator != null)
            {
                runner.AttachedAgent.Animator.SetBool("Stunned",true);
            }
            return Task.CompletedTask;
        }

        protected override Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            if (runner.AttachedAgent.Animator != null)
            {
                runner.AttachedAgent.Animator.SetBool("Stunned", false);
            }
            return Task.CompletedTask;
        }
    }
}