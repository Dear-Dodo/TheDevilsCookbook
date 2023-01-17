using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac.Actions;
using TDC.Core.Extension;
using TDC.Core.Manager;
using UnityEngine;
using Utility;

namespace TDC.AIRefac.States
{
    public class StrafeWander : State
    {
        //public float Angle = 75.0f;
        //public int CastCount = 8;
        //public float CastDistance = 2.0f;
        
        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            while (!token.IsCancellationRequested && Application.isPlaying)
            {
                Vector2 target = GameManager.CurrentLevelData.PoissonDisc.Points.Random(GameManager.GameRandom);
                var moveAction = new StrafeToPosition(runner, target.xoy());
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