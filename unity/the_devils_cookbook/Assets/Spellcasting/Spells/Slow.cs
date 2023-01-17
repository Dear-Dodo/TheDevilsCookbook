using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.Affectables;
using TDC.AIRefac;
using TDC.Spellcasting.Selectors;
using UnityAsync;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Slow")]
    public class Slow : Spell
    {
        [FormerlySerializedAs("_BaseRange")] [SerializeField, SpellStat("Cast Range")]
        private float _BaseCastRange = 10.0f;

        [SerializeField, SpellStat("Radius")]
        private float _Radius = 3;

        [SerializeField, SpellStat("Duration")]
        private float _Duration = 5;

        [SerializeField, SpellStat("Debuff Linger Time")]
        private float _DebuffLingerTime = 0;

        [SerializeField, SpellStat("Slow Factor")]
        private float _SlowFactor = 0.75f;

        public override async Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            Vector3 target = targets[0].Position;
            var castEffect = new GameObject("Slow_CastVFX")
            {
                transform =
                {
                    position = target + new Vector3(0,0.01f,0)
                }
            };
            var vfxComponent = castEffect.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = CastGraphics.VFX;
            vfxComponent.SetFloat("Radius", _Radius);
            vfxComponent.SetFloat("Duration", _Duration);

            var effect = CreateInstance<Effect>();
            effect.Name = "Push Slow";
            effect.ExpireMode = _DebuffLingerTime == 0 ? ExpiryMode.FixedUpdate : ExpiryMode.Time;
            effect.ExpireTime = _DebuffLingerTime == 0 ? 2 : _DebuffLingerTime;
            effect.ReapplicationMode = ReapplicationMode.Renew;
            effect.Modifiers = new[]
            {
                new StatModifier()
                {
                    StatName = "Movement Speed",
                    Operation = StatOperation.Multiplicative,
                    StackValue = 0,
                    Value = 1 - _SlowFactor,
                    ValueStackMode = StackMode.None
                }
            };

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            grabSound.set3DAttributes(RuntimeUtils.To3DAttributes(target));
            grabSound.start();
            grabSound.release();

            var agents = new Dictionary<Collider, Agent>();
            Agent agent;
            float elapsed = 0;

            do
            {
                foreach (Collider c in Physics.OverlapCapsule(target + Vector3.down, target + Vector3.up, _Radius))
                {
                    if (!agents.TryGetValue(c, out agent))
                    {
                        if (!c.TryGetComponent<Agent>(out agent)) continue;
                        agents.Add(c, agent);
                    }

                    agent.Stats.AddEffect(effect);
                }
                onTick?.Invoke(agents.Values);
                elapsed += Time.fixedDeltaTime;
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate();
            } while (elapsed < _Duration);

            Destroy(castEffect, 1f);
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<PointSelector>();

            selector.Settings.MaxCastRange = _BaseCastRange;

            Transform selectorTransform = selector.transform;
            selectorTransform.localScale =
                new Vector3(_Radius * 2, _Radius * 2, selectorTransform.localScale.z);
            selector.SetGraphics(SelectorGraphics);

            selector.OnValidityChanged += OnValidityChanged;

            return selector.StartSelection(caster.transform, token);
        }

        private void OnValidityChanged(PointSelector.ValidityChangeData data, PointSelector selector)
        {
            data.Selector.Renderer.material.SetColor(Selector.ColourID, data.IsValid ? SelectorGraphics.DecalColour : SelectorGraphics.InvalidDecalColor);
        }
    }
}