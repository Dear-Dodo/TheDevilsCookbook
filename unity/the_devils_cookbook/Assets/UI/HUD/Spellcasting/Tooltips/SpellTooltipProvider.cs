using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TDC.Spellcasting;
using TDC.UI.Generic.Overlay;

namespace TDC.UI.HUD.Spellcasting.Tooltips
{
    public class SpellTooltipProvider : TooltipProviderBase
    {
        private struct SpellStatData
        {
            public FieldInfo Field;
            public SpellStatAttribute StatInfo;
        }
        
        private SpellTooltipProxy _SpellTooltipInstance;
        
        private List<SpellStatData> _CachedSpellStats;
        private Spell _CachedSpell;

        /// <summary>
        /// Rebuilds the stat cache for the specified spell.
        /// </summary>
        /// <param name="spell"></param>
        public void BuildSpellCache(Spell spell)
        {
            FieldInfo[] statFields = spell.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public
                | BindingFlags.Instance)
                .Where(f => f.IsDefined(typeof(SpellStatAttribute), true)).ToArray();
            _CachedSpellStats = new List<SpellStatData>(statFields.Length);
            
            foreach (FieldInfo statField in statFields)
            {
                _CachedSpellStats.Add(new SpellStatData()
                {
                    Field = statField,
                    StatInfo = statField.GetCustomAttribute<SpellStatAttribute>()
                });
            }

            _CachedSpell = spell;
        }
        
        protected override void OnTooltipCreated()
        {
            _SpellTooltipInstance = TooltipInstance as SpellTooltipProxy;
        }

        protected override void OnTooltipEnabled()
        {
            _SpellTooltipInstance.SpellStats.text = GenerateStatText();
            _SpellTooltipInstance.SpellName.text = _CachedSpell.DisplayName;
            _SpellTooltipInstance.SpellDescription.text = _CachedSpell.Description;
            _SpellTooltipInstance.SpellCooldown.text = $"{_CachedSpell.BaseCooldown:F1}s";
        }

        protected override void OnTooltipDisabled()
        {
            
        }

        private string GenerateStatText()
        {
            var output = new StringBuilder(_CachedSpellStats.Count * 20);

            foreach (SpellStatData stat in _CachedSpellStats)
            {
                output.AppendLine($"{stat.StatInfo.Name}: {stat.Field.GetValue(_CachedSpell)}");
            }

            return output.ToString();
        }
    }
}
