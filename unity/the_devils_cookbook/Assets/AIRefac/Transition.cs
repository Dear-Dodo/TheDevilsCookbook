using System;

namespace TDC.AIRefac
{
    public class Transition
    {
        public bool TransitionAtEnd;
        public Func<bool> Condition;
        public State Target;

        public Transition(State target, bool transitionAtEnd, Func<bool> condition = null)
        {
            Target = target;
            TransitionAtEnd = transitionAtEnd;
            Condition = condition;
        }
    }
}