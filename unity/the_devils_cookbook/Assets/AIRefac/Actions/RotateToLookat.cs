using System.Threading;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

namespace TDC.AIRefac.Actions
{
    public class RotateToLookat : Action
    {
        public Transform Target;
        // This should really be an Effect but I really can't rn
        public float RotationSpeedModifier;

        public RotateToLookat(StateMachine runner, Transform target, float rotationSpeedModifier = 1) : base(runner)
        {
            Target = target;
            RotationSpeedModifier = rotationSpeedModifier;
        }

        public override async Task Run(CancellationToken token)
        {
            Debug.Assert(Target != null);
            Transform agentTransform = Runner.AttachedAgent.Transform;
            while (!token.IsCancellationRequested && Application.isPlaying)
            {
                float rotationSpeed = Runner.AttachedAgent.Stats.ModifiedStats["Rotation Speed"] * Time.fixedDeltaTime *
                                      RotationSpeedModifier;
                Quaternion currentRotation = agentTransform.rotation;
                Vector3 agentPos = agentTransform.position;
                Vector3 targetPos = Target.position;
                var lookPosition = new Vector3(targetPos.x, agentPos.y, targetPos.z);
                Quaternion target = Quaternion.LookRotation(lookPosition - agentPos, Vector3.up);
                agentTransform.rotation = currentRotation = Quaternion.RotateTowards(currentRotation, target, rotationSpeed);
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate();
            }
        }
    }
}