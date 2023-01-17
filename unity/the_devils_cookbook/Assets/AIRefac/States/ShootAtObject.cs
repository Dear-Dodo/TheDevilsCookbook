using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using TDC.AIRefac.Actions;
using TDC.Affectables;
using TDC.Core.Manager;
using FMODUnity;

namespace TDC.AIRefac.States
{
    public class ShootAtObject : State
    {
        public GameObject Target;
        public Projectile Projectile;
        private float _AimThreshold = 25.0f;
        private float _Timer;
        public float FireRate;
        public EventReference SoundEffect;

        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            await GameManager.PlayerInitialised.WaitAsync();
            while (!token.IsCancellationRequested && Application.isPlaying)
            {
                if (Target != null)
                {
                    var rotateAction = new RotateTowardsTarget(runner, Target.transform.position);
                    await rotateAction.Run(token);
                    if (!(runner.AttachedAgent.Stats.ModifiedStats["Silenced"] > 0))
                    {
                        if (Quaternion.Angle(runner.AttachedAgent.transform.rotation,
                            Quaternion.LookRotation(Target.transform.position -
                            runner.AttachedAgent.transform.position, Vector3.up)) <= _AimThreshold && _Timer < 0)
                        {
                            _Timer = FireRate;
                            var shootAction = new SpawnProjectile(runner, Target, Projectile, SoundEffect);
                            await shootAction.Run(token);
                        }
                        else
                        {
                            _Timer -= Time.fixedDeltaTime;
                        }
                    }
                }
            }
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
