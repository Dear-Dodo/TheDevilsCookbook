using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.Affectables;
using TDC.AIRefac;
using TDC.Spellcasting.Selectors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Silence")]
    public class Silence : Spell
    {
        [SerializeField] private VisualEffectAsset _SilencedEffect;

        [FormerlySerializedAs("_BaseRange")] [SerializeField, SpellStat("Cast Range")]
        private float _BaseCastRange = 3.5f;

        [SerializeField, SpellStat("Radius")]
        private float _Radius = 2.5f;

        [SerializeField, SpellStat("Silence Duration")]
        private float _SilenceDuration = 3;

        public override Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            Vector3 target = targets[0].Position;
            var castEffect = new GameObject("Silence_CastVFX")
            {
                transform =
                {
                    position = target + new Vector3(0,0.01f,0)
                }
            };
            var vfxComponent = castEffect.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = CastGraphics.VFX;
            vfxComponent.SetFloat("Radius", _Radius * 2);
            vfxComponent.gameObject.layer = 8;

            var effect = CreateInstance<Effect>();
            effect.Name = "Silence";
            effect.ExpireMode = ExpiryMode.Time;
            effect.ExpireTime = _SilenceDuration;
            effect.ReapplicationMode = ReapplicationMode.Renew;
            effect.Modifiers = new[]
            {
                new StatModifier()
                {
                    StatName = "Silenced",
                    Operation = StatOperation.Set,
                    StackValue = 1,
                    Value = 1,
                    ValueStackMode = StackMode.None,
                    AppliedVFX = _SilencedEffect
                },
                new StatModifier()
                {
                    StatName = "Cast Speed",
                    Operation = StatOperation.Set,
                    StackValue = 0,
                    Value = 0,
                    ValueStackMode = StackMode.None
                }
            };

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            grabSound.set3DAttributes(RuntimeUtils.To3DAttributes(target));
            grabSound.start();
            grabSound.release();

            var agents = new Dictionary<Collider, Agent>();
            foreach (Collider c in Physics.OverlapCapsule(target + Vector3.down, target + Vector3.up, _Radius))
            {
                if (!agents.TryGetValue(c, out Agent agent))
                {
                    if (!c.TryGetComponent(out agent)) continue;
                    agents.Add(c, agent);
                }

                agent.Stats.AddEffect(effect);
            }
            onTick?.Invoke(agents.Values);

            Destroy(castEffect, 5f);
            return Task.CompletedTask;
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<PointSelector>();

            selector.Settings.MaxCastRange = _BaseCastRange;

            selector.SetGraphics(SelectorGraphics);
            selector.Settings.ShouldDestroyImmediately = true;

            selector.OnValidityChanged += OnValidityChanged;

            return selector.StartSelection(caster.transform, token);
        }

        private void OnValidityChanged(PointSelector.ValidityChangeData data, PointSelector selector)
        {
            if (!selector)
            {
                return;
            }

            if (!selector.VisualEffect)
            {
                return;
            }

            selector.VisualEffect.SetVector4("Color", data.IsValid ? SelectorGraphics.DecalColour : SelectorGraphics.InvalidDecalColor);
        }
    }
}