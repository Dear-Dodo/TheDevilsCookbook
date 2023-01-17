using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

namespace TDC.AIRefac
{
    public class StateMachine
    {
        private List<State> _States = new List<State>();
        private State _EntryPoint;
        public State ActiveState { get { return _ActiveState; }}
        private State _ActiveState;
        
        public readonly Agent AttachedAgent;

        private List<Transition> _GlobalTransitions = new List<Transition>();

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationTokenSource _StateTokenSource = new CancellationTokenSource();

        public async Task Run()
        {
            try
            {
                CancellationToken token = _TokenSource.Token;
                _ActiveState = _EntryPoint;
                _ = _ActiveState.Run(this, _StateTokenSource.Token);

                while (!token.IsCancellationRequested && Application.isPlaying)
                {
                    try
                    {
                        await Await.NextUpdate().ConfigureAwait(token);
                        if (TestGlobalTransitions(out Transition target)) MoveToNewState(target.Target);
                        if (_ActiveState.TestTransitions(out target)) MoveToNewState(target.Target);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

                _ActiveState = null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            
            if (_TokenSource.IsCancellationRequested) Debug.Log("Cancelled");
        }

        public void Stop()
        {
            _StateTokenSource.Cancel();
            _StateTokenSource = new CancellationTokenSource();
            _TokenSource.Cancel();
            _TokenSource = new CancellationTokenSource();
        }

        
        /// <summary>
        /// Add a new state to the machine. The first state added is considered the entry point.
        /// </summary>
        /// <param name="state"></param>
        public void AddState(State state)
        {
            _States.Add(state);
            _EntryPoint ??= state;
        }

        /// <summary>
        /// Add a global transition. Global transitions are evaluated regardless of current state.
        /// </summary>
        /// <param name="transition"></param>
        public void AddGlobalTransition(Transition transition)
        {
            _GlobalTransitions.Add(transition);
        }

        private bool TestGlobalTransitions(out Transition suceeded)
        {
            suceeded = _GlobalTransitions.FirstOrDefault(t => t.Condition() && t.Target != _ActiveState);
            return suceeded != null;
        }

        public void MoveToNewState(State state)
        {
            _StateTokenSource.Cancel();
            _StateTokenSource = new CancellationTokenSource();
            _ActiveState = state;
            _ = _ActiveState.Run(this, _StateTokenSource.Token);
        }
        
        public StateMachine(Agent agent)
        {
            AttachedAgent = agent;
        }
        
        public StateMachine(Agent agent, params State[] states)
        {
            AttachedAgent = agent;
            _States = states.ToList();
            _EntryPoint = states[0];
        }

        public void OnDrawGizmosSelected()
        {
            _ActiveState?.OnDrawGizmosSelected();
        }
    }
}