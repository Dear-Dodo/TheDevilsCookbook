using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Spellcasting;
using TDC.UI.HUD.Spellcasting.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.HUD.Spellcasting
{
    public class SpellSlotWidget : MonoBehaviour
    {
        [SerializeField, SerializedValueRequired] private TextMeshProUGUI _KeyText;
        [SerializeField, SerializedValueRequired] private TextMeshProUGUI _ChargeText;

        [SerializeField, SerializedValueRequired]
        private GameObject _ChargeBackground;

        [SerializeField, SerializedValueRequired]
        private Image _ChargeCooldown;
        [SerializeField, SerializedValueRequired] private TextMeshProUGUI _CooldownText;
        [SerializeField, SerializedValueRequired] private Image _CooldownOverlay;
        [SerializeField, SerializedValueRequired] private Image _SpellIcon;
        
        private SpellData _AssignedSpell;
        private int _SpellIndex;

        public void AssignSpell(SpellData spell, int spellIndex)
        {
            if (spell == _AssignedSpell) return;
            
            DeregisterSpell();

            _AssignedSpell = spell;
            _SpellIndex = spellIndex;
            _SpellIcon.sprite = _AssignedSpell.Spell.SpellIcon;
            _KeyText.text = GameManager.PlayerCharacter.GetSpellInputAction(spellIndex)?.controls[0].name.ToUpper() ?? "";
            _AssignedSpell.CastAttempted += OnCastAttempt;
            _AssignedSpell.CastStop += OnSpellSelectEnd;
            
            GetComponent<SpellTooltipProvider>()?.BuildSpellCache(_AssignedSpell.Spell);
        }

        private void DeregisterSpell()
        {
            if (_AssignedSpell == null) return;
            _AssignedSpell.CastAttempted -= OnCastAttempt;
            _AssignedSpell.CastStop -= OnSpellSelectEnd;
        }

        private void OnDestroy()
        {
            DeregisterSpell();
        }

        private void Awake()
        {
            SerializedFieldValidation.Validate(GetType(), this, true);
        }

        private void UpdateCharges()
        {
            if (_AssignedSpell.Charges < 2 && _AssignedSpell.Spell.BaseMaxCharges < 2)
            {
                _ChargeBackground.SetActive(false);
                return;
            }
            _ChargeText.text = _AssignedSpell.Charges.ToString();
            float cooldownPercent = _AssignedSpell.IsCoolingDown
                ? 1 - _AssignedSpell.Cooldown / _AssignedSpell.Spell.BaseCooldown
                : 1;
            _ChargeCooldown.fillAmount = cooldownPercent;
        }

        private void UpdateCooldown()
        {
            bool showCooldown = _AssignedSpell.Cooldown > 0 && _AssignedSpell.Charges == 0;
            if (showCooldown)
            {
                if (!_CooldownOverlay.gameObject.activeSelf) _CooldownOverlay.gameObject.SetActive(true);
                _CooldownText.text = _AssignedSpell.Cooldown < 10
                    ? $"{_AssignedSpell.Cooldown:0.0}"
                    : $"{_AssignedSpell.Cooldown:0}";
                _CooldownOverlay.fillAmount =
                    Mathf.Clamp01(_AssignedSpell.Cooldown / _AssignedSpell.Spell.BaseCooldown);
            }
            else if (_CooldownOverlay.gameObject.activeSelf) _CooldownOverlay.gameObject.SetActive(false);
        }
        private void Update()
        {
            UpdateCharges();
            UpdateCooldown();
        }

        private void OnCastAttempt(bool isSuccess)
        {
            if (isSuccess) OnSpellSelect();
            else OnSpellFail();
        }
        
        /// <summary>
        /// Listener for beginning of successful target selection start.
        /// </summary>
        private void OnSpellSelect()
        {

        }

        /// <summary>
        /// Listener for end of target selection (success or cancelled).
        /// </summary>
        private void OnSpellSelectEnd()
        {

        }
        
        /// <summary>
        /// Listener for failed attempt to cast spell.
        /// </summary>
        private void OnSpellFail()
        {

        }
        
    }
}
