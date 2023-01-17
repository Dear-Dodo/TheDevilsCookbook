using System.Collections.Generic;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Spellcasting;
using UnityEngine;

namespace TDC.UI.HUD.Spellcasting
{
    public class SpellbarWidget : MonoBehaviour, IHideable
    {
        [SerializeField, SerializedValueRequired]
        private SpellSlotWidget _SlotPrefab;

        [SerializeField, SerializedValueRequired]
        private RectTransform _SpellSlotContainer;

        private readonly List<SpellSlotWidget> _SlotWidgets = new List<SpellSlotWidget>();

        private bool _IsHidden;
        
        private void RebuildSpellWidgets()
        {
            RebuildSpellWidgets(GameManager.PlayerCharacter.Spellcaster.Spells);
        }
        
        private void RebuildSpellWidgets(SpellData[] spells)
        {
            for (var i = 0; i < spells.Length; i++)
            {
                if (_SlotWidgets.Count <= i)
                {
                    _SlotWidgets.Add(Instantiate(_SlotPrefab, _SpellSlotContainer).GetComponent<SpellSlotWidget>());
                }
                _SlotWidgets[i].AssignSpell(spells[i], i);
            }
            
            DestoryExcessSlots(spells.Length);
            gameObject.SetActive(spells.Length != 0 && !_IsHidden);
        }

        private void DestoryExcessSlots(int targetCount)
        {
            if (_SlotWidgets.Count <= targetCount) return;

            for (int i = _SlotWidgets.Count - 1; i >= targetCount; i--)
            {
                Destroy(_SlotWidgets[i]);
                _SlotWidgets.RemoveAt(i);
            }
        }

        private void Awake()
        {
            SerializedFieldValidation.Validate(GetType(), this, true);
        }

        private async void Start()
        {
            await GameManager.LevelInitialisedAsync.WaitAsync();
            GameManager.PlayerCharacter.Spellcaster.SpellListChanged += RebuildSpellWidgets;
            RebuildSpellWidgets();
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
            _IsHidden = isHidden;
        }
    }
}
