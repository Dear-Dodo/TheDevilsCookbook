using System;

namespace TDC.UI.Objectives
{
    public enum ObjectiveStatus
    {
        Incomplete,
        Completed,
        Failed
    }
    public abstract class Objective
    {
        public event Action<bool> Completed;
        public ObjectiveStatus Status { get; protected set; }
        public virtual void Complete(bool triggerListCompletionCheck = true)
        {
            if (Status == ObjectiveStatus.Completed) return;
            Status = ObjectiveStatus.Completed;
            Completed?.Invoke(triggerListCompletionCheck);
        }
        public virtual void Fail()
        {
            Status = ObjectiveStatus.Failed;
        }

        public abstract void PollIfAvailable();
        
        public readonly string Text;

        public Objective(string text)
        {
            Text = text;
        }
    }

    public class QuantifiableObjective : Objective
    {
        public event Action<float> QuantityChanged;
        public float CurrentQuantity { get; protected set; }
        public readonly float TargetQuantity;

        private readonly Func<float> _Poller;

        public void Increment(float value = 1)
        {
            CurrentQuantity += value;
            QuantityChanged?.Invoke(CurrentQuantity);
            if (CurrentQuantity >= TargetQuantity) Complete();
        }

        public void SetQuantity(float value)
        {
            CurrentQuantity = value;
            QuantityChanged?.Invoke(CurrentQuantity);
            if (CurrentQuantity >= TargetQuantity) Complete();
            // TODO Check for uncompletion
        }

        public void Decrement(float value = 1)
        {
            CurrentQuantity -= value;
            QuantityChanged?.Invoke(CurrentQuantity);
            // TODO Uncomplete
        }

        public override void PollIfAvailable()
        {
            if (_Poller == null) return;
            SetQuantity(_Poller());
        }
        
        public QuantifiableObjective(string text, float target, float current = 0, Func<float> poller = null) : base(text)
        {
            CurrentQuantity = current;
            TargetQuantity = target;
            _Poller = poller;
        }
    }
    
    public class BooleanObjective : Objective
    {
        private Func<bool> _Poller;

        public void SetValue(bool value)
        {
            if (value) Complete();
            // TODO: uncomplete
        }
        
        public override void PollIfAvailable()
        {
            if (_Poller == null) return;
            SetValue(_Poller());
        }

        public BooleanObjective(string text) : base(text)
        {
        }
    }
}