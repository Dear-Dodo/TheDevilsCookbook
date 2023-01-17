using System;
using UnityEngine.VFX;

namespace TDC.Affectables
{
    /// <summary>
    /// How modifier values stack on eachother.
    /// </summary>
    public enum StackMode
    {
        /// <summary>
        /// Do not stack modifier.
        /// </summary>
        None,
        /// <summary>
        /// Add values together.
        /// </summary>
        Additive,
        /// <summary>
        /// Add (newValue - 1) to the current value.
        /// </summary>
        AdditiveMinusOne,
        /// <summary>
        /// Multiply values together.
        /// </summary>
        Multiplicative
    }
    [Serializable]
    public class StatModifier : ICloneable
    {
        public string StatName;
        public StatOperation Operation;
        public StackMode ValueStackMode;
        public float Value;
        public float StackValue;
        public VisualEffectAsset AppliedVFX;
        public Action<VisualEffect, AffectableStats> OnVFXSpawned;

        public object Clone()
        {
            return new StatModifier()
            {
                StatName = StatName,
                Operation = Operation,
                ValueStackMode = ValueStackMode,
                Value = Value,
                StackValue = StackValue
            };
        }
    }
}
