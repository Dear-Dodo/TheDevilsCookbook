using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac;
using TDC.Core.Manager;
using TDC.Ingredient;
using TDC.Items;
using TDC.Spellcasting.Selectors;
using UnityAsync;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace TDC.Spellcasting.Spells
{
    [CreateAssetMenu(menuName = "TDC/Spells/Trap")]
    public class Trap : Spell
    {
        [FormerlySerializedAs("BaseDelay")]
        [SerializeField, Tooltip("Time after cast before trap will catch ingredients."),
        SpellStat("Delay")]
        private float _BaseDelay;

        [FormerlySerializedAs("BaseDuration")]
        [SerializeField, Tooltip("At 0, this is an instantaneous effect."),
        SpellStat("Duration")]
        private float _BaseDuration = 0;

        [SerializeField, FormerlySerializedAs("BaseRange"), SpellStat("Radius")]
        private float _BaseRange;

        [SerializeField, FormerlySerializedAs("BaseCastRange"), SpellStat("Cast Range")]
        private float _BaseCastRange = 7.5f;
        
        [SerializeField, Tooltip("Rate at which the trap is polled for ingredients in seconds. 0 = every frame")]
        private float _PollTime = 0;

        [SerializeField, Tooltip("Refunds the spell cast if no ingredients are caught.")]
        private bool _RefundCastOnWhiff;

        [Header("Selector VFX variables")]
        [SerializeField, GradientUsage(true)] private Gradient _VFXColour;

        [SerializeField] private AnimationCurve _VFXSizeOverLife;
        [SerializeField] private float _VFXSpawnRate;

        [Header("Cast VFX variables")]
        [SerializeField] private AnimationCurve _VFXCastSizeOverLife;

        [SerializeField] private float _VFXCastLifetime;
        [SerializeField] private GameObject _HolePrefab;

        [SerializeField] private int _Catch2bonus = 2;
        [SerializeField] private int _Catch3bonus = 5;
        [SerializeField] private int _Catch4bonus = 10;

        public override async Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token)
        {
            Vector3 target = targets.First().Position;
            var castEffect = new GameObject("Trap_CastVFX")
            {
                transform =
                {
                    position = target + new Vector3(0,0.01f,0)
                }
            };
            var vfxComponent = castEffect.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = CastGraphics.VFX;
            vfxComponent.SetAnimationCurve("Size Over Life", _VFXCastSizeOverLife);
            vfxComponent.SetFloat("Size", _BaseRange);
            vfxComponent.SetFloat("Lifetime", _BaseDuration == 0 ? _VFXCastLifetime : _BaseDuration);
            castEffect.layer = 8;

            CreateHole(castEffect);

            EventInstance grabSound = RuntimeManager.CreateInstance(CastSound);
            grabSound.set3DAttributes(RuntimeUtils.To3DAttributes(target));
            grabSound.start();
            grabSound.release();

            if (_BaseDelay != 0) await Await.Seconds(_BaseDelay);
            int amountCaught = await CatchIngredients(caster, target, onTick);
            switch (amountCaught)
            {
                case 2:
                    GameManager.CurrentLevelData.CatchBonus += 200;
                    (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value += _Catch2bonus;
                    break;

                case 3:
                    GameManager.CurrentLevelData.CatchBonus += 500;
                    (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value += _Catch3bonus;
                    break;

                default:
                    if (amountCaught >= 4)
                    {
                        GameManager.CurrentLevelData.CatchBonus += 1000;
                        (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value += _Catch4bonus;
                    }
                    break;
            }

            Destroy(vfxComponent.gameObject, 5);
            if (_RefundCastOnWhiff && amountCaught == 0 && caster.TryGetSpellByType<Trap>(out SpellData spell))
                spell.AddCharges(1);
        }

        protected override Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token)
        {
            var selector = Instantiate(SelectorPrefab).GetComponent<PointSelector>();
            selector.SetGraphics(SelectorGraphics);

            selector.Settings.MaxCastRange = _BaseCastRange;

            Transform selectorTransform = selector.transform;
            selectorTransform.localScale =
                new Vector3(_BaseRange * 2, _BaseRange * 2, selectorTransform.localScale.z);

            selector.VisualEffect.SetFloat("Size", _BaseRange);
            selector.VisualEffect.SetGradient("Colour", _VFXColour);
            selector.VisualEffect.SetAnimationCurve("Size Over Life", _VFXSizeOverLife);
            selector.VisualEffect.SetFloat("Spawn Rate", _VFXSpawnRate);

            selector.OnValidityChanged += OnValidityChanged;

            return selector.StartSelection(caster.transform, token);
        }

        private void OnValidityChanged(PointSelector.ValidityChangeData data, PointSelector selector)
        {
            data.Selector.Renderer.material.SetColor(Selector.ColourID, data.IsValid ? SelectorGraphics.DecalColour : SelectorGraphics.InvalidDecalColor);
        }

        private async Task<int> CatchIngredients(Spellcaster caster, Vector3 target, Action<IEnumerable<Agent>> onTick)
        {
            var casterInventory = caster.GetComponent<Inventory>();
            var colliders = new Collider[64];
            float startTime = Time.time;
            var isRepeat = false;

            var caught = 0;

            do
            {
                if (isRepeat)
                {
                    if (_PollTime == 0) await Await.NextFixedUpdate();
                    else await Await.Seconds(_PollTime);
                }
                isRepeat = true;

                int colliderCount = Physics.OverlapCapsuleNonAlloc(target + Vector3.down, target + Vector3.up,
                    _BaseRange, colliders);
                for (var i = 0; i < colliderCount; i++)
                {
                    Collider collider = colliders[i];
                    if (!collider.TryGetComponent(out Creature creature)) continue;
                    if (!casterInventory.HasSpaceForType(creature.PeekData().StorageTypes, 1)) continue;

                    Item ingredientItem = creature.Catch();
                    ingredientItem.Pickup(caster.transform);
                    casterInventory.DepositItems(new Dictionary<StorableObject, int> { { ingredientItem.Data, 1 } }, ingredientItem.Data.StorageTypes);
                    caught++;
                }
            } while (Time.time - startTime < _BaseDuration);

            return caught;
        }

        private async void CreateHole(GameObject parent)
        {
            GameObject hole = Instantiate(_HolePrefab, parent.transform);
            hole.transform.localScale = Vector3.zero;
            await hole.transform.DOScale(Vector3.one, 0.5f).AsyncWaitForCompletion();
            await Await.Seconds(1);
            await hole.transform.DOScale(Vector3.zero, 0.5f).AsyncWaitForCompletion();
            Destroy(hole);
        }
    }
}