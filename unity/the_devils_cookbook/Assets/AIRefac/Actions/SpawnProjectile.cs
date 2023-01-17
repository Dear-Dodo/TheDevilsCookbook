using FMOD.Studio;
using FMODUnity;
using System.Threading.Tasks;
using System.Threading;
using TDC.Affectables;
using UnityEngine;

namespace TDC.AIRefac.Actions
{
    public class SpawnProjectile : Action
    {
        private Projectile ProjectilePrefab;
        private float ForwardOffset = 0.0f;
        private float VerticalOffset = 0.0f;

        public GameObject Target;

        private EventReference _SoundEffect;

        public SpawnProjectile(StateMachine runner, GameObject target, Projectile projectile, EventReference soundEffect) : base(runner)
        {
            Target = target;
            ProjectilePrefab = projectile;
            _SoundEffect = soundEffect;
        }

        public async override Task Run(CancellationToken token)
        {
                Vector3 startPosition = Runner.AttachedAgent.transform.position + Runner.AttachedAgent.transform.forward *
                    ForwardOffset + new Vector3(0, VerticalOffset, 0);
                var projectile = Object.Instantiate(ProjectilePrefab.gameObject, startPosition, Quaternion.identity)
                    .GetComponent<Projectile>();
                projectile.Initialize(startPosition, Target.transform.position);


                EventInstance Sound = RuntimeManager.CreateInstance(_SoundEffect);
                Sound.set3DAttributes(RuntimeUtils.To3DAttributes(Runner.AttachedAgent.gameObject));
                Sound.start();
                Sound.release();
                return;
        }
    }
}