using TDC.UI.Generic;
using TMPro;
using UnityEngine;

namespace TDC
{
    public class ShopTooltipProxy : TooltipProxyBase
    {
        [SerializeField] public TextMeshProUGUI Tier;
        [SerializeField] public TextMeshProUGUI Cost;
        [SerializeField] public TextMeshProUGUI Info;
    }
}
