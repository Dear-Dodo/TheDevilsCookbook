using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.AIRefac;
using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.VFX;
using Action = System.Action;

namespace TDC.Spellcasting
{
    public abstract class Spell : ScriptableObject
    {
        [Serializable]
        public struct SpellGraphics
        {
            [ColorUsage(true, true)]
            public Color DecalColour;

            public Color InvalidDecalColor;
            public Texture2D Decal;
            public VisualEffectAsset VFX;
        }

        public string DisplayName = "UNNAMED";

        [TextArea]
        public string Description = "UNSET DESCRIPTION";

        public float BaseCooldown = 1;
        public int BaseMaxCharges = 1;

        [SerializeField] protected Sprite _SpellIcon;
        public Sprite SpellIcon => _SpellIcon;

        [SerializeField] protected Selector SelectorPrefab;
        [SerializeField] protected SpellGraphics SelectorGraphics;
        [SerializeField] protected SpellGraphics CastGraphics;
        [SerializeField] protected EventReference CastSound;

        /// <summary>
        /// TryCast with existing target list.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="targets"></param>
        /// <param name="onTick"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract Task Cast(Spellcaster caster, Target[] targets, Action<IEnumerable<Agent>> onTick, CancellationToken token);

        /// <summary>
        /// TryCast, expecting a Selector to specify targets through user input.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="token"></param>
        /// <param name="onSpellCast">Callback after targets have been selected and spell is being cast.</param>
        /// <param name="onCancel">Callback on cancellation of cast.</param>
        /// <param name="onTick"></param>
        public async Task PlayerCast(Spellcaster caster, CancellationToken token,
            Action onSpellCast = null, Action onCancel = null, Action<IEnumerable<Agent>> onTick = null)
        {
            try
            {
                Target[] targets = await SelectTargets(caster, token);
                onSpellCast?.Invoke();
                await Cast(caster, targets, onTick, token);
            }
            catch (OperationCanceledException)
            {
                onCancel?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        protected abstract Task<Target[]> SelectTargets(Spellcaster caster, CancellationToken token);

        protected void Awake()
        {
            SerializedFieldValidation.Validate(GetType(), this, true);
        }
    }
}