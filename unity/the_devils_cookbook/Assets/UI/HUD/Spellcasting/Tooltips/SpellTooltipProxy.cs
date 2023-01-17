using TDC.Core.Utility;
using TDC.UI.Generic;
using TMPro;

namespace TDC.UI.HUD.Spellcasting.Tooltips
{
    public class SpellTooltipProxy : TooltipProxyBase
    {
        [SerializedValueRequired]
        public TextMeshProUGUI SpellName;
        [SerializedValueRequired] 
        public TextMeshProUGUI SpellDescription;
        [SerializedValueRequired]
        public TextMeshProUGUI SpellStats;
        [SerializedValueRequired]
        public TextMeshProUGUI SpellCooldown;

    }
}
