using FMODUnity;
using TDC.AIRefac.States;
using TDC.Core.Manager;
using TDC.Core.Utility;
using UnityEngine;

namespace TDC.AIRefac.Ingredients
{
    public class SpiderAgent : Agent
    {
        public float AvoidRange;
        public EventReference PanicSoundEffect;

        private Wander _WanderState = new Wander();
        private Avoid _AvoidState = new Avoid();

        protected override async void Awake()
        {
            base.Awake();
            await GameManager.PlayerInitialised.WaitAsync();

            StateMachine = new StateMachine(this);

            _AvoidState.AvoidTarget = GameManager.PlayerCharacter.gameObject;

            _WanderState.AddTransition(new Transition(_AvoidState, false, InRangeOfPlayer));
            _AvoidState.AddTransition(new Transition(_WanderState, false, OutOfRangeOfPlayer));

            bool IsStunned()
            {
                return Stats.ModifiedStats["Stunned"] > 0;
            }

            bool IsNotStunned()
            {
                return !IsStunned();
            }

            var StunnedState = new Stunned();
            StateMachine.AddGlobalTransition(new Transition(StunnedState, false, IsStunned));
            StunnedState.AddTransition(new Transition(_WanderState, false, IsNotStunned));

            StateMachine.AddState(_WanderState);
            StateMachine.AddState(_AvoidState);
            StateMachine.AddState(StunnedState);
        }

        private bool InRangeOfPlayer()
        {
            if (Vector3.Distance(GameManager.PlayerCharacter.transform.position, transform.position) < AvoidRange && Stats.ModifiedStats["Silenced"] > 0)
            {
                if (!StateMachine.ActiveState.Equals(_AvoidState)) { SFXHelper.PlayOneshot(PanicSoundEffect, gameObject); }
                return true;
            }
            return false;
        }

        private bool OutOfRangeOfPlayer()
        {
            return !InRangeOfPlayer();
        }
    }
}