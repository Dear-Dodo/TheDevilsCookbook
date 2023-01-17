using System;
using System.Collections.Generic;
using System.Linq;
using TDC.ThirdParty.SerializableDictionary;
using UnityEngine;
using UnityEngine.VFX;

namespace TDC.Affectables
{
    /// <summary>
    /// How and when modifier value is applied to its stat.
    /// </summary>
    public enum StatOperation
    {
        /// <summary>
        /// Set the value at the start.
        /// </summary>
        EarlySet,

        /// <summary>
        /// Add the value before multiplication.
        /// </summary>
        EarlyAdditive,

        /// <summary>
        /// Multiply the value.
        /// </summary>
        Multiplicative,

        /// <summary>
        /// Add the value after multiplication.
        /// </summary>
        Additive,

        /// <summary>
        /// Set the value after all modifiers.
        /// </summary>
        Set
    }

    public class AffectableStats : MonoBehaviour
    {
        private static readonly int _OperationCount = Enum.GetValues(typeof(StatOperation)).Length;

        private class EffectInstance
        {
            public readonly Effect Effect;
            public readonly StatModifier[] StatModifierInstances;
            public List<GameObject> VFXObjects;
            public int StackCount = 1;
            public readonly bool ForceNoExpiration;
            public float RemainingTime;

            public EffectInstance(Effect effect, bool forceNoExpiration = false)
            {
                Effect = effect;
                ForceNoExpiration = forceNoExpiration;
                RemainingTime = Effect.ExpireTime;
                StatModifierInstances = Effect.Modifiers.Select(m => (StatModifier)m.Clone()).ToArray();
                VFXObjects = new List<GameObject>();
            }
        }

        private class ImmunityInstance
        {
            public float RemainingTime;

            public ImmunityInstance(float remainingTime)
            {
                RemainingTime = remainingTime;
            }
        }

        private Dictionary<string, LinkedListNode<EffectInstance>> _EffectsByName =
            new Dictionary<string, LinkedListNode<EffectInstance>>();

        private LinkedList<EffectInstance> _Effects = new LinkedList<EffectInstance>();
        private Dictionary<string, ImmunityInstance> _Immunities = new Dictionary<string, ImmunityInstance>();

        [SerializeField]
        private SerializableDictionary<string, float> _BaseStats = new SerializableDictionary<string, float>()
        {
            { "Movement Speed", 5 },
            { "Cooldown Speed", 1 },
            { "Cast Speed", 1 },
            { "Status Duration", 1 },
            { "Slowdown Amount", 0},
            { "Silenced", 0 },  // 0 = false, 1 = true?
            { "Stunned", 0 }    // 0 = false, 1 = true?
        };


        public SerializableDictionary<string, float> BaseStats { get { return _BaseStats; }}
        public Dictionary<string, float> ModifiedStats { get; private set; }

        private List<StatModifier>[] _ModifiersToApply;

        public void ForceStatUpdate()
        {
            FixedUpdateStats(false);
        }
        
