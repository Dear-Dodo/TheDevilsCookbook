using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDC.Affectables;
using TDC.AIRefac;
using TDC.Core.Utility;
using TDC.Spellcasting.Selectors;
using UnityAsync;
using UnityEngine;
using UnityEngine.VFX;
using Utility;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Push")]
    public class Push : Spell
    {
        [SerializeField, SpellStat("Angle"), Tooltip("Angle of the cast cone.")]
        private float Angle = 30;

        [SerializeField, SpellStat("Range"), Tooltip("Distance from the caster to the edge of the cone.")]
        private float Range = 5;

        [SerializeField, SpellStat("Force"), Tooltip("Force applied to ingredients within the cone")]
        private float Force = 10;

        [SerializeField, SpellStat("Falloff Time"), Tooltip("Falloff duration of the push")]
        private float FalloffTime = 2;
        
        public override async Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            Vector3 castDirection = targets[0].Position;
            
            var castEffect = new GameObject("Push_CastVFX")
            {
                transform =
                {
                    position = caster.transform.position,
                    forward = castDirection
                }
            };
            var vfxComponent = castEffect.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = CastGraphics.VFX;
            vfxComponent.SetMesh("Sector Mesh", SectorMeshGenerator.BuildSquareUV2D(Range, Angle, 0.25f));
            vfxComponent.SetFloat("Sector Radians", Mathf.Deg2Rad * Angle);
            vfxComponent.SetInt("Smoke Count", Mathf.CeilToInt(Mathf.Lerp(0, 20, Angle / 90)));
            Destroy(castEffect, 5);

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            grabSound.set3DAttributes(RuntimeUtils.To3DAttributes(targets[0].Position));
            grabSound.start();
            grabSound.release();

            var agents = new List<(Agent agent, Vector2 direction)>();
            foreach (Collider collider in Physics.OverlapSphere(caster.transform.position, Range))
            {
                if (collider.TryGetComponent(out Agent agent) || collider.TryGetComponent<Projectile>(out _))
                {
                    Vector2 direction = (collider.transform.position.xz() - caster.transform.position.xz()).normalized;
                    float angle = Vector2.Angle(castDirection, direction);
                    if (angle > Angle) continue;
                    if (agent != null)
                    {
                        agents.Add((agent, direction));
                    } else if (collider.TryGetComponent(out Projectile projectile))
                    {
                        projectile.Deflected = true;
                        projectile.Initialize(projectile.transform.position, projectile.transform.position + direction.xoy() * Force * 10);
                    }
                }
            }

            onTick?.Invoke(agents.Select(a => a.agent));
            foreach ((Agent agent, Vector2 direction) in agents)
            {
                var effect = CreateInstance<Effect>();
                effect.Name = "Push Slow";
                effect.ExpireMode = ExpiryMode.Time;
                effect.ExpireTime = FalloffTime;
                effect.ReapplicationMode = ReapplicationMode.Renew;
                effect.Modifiers = new[]
                {
                    new StatModifier()
                    {
                        StatName = "Movement Speed",
                        Operation = StatOperation.Set,
                        StackValue = 0,
                        Value = 0,
                        ValueStackMode = StackMode.None
                    }
                };
                agent.Stats.AddEffect(effect);
            }
            float time = 0;
            while (time < FalloffTime)
            {
                foreach ((Agent agent, Vector2 direction) in agents)
                {
                    agent.MomentumController.AddForce(new Vector3(direction.x * Force, 0, direction.y * Force));
                }

                time += Time.fixedDeltaTime;
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate();
            }
            
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<ConeSelector>();
            selector.Settings.Angle = Angle;
            selector.Settings.Radius = Range;
            return selector.StartSelection(caster.transform, token);
        }
    }
}