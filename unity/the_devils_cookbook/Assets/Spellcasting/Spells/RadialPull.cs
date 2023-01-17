using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac;
using FMOD.Studio;
using FMODUnity;
using TDC.Affectables;
using UnityAsync;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Utility;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/RadialPull")]
    public class RadialPull : Spell
    {
        [SerializeField, SpellStat("Duration")]
        private float _BaseDuration = 1;
        [SerializeField, Tooltip("Velocity towards cast point. Moved per second."), SpellStat("Pull Force")]
        private float _CentripetalVelocity = 1;

        [SerializeField, Tooltip("Velocity tangential to cast point. Moved per second."), SpellStat("Spin Force")]
        private float _TangentialVelocity = 1;

        [SerializeField, Tooltip("Whether the tangent is clockwise or counter clockwise (looking down).")] 
        private bool _IsClockwise = true;

        [SerializeField, FormerlySerializedAs("BaseRange")] private float _BaseRange;
        [SerializeField] private GameObject _CentreVFX;
        
        
        public override async Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            var activeAgents = new Dictionary<GameObject, Agent>(32);
            Transform casterTransform = caster.transform;
            
            var castEffect = new GameObject("RadialPull_CastVFX")
            {
                transform =
                {
                    position = casterTransform.position + new Vector3(0,0.01f,0)
                }
            };
            Transform castEffectTransform = castEffect.transform;
            var vfxComponent = castEffect.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = CastGraphics.VFX;
            vfxComponent.SetVector3("CrossVector", _IsClockwise ? Vector3.down : Vector3.up);
            vfxComponent.SetFloat("Radius", _BaseRange);
            vfxComponent.SetFloat("TangentialSpeed", _TangentialVelocity);
            vfxComponent.SetFloat("CentripetalSpeed", _CentripetalVelocity);
            vfxComponent.SetFloat("Duration", _BaseDuration);

            var castCenterEffect = Instantiate(_CentreVFX, castEffectTransform);
            var castCenterVFX = castCenterEffect.GetComponent<VisualEffect>();
            castCenterVFX.SetFloat("Duration", _BaseDuration);

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            RuntimeManager.AttachInstanceToGameObject(grabSound, casterTransform);
            grabSound.start();

            float startTime = Time.time;
            var isRepeat = false;

            var effect = CreateInstance<Effect>();
            effect.Name = "Pull Slow";
            effect.ExpireMode = ExpiryMode.FixedUpdate;
            effect.ExpireTime = 1;
            effect.Modifiers = new[]
            {
                new StatModifier()
                {
                    StatName = "Movement Speed",
                    Operation = StatOperation.Set,
                    StackValue = 0,
                    Value = 0,
                    ValueStackMode = StackMode.None
                },
                new StatModifier()
                {
                StatName = "Stunned",
                Operation = StatOperation.Set,
                StackValue = 0,
                Value = 1,
                ValueStackMode = StackMode.None
                }
            };
            
            do
            {
                if (isRepeat) await Await.NextFixedUpdate();
                isRepeat = true;

                Vector3 position = casterTransform.position;

                Collider[] colliders = Physics.OverlapCapsule(position + Vector3.down, position + Vector3.up, _BaseRange);
                
                float centripetalDelta = _CentripetalVelocity * Time.fixedDeltaTime;
                float tangentDelta = _TangentialVelocity * Time.fixedDeltaTime;
                Vector3 crossVector = _IsClockwise ? Vector3.down : Vector3.up;
                
                foreach (Collider collider in colliders)
                {
                    bool isActiveAgent = activeAgents.TryGetValue(collider.gameObject, out Agent agent);

                    if (!isActiveAgent && !collider.TryGetComponent(out agent)) continue;
                    
                    if (!isActiveAgent) activeAgents.Add(collider.gameObject, agent);
                    Vector3 displacement = agent.Transform.position - position;
                    Vector3 tangent = Vector3.Cross(displacement.xoz(), crossVector).normalized;
                    agent.Stats.AddEffect(effect);
                    agent.MomentumController.SetVelocity(Vector3.zero);
                    agent.MomentumController.ChangeSpeedOnce(-displacement.normalized * _CentripetalVelocity +
                                                      tangent * _TangentialVelocity);
                }
                
                if (Time.time - startTime >= _BaseDuration - 2f)
                {
                    vfxComponent.SendEvent("OnStop");
                }
                
                castEffectTransform.position = casterTransform.position + new Vector3(0, 0.01f, 0);
                onTick?.Invoke(activeAgents.Values);

            } while (Time.time - startTime < _BaseDuration);
            
            Destroy(castEffect);
            grabSound.release();
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            return Task.FromResult<Target[]>(null);
        }
    }
}
