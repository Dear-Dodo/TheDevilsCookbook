using System;
using System.Collections.Generic;
using TDC.AIRefac;
using UnityEngine;
using Action = System.Action;

namespace TDC.Spellcasting
{
    [Serializable]
    public class SpellData
    {
        public Spell Spell;
        public bool IsCoolingDown;
        public float Cooldown;
        public int Charges;
        public event Action StateChanged;
        public event Action<bool> CastAttempted;
        public event Action CastStop;

        public event Action<IEnumerable<Agent>> SpellTickPerformed;

        public bool IsValidCast(bool isCastAttempt)
        {
            bool isValid = Charges > 0;
            if (isCastAttempt) CastAttempted?.Invoke(isValid);
            return isValid;
        }

        public void RegisterSpellTick(IEnumerable<Agent> agents) => SpellTickPerformed?.Invoke(agents);
        
        public void RegisterCast()
        {
            Charges = Mathf.Max(Charges - 1, 0);
            if (Charges < Spell.BaseMaxCharges && !IsCoolingDown) RestartCooldown();
            CastStop?.Invoke();
        }

        public void ProgressCooldown()
        {
            if (!IsCoolingDown) return;
            Cooldown -= Time.deltaTime;
            
            if (Cooldown > 0) return;
            
            OnFinishCooldown();
        }

        /// <summary>
        /// Add `count` charges to the spell.
        /// </summary>
        /// <param name="count"></param>
        public void AddCharges(int count, bool clampToMax = true)
        {
            if (Charges >= Spell.BaseMaxCharges) return;
            Charges += count;
            if (Charges >= Spell.BaseMaxCharges) StopCooldown();
        }

        public void Reset()
        {
            Charges = Spell.BaseMaxCharges;
            Cooldown = 0;
        }
        public SpellData(Spell spell)
        {
            Spell = spell;
            Charges = Spell.BaseMaxCharges;
        }

        private void OnFinishCooldown()
        {
            AddCharges(1);
            if (Charges < Spell.BaseMaxCharges) RestartCooldown();
        }
        
        private void RestartCooldown()
        {
            IsCoolingDown = true;
            Cooldown = Spell.BaseCooldown;
        }

        private void StopCooldown()
        {
            IsCoolingDown = false;
        }
    }
}
