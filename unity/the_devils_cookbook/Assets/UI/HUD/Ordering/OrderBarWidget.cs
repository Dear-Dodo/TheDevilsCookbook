using System.Collections.Generic;
using System.Linq;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Ordering;
using UnityEngine;

namespace TDC.UI.HUD.Ordering
{
    public class OrderBarWidget : MonoBehaviour, IHideable
    {
        [SerializeField, SerializedValueRequired]
        internal OrderTooltipWidget _OrderTooltip;

        [SerializeField, SerializedValueRequired]
        private OrderSlotWidget _SlotPrefab;

        [SerializeField, SerializedValueRequired]
        private RectTransform _OrderSlotContainer;

        private readonly List<OrderSlotWidget> _SlotWidgets = new List<OrderSlotWidget>();

        public async void Start()
        {
            await GameManager.InitialisedAsync.WaitAsync();
            GameManager.OrderManager.OnOrderCreated += CreateOrderWidget;
            GameManager.OrderManager.OnOrderDeleted += DeleteOrderWidget;
            await GameManager.LevelInitialisedAsync.WaitAsync();
        }

        private void DeleteOrderWidget(Order obj)
        {
            OrderSlotWidget widgetToRemove = _SlotWidgets.FirstOrDefault(slotWidget => slotWidget.AssignedOrder == obj);
            if (widgetToRemove != null)
            {
                Destroy(widgetToRemove.gameObject);
            }
        }

        private void CreateOrderWidget(Order order)
        {
            OrderSlotWidget widget = Instantiate(_SlotPrefab, _OrderSlotContainer).GetComponent<OrderSlotWidget>();
            widget.AssignOrder(order);
            widget.OrderBar = this;
            _SlotWidgets.Add(widget);
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}