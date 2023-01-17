using Nito.AsyncEx;
using System.Collections.Generic;
using TDC.Core.Manager;
using TDC.Spellcasting;
using UnityEngine;

namespace TDC
{
    public class LoadoutManagerUI : MonoBehaviour
    {
        [SerializeField] public float SnappingRange;
        [SerializeField] public List<LoadoutSpellSlotUI> Slots;
        [SerializeField] public LoadoutSpellSlotUI[] ActiveSlots;
        [SerializeField] public LoadoutSpell[] Spells;

        private AsyncManualResetEvent _Started = new AsyncManualResetEvent();
        // Start is called before the first frame update
        async void Start()
        {
            for (int i = 0; i < ActiveSlots.Length; i++)
            {
                LoadoutSpellSlotUI slot = ActiveSlots[i];
                Spell targetSpell = (await GameManager.PlayerCharacter.GetPlayerStats()).ActiveSpells[i];
                foreach (LoadoutSpell spell in Spells)
                {
                    if (spell.Spell == targetSpell)
                    {
                        slot.Spell = spell;
                        spell.CurrentSlot = slot;
                    }
                }
            }
            foreach (LoadoutSpell spell in Spells)
            {
                bool spellActive = false;
                foreach (LoadoutSpellSlotUI slot in ActiveSlots)
                {
                    if (spell == slot.Spell)
                    {
                        spellActive = true;
                        break;
                    }
                }
                if (!spellActive)
                {
                    spell.CurrentSlot = spell.ParentSlot;
                    spell.ParentSlot.Spell = spell;
                }
            }
            _Started.Set();
        }

        // Update is called once per frame
        async void Update()
        {
            await _Started.WaitAsync();
            (await GameManager.PlayerCharacter.GetPlayerStats()).ActiveSpells.Clear();
            foreach (LoadoutSpellSlotUI slot in ActiveSlots)
            {
                if (slot.Spell != null)
                    (await GameManager.PlayerCharacter.GetPlayerStats()).ActiveSpells.Add(slot.Spell.Spell);
            }
        }
    }
}
