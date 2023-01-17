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
using UnityEngine.VFX;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Stun")]
    public class Stun : Spell
    {
        [SerializeField] private VisualEffectAsset _BoltVFX;
        [SerializeField] private VisualEffectAsset _BlastVFX;
        [SerializeField] private VisualEffectAsset _StunnedEffect;

        [SerializeField, SpellStat("BaseRange")]
        private float _BaseRange = 3.5f;

        [SerializeField, SpellStat("Cast Range")]
        private float _BaseCastRange = 10.0f;
        
        [SerializeField, SpellStat("Blast Radius")]
        private float _BlastRadius = 2.5f;

        [SerializeField, SpellStat("Trigger Radius")]
        private float _TriggerRadius = 0.75f;

        [SerializeField, SpellStat("Stun Duration")]
        private float _StunDuration = 3;

        [SerializeField, SpellStat("Lifetime")]
        private float _Lifetime = 8;

        [SerializeField]
        private EventReference DetonateSound;

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
            vfxComponent.SetFloat("Blast Radius", _BlastRadius);
            vfxComponent.SetFloat("Trigger Radius", _TriggerRadius);

            var effect = CreateInstance<Effect>();
            effect.Name = "Stun";
            effect.ExpireMode = ExpiryMode.Time;
            effect.ExpireTime = _StunDuration;
            effect.ReapplicationMode = ReapplicationMode.Renew;
            effect.Modifiers = new[]
            {
                new StatModifier()
                {
                    StatName = "Stunned",
                    Operation = StatOperation.Set,
                    StackValue = 1,
                    Value = 1,
                    ValueStackMode = StackMode.None,
                    AppliedVFX = _StunnedEffect,
                    OnVFXSpawned = (vfx, stats) =>
                    {
                        vfx.SetMesh("AgentMesh",
                            stats.GetComponentInChildren<SkinnedMeshRenderer>()?.sharedMesh ??
                            stats.GetComponentInChildren<MeshFilter>().mesh);
                    }
                }
            };

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            grabSound.set3DAttributes(RuntimeUtils.To3DAttributes(target));
            grabSound.start();
            grabSound.release();

            float elapsed = 0;
            while (elapsed < _Lifetime)
            {
                if (Utilities.CapsuleCastForAgents(target, _TriggerRadius))
                {
                    DetonateTrap(target, effect, onTick);
                    break;
                }
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            Destroy(castEffect);
        }

        protected void DetonateTrap(Vector3 position, Effect effect, Action<IEnumerable<Agent>> onTick)
        {
            var agents = new List<Agent>();
            Utilities.PollCapsuleForAgents(ref agents, position, _BlastRadius);
            foreach (Agent agent in agents)
            {
                agent.Stats.AddEffect(effect);
                var boltVFXObject = new GameObject("VFX_Stun_Bolt")
                {
                    transform =
                    {
                        position = position
                    }
                };
                Destroy(boltVFXObject, 3.0f);
                var boltVFX = boltVFXObject.AddComponent<VisualEffect>();
                boltVFX.visualEffectAsset = _BoltVFX;
                boltVFX.SetVector3("StartPoint", position);
                boltVFX.SetVector3("EndPoint", agent.transform.position);
            }
            var blastVFXObject = new GameObject("VFX_Stun_Blast")
            {
                transform =
                    {
                        position = position
                    }
            };
            Destroy(blastVFXObject, 3.0f);
            var blastVFX = blastVFXObject.AddComponent<VisualEffect>();
            blastVFX.visualEffectAsset = _BlastVFX;
            EventInstance stunSound = RuntimeManager.CreateInstance(DetonateSound);
            stunSound.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            stunSound.start();
            stunSound.release();
            onTick?.Invoke(agents);
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<PointSelector>();

            selector.Settings.MaxCastRange = _BaseRange;

            Transform selectorTransform = selector.transform;
            selectorTransform.localScale =
                new Vector3(_BlastRadius * 2, _BlastRadius * 2, selectorTransform.localScale.z);
            selector.SetGraphics(SelectorGraphics);

            selector.OnValidityChanged += OnValidityChanged;

            return selector.StartSelection(caster.transform, token);
        }

        private void OnValidityChanged(PointSelector.ValidityChangeData data, PointSelector selector)
        {
            data.Selector.Renderer.material.SetColor(Selector.ColourID,
                data.IsValid ? SelectorGraphics.DecalColour : SelectorGraphics.InvalidDecalColor);
        }
    }
}