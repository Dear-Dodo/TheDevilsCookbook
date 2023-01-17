using TDC.UI.HUD.Ordering;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TDC
{
    public class OrderTooltipProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private OrderSlotWidget _AssignedSlot;

        public void Awake()
        {
            _AssignedSlot = this.GetComponent<OrderSlotWidget>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _AssignedSlot.OrderBar._OrderTooltip.gameObject.SetActive(true);
            _AssignedSlot.OrderBar._OrderTooltip.Create(_AssignedSlot.AssignedOrder);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _AssignedSlot.OrderBar._OrderTooltip.gameObject.SetActive(false);
            _AssignedSlot.OrderBar._OrderTooltip.Destroy();
        }
    }
}