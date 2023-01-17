using TDC.Core.Utility;
using TDC.Ordering;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.PatronIndicator
{
    public class PatronOrderIndicator : MonoBehaviour
    {
        [SerializeField, SerializedValueRequired]
        private RectTransform _DynamicClockHand;

        [SerializeField, SerializedValueRequired]
        private Image _ClockFill;
        [SerializeField, SerializedValueRequired]
        private Image _InverseClockFill;

        [SerializeField, SerializedValueRequired]
        private Image _WindowIcon;
        [SerializeField, SerializedValueRequired]
        private Image _FoodIcon;

        private Order _Order;

        public void Initialise(Order order)
        {
            this.Validate();
            _Order = order;
            _WindowIcon.sprite = order.PatronWindow.WindowIdentificationSprite;
            _FoodIcon.sprite = order.Food.Output[0].Icon;
        }

        private void UpdateClock()
        {
            float elapsedFactor = _Order.ElapsedTime / _Order.Time;
            float remainingFactor = 1 - elapsedFactor;
            _ClockFill.fillAmount = remainingFactor;
            _InverseClockFill.fillAmount = elapsedFactor;
            const float emptyDegrees = -360;
            _DynamicClockHand.localRotation = Quaternion.Euler(0, 0, emptyDegrees * elapsedFactor);
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateClock();
        }
    }
}
