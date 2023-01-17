using FMODUnity;
using TDC.Affectables;
using TDC.AIRefac.States;
using TDC.Core.Manager;
using UnityEngine;

namespace TDC.AIRefac.Ingredients
{
    public class BatAgent : Agent
    {
        //[Header("Obstacle Avoidance")]
        //public float CastAngle = 75.0f;
        //public int CastCount = 8;
        //public float CastDistance = 2.0f;

        public float FollowRange;
        public LayerMask LineOfSightMask;
        public Projectile Projectile;
        public float FireRate;
        public EventReference ShootSoundEffect;

        private StrafeWander _WanderState = new StrafeWander();
        private StrafeAvoid _AvoidState = new StrafeAvoid();
        private StrafeFollow _FollowState = new StrafeFollow();

        private StateMachine AimController;

        private ShootAtObject _ShootAtPlayerState = new ShootAtObject();
        private RotateTowardsMotion _RotateToMotionState = new RotateTowardsMotion();

        private void OnValidate()
        {
            //_WanderState.Angle = CastAngle;
            //_WanderState.CastCount = CastCount;
            //_WanderState.CastDistance = CastDistance;
        }

        protected override async void Awake()
        {
            base.Awake();
            await GameManager.PlayerInitialised.WaitAsync();

            StateMachine = new StateMachine(this);

            _AvoidState.AvoidTarget = GameManager.PlayerCharacter.gameObject;
            _FollowState.FollowTarget = GameManager.PlayerCharacter.gameObject;

            _WanderState.AddTransition(new Transition(_AvoidState, false, InRangeOfPlayer));
            _WanderState.AddTransition(new Transition(_FollowState, false, CanSeePlayer));
            _AvoidState.AddTransition(new Transition(_FollowState, false, InFollowRangeOfPlayer));
            _AvoidState.AddTransition(new Transition(_WanderState, false, OutOfRangeOfPlayer));
            _FollowState.AddTransition(new Transition(_AvoidState, false, InRangeOfPlayer));
            _FollowState.AddTransition(new Transition(_WanderState, false, CantSeePlayer));

            var StunnedState = new Stunned();
            StateMachine.AddGlobalTransition(new Transition(StunnedState, false, IsStunned));
            StunnedState.AddTransition(new Transition(_WanderState, false, IsNotStunned));

            StateMachine.AddState(_WanderState);
            StateMachine.AddState(_FollowState);
            StateMachine.AddState(_AvoidState);
            StateMachine.AddState(StunnedState);

            AimController = new StateMachine(this);

            _ShootAtPlayerState.Target = GameManager.PlayerCharacter.gameObject;
            _ShootAtPlayerState.Projectile = Projectile;
            _ShootAtPlayerState.FireRate = FireRate;
            _ShootAtPlayerState.SoundEffect = ShootSoundEffect;

            var StunnedStateAim = new Stunned();
            AimController.AddGlobalTransition(new Transition(_RotateToMotionState, false, IsStunned));
            StunnedStateAim.AddTransition(new Transition(_WanderState, false, IsNotStunned));

            _RotateToMotionState.AddTransition(new Transition(_ShootAtPlayerState, false, CanShootPlayer));
            _ShootAtPlayerState.AddTransition(new Transition(_RotateToMotionState, false, CantShootPlayer));

            AimController.AddState(_RotateToMotionState);
            AimController.AddState(_ShootAtPlayerState);
            AimController.AddState(StunnedStateAim);
        }

        public override void Enable()
        {
            base.Enable();
            GameManager.RunOnInitialisation(() => _ = AimController.Run());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            AimController.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AimController.Stop();
        }

        private bool InRangeOfPlayer()
        {
            return Vector3.Distance(GameManager.PlayerCharacter.transform.position,transform.position) < FollowRange;
        }

        private bool InFollowRangeOfPlayer()
        {
            return OutOfRangeOfPlayer() && CanSeePlayer();
        }

        private bool CanShootPlayer()
        {
            return Vector3.Distance(GameManager.PlayerCharacter.transform.position, transform.position) < FollowRange * 1.5f && CanSeePlayer();
        }
        private bool CantShootPlayer()
        {
            return Vector3.Distance(GameManager.PlayerCharacter.transform.position, transform.position) >= FollowRange * 1.5f || CantSeePlayer();
        }
        private bool OutOfRangeOfPlayer()
        {
            return Vector3.Distance(GameManager.PlayerCharacter.transform.position, transform.position) > FollowRange;
        }

        private bool CanSeePlayer()
        {
            Vector3 HeightOffset = new Vector3(0, 1, 0);
            if (Physics.Raycast((transform.position + HeightOffset), (GameManager.PlayerCharacter.transform.position + HeightOffset) -
                (transform.position + HeightOffset), out RaycastHit hit, Vector3.Distance(transform.position, GameManager.PlayerCharacter.transform.position), LineOfSightMask))
            {
                // Debug.Log(hit.collider.gameObject.name);
            }
            return !Physics.Raycast((transform.position + HeightOffset), (GameManager.PlayerCharacter.transform.position + HeightOffset) -
                (transform.position + HeightOffset), Vector3.Distance(transform.position, GameManager.PlayerCharacter.transform.position), LineOfSightMask);
        }

        private bool CantSeePlayer()
        {
            return !CanSeePlayer();
        }

        bool IsStunned()
        {
            return Stats.ModifiedStats["Stunned"] > 0;
        }

        bool IsNotStunned()
        {
            return !IsStunned();
        }
    }
}