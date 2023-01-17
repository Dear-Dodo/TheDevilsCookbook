using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac.Actions;
using TDC.Core.Extension;
using TDC.Core.Manager;
using UnityEngine;
using Utility;

namespace TDC.AIRefac.States
{
    public class Wander : State
    {
        public float Angle = 75.0f;
        public int CastCount = 8;
        public float CastDistance = 2.0f;
        private static readonly int _StartWalk = Animator.StringToHash("StartWalk");
        private static readonly int _StartIdle = Animator.StringToHash("StartIdle");

        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            while (!token.IsCancellationRequested && Application.isPlaying && runner.AttachedAgent != null)
            {
                Vector2 target = GameManager.CurrentLevelData.PoissonDisc.Points.Random(GameManager.GameRandom);
                var moveAction = new MoveToPosition(runner, target.xoy())
                {
                    Angle = Angle,
                    CastCount = CastCount,
                    CastDistance = CastDistance
                };
                GizmosDrawn += moveAction.OnDrawGizmosSelected;
                if (runner.AttachedAgent.Animator != null) { runner.AttachedAgent.Animator.SetTrigger(_StartWalk); }
                await moveAction.Run(token);
                if (runner.AttachedAgent.Animator != null) { runner.AttachedAgent.Animator.SetTrigger(_StartIdle); }
                GizmosDrawn -= moveAction.OnDrawGizmosSelected;
            }
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            // TODO: Replace with acceleration stat
            runner.AttachedAgent.MomentumController.SetTarget(Vector3.zero, 0.22f);
            return Task.CompletedTask;
        }
    }
}