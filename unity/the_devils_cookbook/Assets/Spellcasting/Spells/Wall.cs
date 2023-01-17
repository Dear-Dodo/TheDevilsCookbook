using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac;
using TDC.Spellcasting.Selectors;
using UnityAsync;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Wall")]
    public class Wall : Spell
    {
        [SpellStat("Targets")]
        public int BaseMaxTargetPoints = 2;
        [SpellStat("Wall Distance")]
        public float BaseMaxDistance = 5.0f;
        [SpellStat("Duration")]
        public float BaseDuration = 5.0f;
        [SerializeField, FormerlySerializedAs("BaseCastRange"), SpellStat("Cast Range")]
        private float _BaseCastRange;

        [Header("Select VFX Options")] 
        [SerializeField] private float _SelectWallHeight = 1.0f;

        [SerializeField, ColorUsage(true, true)]
        private Color _WallColour;

        [SerializeField, ColorUsage(true, true)]
        private Color _PylonColour;
        [SerializeField, ColorUsage(true, true)]
        private Color _InvalidSelectColourMultiplier;
        
        [Header("Cast VFX Options")] 
        [SerializeField] private float _WallHeight = 2.0f;
        [SerializeField] private GameObject _WallPrefab;


        public override async Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            GameObject[] effects = CreateWalls(targets);
            EventInstance wallSound = RuntimeManager.CreateInstance(CastSound);
            wallSound.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.Lerp(targets[0].Position, targets[1].Position,0.5f)));
            wallSound.start();

            await Await.Seconds(BaseDuration);

            wallSound.release();
            foreach (GameObject effect in effects)
            {
                effect.transform.Find("Cracks").transform.DOScaleZ(0, 0.25f);
                Destroy(effect.transform.Find("Fire").gameObject);
                Destroy(effect, 0.25f);
            }
        }

        private void OnSelectorValidityChange(LineSelector.ValidityChangeData data)
        {
            data.SelectorVFX.SetVector4("Pylon Colour",
                data.IsValid ? _PylonColour : _PylonColour * _InvalidSelectColourMultiplier);
            data.SelectorVFX.SetVector4("Wall Colour",
                data.IsValid ? _WallColour : _WallColour * _InvalidSelectColourMultiplier);
            
            if (!data.SecondaryVFX.Any()) return;
            
            data.SecondaryVFX.Last().SetVector4("Wall Colour",
                !data.IsBlocked ? _WallColour : _WallColour * _InvalidSelectColourMultiplier);
        }
        
        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<LineSelector>();
            selector.SetGraphics(SelectorGraphics);

            selector.Settings.MaxLineDistance = BaseMaxDistance;
            selector.Settings.MaxCastRange = _BaseCastRange;
            selector.Settings.TargetCount = BaseMaxTargetPoints;
            selector.Settings.ShouldDestroyImmediately = true;
            selector.VisualEffect.SetVector4("Pylon Colour", _PylonColour);
            
            selector.AddVFXDelegate(v=> v.SetFloat("Height", _SelectWallHeight), true);
            selector.AddVFXDelegate(v => v.SendEvent("SpawnWall"), false);
            selector.AddVFXDelegate(v => v.SendEvent("SpawnWall"), false);

            selector.ValidityChanged += OnSelectorValidityChange;

            return selector.StartSelection(caster.transform, token);
        }
        
        private GameObject[] CreateWalls(Target[] targets)
        {
            var effects = new GameObject[targets.Length - 1];
            for (var i = 0; i < targets.Length - 1; i++)
            {
                Vector3 start = targets[i].Position;
                Vector3 end = targets[i + 1].Position;
                Vector3 line = end - start;
                var vfxObject = Instantiate(_WallPrefab);
                vfxObject.transform.position = (start + end) / 2;
                vfxObject.transform.Find("Cracks").localScale = new Vector3(Mathf.Max(line.magnitude,1) * 0.18f, 1, 0);
                vfxObject.transform.Find("Cracks").DOScaleZ(1,0.1f);
                vfxObject.transform.Find("Fire").GetComponent<VisualEffect>().SetFloat("Scale", Mathf.Max(line.magnitude, 1) * 0.18f);
                vfxObject.transform.right = line;
                var obstacle = vfxObject.AddComponent<NavMeshObstacle>();
                var collider = vfxObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(Mathf.Max(line.magnitude * 0.9f, 1), _WallHeight, 0.1f);
                collider.center = new Vector3(0, _WallHeight / 2, 0);
                obstacle.carving = true;
                obstacle.size = new Vector3(0.1f, _WallHeight, 1);
                obstacle.center = new Vector3(Mathf.Max(line.magnitude * 0.9f, 1), _WallHeight, 0.1f);

                effects[i] = vfxObject;

            }
            return effects;
        }
    }
}
