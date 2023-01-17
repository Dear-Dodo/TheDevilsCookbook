using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TDC.Affectables;
using TDC.Core.Manager;
using TDC.Player;
using TDC.Player.Upgrades;
using UnityEngine;

namespace TDC.Spellcasting
{
    public class Spellcaster : MonoBehaviour
    {
        [SerializeField]
        private List<SpellData> _Spells;
        public SpellData[] Spells => _Spells.ToArray();

        public event Action<SpellData[]> SpellListChanged;
        public event Action<Spell> SpellCast;
        
        private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

        private AffectableStats _Stats;
        
        public bool IsSelectingForCast { get; private set; }

        public void ClearSpells()
        {
            _Spells.Clear();
            SpellListChanged?.Invoke(Spells);
        }

        public void AddSpell(Spell spell)
        {
            var data = new SpellData(spell);
            _Spells.Add(data);
            data.Reset();
            SpellListChanged?.Invoke(Spells);
        }

        public void SetSpell(Spell spell, int index)
        {
            for (int i = _Spells.Count; i <= index; i++)
            {
                _Spells.Add(null);
            }

            _Spells[index] = new SpellData(spell);
            _Spells[index].Reset();
            SpellListChanged?.Invoke(Spells);
        }
        
        public SpellData GetSpellByType<T>() where T : Spell
        {
            return Spells.First(s => s.Spell is T);
        }

        public bool TryGetSpellByType<T>(out SpellData data) where T : Spell
        {
            try
            {
                data = Spells.First(s => s.Spell is T);
                return true;
            }
            catch (InvalidOperationException e)
            {
                data = null;
                return false;
            }
        }
        
        public bool TryCast(int index)
        {
            if (_Stats.ModifiedStats["Stunned"] > 0) return false;
            CancelCast();
            if (_Spells.Count <= index)
            {
                Debug.LogError($"'{gameObject.name}' attempted to cast spell at index '{index}' but only has '{_Spells.Count}' spells.");
                return false;
            }

            if (!_Spells[index].IsValidCast(true)) return false;

            IsSelectingForCast = true;
            CancellationToken castToken = _CancellationTokenSource.Token;
            _ = _Spells[index].Spell.PlayerCast(this, castToken, () =>
            {
                IsSelectingForCast = false;
                _Spells[index].RegisterCast();
                SpellCast?.Invoke(_Spells[index].Spell);
            }, () => IsSelectingForCast = false, _Spells[index].RegisterSpellTick);
            
            return true;
        }

        public void CancelCast()
        {
            _CancellationTokenSource.Cancel();
            _CancellationTokenSource = new CancellationTokenSource();
        }

        private async void Awake()
        {
            await GameManager.LevelInitialisedAsync.WaitAsync();
            for (int i = 0; i < _Spells.Count; i++)
            {
                SpellData spell = _Spells[i];
                PlayerStats stats = await GameManager.PlayerCharacter.GetPlayerStats();
                PlayerUpgrade<Spell> spellType = stats.SpellUpgrades[stats.ActiveSpells[i].GetType()];
                spell.Spell = spellType.Tiers[spellType.CurrentTier].Item;
                spell.Reset();
            }

            _Stats = GetComponent<AffectableStats>();
        }

        private void Update()
        {
            foreach (SpellData spell in _Spells)
            {
                spell.ProgressCooldown();
            }
        }
    }
}