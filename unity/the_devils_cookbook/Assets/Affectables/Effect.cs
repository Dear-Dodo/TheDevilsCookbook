using UnityEngine;

namespace TDC.Affectables
{
    /// <summary>
    /// Behaviour when an effect is applied to an AffectableStats that already has an effect of the same type.
    /// </summary>
    public enum ReapplicationMode
    {
        /// <summary>
        /// Reset the effect to default values.
        /// </summary>
        Renew,
        /// <summary>
        /// Stack StatModifier values without modifying duration.
        /// </summary>
        Stack,
        /// <summary>
        /// Stack StatModifier values and renew duration.
        /// </summary>
        RenewAndStack,
        /// <summary>
        /// Apply another separate instance of the effect.
        /// </summary>
        Apply,
        /// <summary>
        /// Increase effect duration.
        /// </summary>
        AddDuration,
        /// <summary>
        /// Do nothing.
        /// </summary>
        Skip,

    }

    public enum ExpiryMode
    {
        None,
        Time,
        Update,
        FixedUpdate
    }
    [CreateAssetMenu(menuName = "TDC/Affectables/Effect")]
    public class Effect : ScriptableObject
    {
        public string Name;
        public ReapplicationMode ReapplicationMode;
        public ExpiryMode ExpireMode;
        public float ExpireTime;
        public StatModifier[] Modifiers;
        public float ImmunityTimeOnExpire;
    }
}