        public void AddEffect(Effect effect, bool forceNoExpire = false)
        {
            if (_Immunities.ContainsKey(effect.Name)) return;
            
            if (!_EffectsByName.TryGetValue(effect.Name, out LinkedListNode<EffectInstance> existingInstance))
            {
                var instance = new EffectInstance(effect, forceNoExpire);
                _EffectsByName.Add(effect.Name, _Effects.AddLast(instance));

                foreach (StatModifier modifier in effect.Modifiers)
                {
                    if (!modifier.AppliedVFX) continue;
                    var vfxObject = new GameObject($"VFX_Effect_{effect.Name}_{modifier.StatName}");
                    vfxObject.transform.SetParent(transform);
                    vfxObject.transform.localPosition = Vector3.zero;
                    vfxObject.layer = 8;
                    var vfxComponent = vfxObject.AddComponent<VisualEffect>();
                    vfxComponent.visualEffectAsset = modifier.AppliedVFX;
                    modifier.OnVFXSpawned?.Invoke(vfxComponent, this);
                    instance.VFXObjects.Add(vfxObject);
                }
                return;
            }

            switch (effect.ReapplicationMode)
            {
                case ReapplicationMode.Renew:
                    existingInstance.Value.RemainingTime = effect.ExpireTime;
                    break;

                case ReapplicationMode.Stack:
                    StackModifiers(existingInstance.Value);
                    break;

                case ReapplicationMode.RenewAndStack:
                    existingInstance.Value.RemainingTime = effect.ExpireTime;
                    StackModifiers(existingInstance.Value);
                    break;

                case ReapplicationMode.Apply:
                    throw new NotImplementedException();
                    break;

                case ReapplicationMode.AddDuration:
                    existingInstance.Value.RemainingTime += effect.ExpireTime;
                    break;

                case ReapplicationMode.Skip:
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveEffect(string effectName, bool activateImmunities)
        {
            if (!_EffectsByName.TryGetValue(effectName, out LinkedListNode<EffectInstance> instance)) return;
            RemoveEffect(instance, activateImmunities);
        }

        private void RemoveEffect(LinkedListNode<EffectInstance> effect, bool activateImmunities)
        {
            _EffectsByName.Remove(effect.Value.Effect.Name);
            _Effects.Remove(effect);
            foreach (GameObject obj in effect.Value.VFXObjects) Destroy(obj);

            if (!activateImmunities || !(effect.Value.Effect.ImmunityTimeOnExpire > 0)) return;
            
            float expireTime = effect.Value.Effect.ImmunityTimeOnExpire;
            if (_Immunities.TryGetValue(effect.Value.Effect.Name, out ImmunityInstance instance))
            {
                instance.RemainingTime = Mathf.Max(instance.RemainingTime, expireTime);
            }
            else
            {
                _Immunities.Add(effect.Value.Effect.Name, new ImmunityInstance(expireTime));
            }
        }

        /// <summary>
        /// Perform value stacking operation on an effect instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void StackModifiers(EffectInstance instance)
        {
            foreach (StatModifier current in instance.StatModifierInstances)
            {
                switch (current.ValueStackMode)
                {
                    case StackMode.None:
                        continue;
                    case StackMode.Additive:
                        current.Value += current.StackValue;
                        break;

                    case StackMode.AdditiveMinusOne:
                        current.Value += current.StackValue - 1;
                        break;

                    case StackMode.Multiplicative:
                        current.Value *= current.StackValue;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            instance.StackCount++;
        }

        private void Awake()
        {
            ModifiedStats = new Dictionary<string, float>(_BaseStats.Count);
            _ModifiersToApply = new List<StatModifier>[_OperationCount];
            for (var i = 0; i < _ModifiersToApply.Length; i++)
            {
                _ModifiersToApply[i] = new List<StatModifier>();
            }
            ResetStats();
        }

        private void Update()
        {
            ProgressEffectsUpdate();
        }

        private void FixedUpdate()
        {
            FixedUpdateStats();
        }

        private void ResetStats()
        {
            ModifiedStats.Clear();
            foreach (KeyValuePair<string, float> statPair in _BaseStats)
            {
                ModifiedStats.Add(statPair.Key, statPair.Value);
            }
        }

        private void ProgressImmunities()
        {
            var toRemove = new List<string>();
            foreach (KeyValuePair<string, ImmunityInstance> kvp in _Immunities)
            {
                float remaining = kvp.Value.RemainingTime - Time.fixedDeltaTime;
                if (remaining <= 0)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                kvp.Value.RemainingTime = remaining;
            }
            foreach (string s in toRemove)
            {
                _Immunities.Remove(s);
            }
        }
        
        /// <summary>
        /// Progress timed effects and remove expired effects.
        /// </summary>
        private void ProgressEffectsUpdate()
        {
            LinkedListNode<EffectInstance> current = _Effects.First;
            while (current != null)
            {
                bool expiresByUpdate = !current.Value.ForceNoExpiration 
                                       && current.Value.Effect.ExpireMode == ExpiryMode.Update;
                if (expiresByUpdate)
                {
                    current.Value.RemainingTime -= 1;
                    if (current.Value.RemainingTime <= 0) RemoveEffect(current, true);
                }
                current = current.Next;
            }
        }
        private void ProgressEffectsFixedUpdate()
        {
            LinkedListNode<EffectInstance> current = _Effects.First;
            while (current != null)
            {
                bool expiresByFixedUpdate = !current.Value.ForceNoExpiration 
                                       && current.Value.Effect.ExpireMode == ExpiryMode.FixedUpdate;
                bool expiresByTime = !current.Value.ForceNoExpiration
                                     && current.Value.Effect.ExpireMode == ExpiryMode.Time;
                if (expiresByTime)
                {
                    current.Value.RemainingTime -= Time.fixedDeltaTime;
                }
                else if (expiresByFixedUpdate)
                {
                    current.Value.RemainingTime -= 1;
                }

                if (expiresByTime || expiresByFixedUpdate)
                {
                    if (current.Value.RemainingTime <= 0) RemoveEffect(current, true);
                }
                current = current.Next;
            }
        }

        private void PrepareModifiersForProcessing()
        {
            foreach (StatModifier modifierInstance in _Effects.SelectMany(instance => instance.StatModifierInstances))
            {
                _ModifiersToApply[(int)modifierInstance.Operation].Add(modifierInstance);
            }
        }

        private Func<float, float, float> GetModifierDelegate(int operationIndex)
        {
            switch (operationIndex)
            {
                case (int)StatOperation.EarlySet:
                case (int)StatOperation.Set:
                    return (stat, mod) => mod;

                case (int)StatOperation.EarlyAdditive:
                case (int)StatOperation.Additive:
                    return (stat, mod) => stat + mod;

                case (int)StatOperation.Multiplicative:
                    return (stat, mod) => stat * mod;

                default:
                    throw new ArgumentOutOfRangeException($"OperationIndex {operationIndex} is out of range.");
            }
        }

        private void ProcessModifiers()
        {
            for (var i = 0; i < _ModifiersToApply.Length; i++)
            {
                Func<float, float, float> modifierDelegate = GetModifierDelegate(i);
                foreach (StatModifier statModifier in _ModifiersToApply[i])
                {
                    if (!ModifiedStats.TryGetValue(statModifier.StatName, out float statValue))
                    {
                        Debug.LogWarning(
                            $"AffectableStats {gameObject.name} did not have stat {statModifier.StatName}.");
                    }
                    ModifiedStats[statModifier.StatName] = modifierDelegate(statValue, statModifier.Value);
                }
            }
        }
        
        /// <summary>
        /// Recalculate ModifiedStats using the current EffectInstances.
        /// </summary>
        private void FixedUpdateStats(bool shouldProgress = true)
        {
            ResetStats();
            foreach (List<StatModifier> statModifiers in _ModifiersToApply)
            {
                statModifiers.Clear();
            }
            PrepareModifiersForProcessing();
            ProcessModifiers();
            
            if (!shouldProgress) return;
            ProgressEffectsFixedUpdate();
            ProgressImmunities();
        }
    }
}