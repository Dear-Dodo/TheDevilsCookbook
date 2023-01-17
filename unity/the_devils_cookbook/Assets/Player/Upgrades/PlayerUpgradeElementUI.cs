using System.Collections.Generic;
using TDC.Spellcasting;
using TDC.UI.Generic;
using TDC.UI.HUD.Spellcasting.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.Player.Upgrades
{
    public class PlayerUpgradeElementUI : MonoBehaviour
    {
        [SerializeField] public TMPro.TextMeshProUGUI UpgradeName;
        [SerializeField] public TMPro.TextMeshProUGUI UpgradePrice;
        [SerializeField] public Image UpgradeIcon;
        [SerializeField] public HorizontalLayoutGroup TierLayoutGroup;
        [SerializeField] public Button TierButtonTemplate;
        [SerializeField] public TooltipProxyBase TooltipPrefab;
        [SerializeField] public SpellTooltipProvider IconTooltipPrefab;

        private List<Button> _TierButtons = new List<Button>();

        public void Initialize<T>(PlayerUpgrade<T> upgrade, PlayerStats stats)
        {
            for (var index = 0; index < upgrade.Tiers.Count; index++)
            {
                Button copy = Instantiate(TierButtonTemplate, Vector3.zero, Quaternion.identity);

                copy.gameObject.SetActive(true);
                copy.onClick.AddListener(() =>
                {
                    upgrade.Upgrade(stats);
                    UpdateTierState(upgrade);
                });
                copy.transform.SetParent(TierLayoutGroup.transform);
                copy.transform.localScale = new Vector3(1, 1, 1);
                IconTooltipPrefab.BuildSpellCache(upgrade.GetCurrentTier().Item as Spell);
                ShopTooltipProvider tooltipProvider = copy.gameObject.AddComponent<ShopTooltipProvider>();
                tooltipProvider.TooltipPrefab = TooltipPrefab;
                tooltipProvider.Tier = index;
                tooltipProvider.Cost = upgrade.GetTier(index).Cost;
                tooltipProvider.Description = upgrade.GetTier(index).Description;

                _TierButtons.Add(copy);
            }
            UpdateTierState(upgrade);
            UpgradeName.text = upgrade.Name;
            UpgradeIcon.sprite = upgrade.UpgradeSprite;
        }

        private void UpdateTierState<T>(PlayerUpgrade<T> upgrade)
        {
            for (var index = 0; index < _TierButtons.Count; index++)
            {
                var tier = _TierButtons[index];
                if (index <= upgrade.CurrentTier)
                {
                    ColorBlock colours = tier.colors;
                    colours.disabledColor = new Color(1, 0, 0.5f, 1);
                    tier.colors = colours;
                }
                tier.interactable = index == upgrade.CurrentTier + 1;
                // Debug.Log($"{index}, {upgrade.CurrentTier}");
            }
        }
    }
}