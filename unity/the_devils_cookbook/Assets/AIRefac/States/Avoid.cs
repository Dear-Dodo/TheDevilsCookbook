using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac.Actions;
using UnityEngine;
using UnityEngine.AI;

namespace TDC.AIRefac.States
{
    public class Avoid : State
    {
        public GameObject AvoidTarget;
        
        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            
            var moveAction = new MoveToPosition(runner, Vector3.zero);
            while (!token.IsCancellationRequested && Application.isPlaying)
            {
                NavMesh.SamplePosition(
                    runner.AttachedAgent.transform.position + (runner.AttachedAgent.transform.position - AvoidTarget.transform.position).normalized,
                    out NavMeshHit target, 2f, NavMesh.AllAreas
                );
                moveAction.Target = target.position;
                GizmosDrawn += moveAction.OnDrawGizmosSelected;
                await moveAction.Run(token);
                GizmosDrawn -= moveAction.OnDrawGizmosSelected;
            }
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}