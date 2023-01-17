using TDC.UI.Generic.Overlay;
using UnityEngine;

namespace TDC
{
    public class ShopTooltipProvider : TooltipProviderBase
    {
        public int Tier;
        public int Cost;
        public string Description;


        private ShopTooltipProxy _ShopTooltipInstance;
        protected override void OnTooltipCreated()
        {
            _ShopTooltipInstance = TooltipInstance as ShopTooltipProxy;
            Cursor.visible = false;
        }

        protected override void OnTooltipEnabled()
        {
            _ShopTooltipInstance.Tier.SetText($"Tier {Tier}");
            _ShopTooltipInstance.Cost.SetText(Cost != 0 ? $"{Cost}<br>" : "FREE<br>");
            _ShopTooltipInstance.Info.SetText(Description);
            Cursor.visible = false;
        }

        protected override void OnTooltipDisabled()
        {
            Cursor.visible = true;
        }

    }
}
