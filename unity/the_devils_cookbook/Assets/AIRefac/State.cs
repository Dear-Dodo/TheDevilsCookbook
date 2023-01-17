using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using UnityEngine;

namespace TDC.AIRefac
{
    public abstract class State
    {
        protected event System.Action GizmosDrawn;
        
        private List<Transition> _Transitions = new List<Transition>();

        public readonly AsyncManualResetEvent StateCompleted = new AsyncManualResetEvent(false);

        public async Task Run(StateMachine runner, CancellationToken token)
        {
            try
            {
                StateCompleted.Reset();
                await OnStateStart(runner, token);
                await RunBehaviour(runner, token);
                await OnStateEnd(runner, token);
                StateCompleted.Set();
            }
            catch (OperationCanceledException) {}
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public bool TestTransitions(out Transition succeeded)
        {
            succeeded = _Transitions.FirstOrDefault(
                t => (!t.TransitionAtEnd || StateCompleted.IsSet) && (t.Condition == null || t.Condition()));
            return succeeded != null;
        }

        public void AddTransition(Transition transition)
        {
            _Transitions.Add(transition);
        }
        
        protected abstract Task OnStateStart(StateMachine runner, CancellationToken token);

        protected abstract Task RunBehaviour(StateMachine runner, CancellationToken token);

        protected abstract Task OnStateEnd(StateMachine runner, CancellationToken token);

        public virtual void OnDrawGizmosSelected()
        {
            GizmosDrawn?.Invoke();
        }
    }
}