using System;
using System.Threading;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;
using UnityEngine.AI;

namespace TDC.AIRefac.Actions
{
    public class StrafeToPosition : Action
    {
        public Vector3 Target;

        private float _TargetSpeed;

        //public float Angle = 75.0f;
        //public int CastCount = 8;
        //public float CastDistance = 2.0f;

        public int AgentID = 0;

        public float DistanceTolerance = 1.0f;
        
        private NavMeshPath _Path = new NavMeshPath();

        public StrafeToPosition(StateMachine runner, Vector3 target) : base(runner)
        {
            Target = target;
        }

        public override async Task Run(CancellationToken token)
        {
            float sqrDistanceTolerance = Mathf.Pow(DistanceTolerance, 2);
            Transform agentTransform = Runner.AttachedAgent.transform;
            Rigidbody agentRigidbody = Runner.AttachedAgent.Rigidbody;
            Collider agentCollider = Runner.AttachedAgent.GetComponent<Collider>();
            // _CurrentVelocity = agentRigidbody.velocity;
            do
            {
                float moveSpeed = Runner.AttachedAgent.Stats.ModifiedStats["Movement Speed"];
                float acceleration = 10.0f * Time.fixedDeltaTime;
                float drag = 10.0f * Time.fixedDeltaTime;

                Vector3 agentPos = agentTransform.position;
                _Path.ClearCorners();
                if (!NavMesh.CalculatePath(agentPos, Target,
                        new NavMeshQueryFilter() { agentTypeID = AgentID, areaMask = NavMesh.AllAreas }, _Path) || _Path.corners.Length < 2)
                {
                    await Await.NextFixedUpdate().ConfigureAwait(token);
                    continue;
                };

                Vector3 waypointDisplacement = _Path.corners[1] - agentPos;
                Vector3 dirToWaypoint = waypointDisplacement.normalized;

                // Vector3 avoidance = ObstacleAvoidance.AvoidCast(agentCollider.bounds.center, targetHeading, Angle,
                //     CastCount, CastDistance, out float avoidFactor);
                // Debug.DrawRay(agentPos, targetHeading, Color.blue);
                // // targetHeading = (targetHeading + avoidance) / 2;
                // targetHeading = Vector3.Slerp(targetHeading, avoidance, avoidFactor);
                // Debug.DrawRay(agentPos, avoidance, Color.magenta);

                Vector3 agentForward = agentTransform.forward;


                Runner.AttachedAgent.MomentumController.SetDrag(dirToWaypoint, drag);

                _TargetSpeed = moveSpeed;

                Vector3 targetVelocity = dirToWaypoint * _TargetSpeed;
                Runner.AttachedAgent.MomentumController.SetTarget(targetVelocity, acceleration);
                // Debug.DrawRay(agentTransform.position, targetVelocity, Color.red);
                // Debug.DrawRay(agentTransform.position, _CurrentVelocity, Color.cyan);

                await new WaitForFixedUpdate();
                try
                {
                    await Await.NextFixedUpdate().ConfigureAwait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            } while (!token.IsCancellationRequested && Application.isPlaying && (agentTransform.position - Target).sqrMagnitude > sqrDistanceTolerance);
            
            if (Runner.AttachedAgent) Runner.AttachedAgent.MomentumController.SetTarget(Vector3.zero, 10.0f * Time.fixedDeltaTime);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            for (var i = 0; i < _Path.corners.Length - 1; i++)
            {
                Vector3 current = _Path.corners[i];
                Vector3 next = _Path.corners[i + 1];
                Gizmos.DrawSphere(current, 0.25f);
                Gizmos.DrawLine(current, next);
            }

            if (_Path?.corners?.Length == null || _Path.corners.Length == 0) return;
            Gizmos.DrawSphere(_Path.corners[_Path.corners.Length - 1], 0.25f);
        }
    }
}