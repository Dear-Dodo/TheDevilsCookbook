using System;
using System.Threading;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

namespace TDC.AIRefac.Actions
{
    public class RotateTowardsTarget : Action
    {
        private Vector3 _Target;
        private float _TurnSpeed = 10.0f;
        public RotateTowardsTarget(StateMachine runner, Vector3 target) : base(runner)
        {
            _Target = target;
        }

        public override async Task Run(CancellationToken token)
        {
            Transform transform = Runner.AttachedAgent.transform;
            if ((_Target - transform.position).sqrMagnitude > 0)
            {
                Debug.DrawLine(transform.position, transform.position + Quaternion.LookRotation(_Target - transform.position, Vector3.up) * Vector3.forward, Color.yellow);
                Debug.DrawLine(transform.position, _Target, Color.blue);
                Quaternion targetDir = Quaternion.LookRotation(_Target - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDir, _TurnSpeed);
            }
            await new WaitForFixedUpdate();
            await Await.NextFixedUpdate().ConfigureAwait(token);
        }
    }
}
