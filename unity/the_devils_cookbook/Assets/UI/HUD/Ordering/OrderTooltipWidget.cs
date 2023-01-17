using System.Collections.Generic;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Ordering;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TDC.UI.HUD.Ordering
{
    public class OrderTooltipWidget : MonoBehaviour
    {
        [SerializeField, SerializedValueRequired]
        private TextMeshProUGUI _Label;

        [SerializeField, SerializedValueRequired]
        private Image _ImagePrefab;

        [SerializeField, SerializedValueRequired]
        private HorizontalLayoutGroup _LayoutGroup;

        internal Order AssignedOrder;

        private List<Image> _IngredientIcons;

        public void Update()
        {
            this.transform.position = Mouse.current.position.ReadValue();
        }

        public void Start()
        {
            GameManager.OrderManager.OnOrderDeleted += Destroy;
        }

        public void Create(Order order)
        {
            _IngredientIcons = new List<Image>();

            _Label.text = order.Food.name;
            foreach (var food in order.Food.Input)
            {
                Image image = Instantiate(_ImagePrefab.gameObject, _LayoutGroup.transform).GetComponent<Image>();
                image.sprite = food.Icon;
                _IngredientIcons.Add(image);
                image.gameObject.SetActive(true);
            }

            AssignedOrder = order;
        }

        public void Destroy(Order order)
        {
            if (AssignedOrder == order)
            {
                Destroy();
                gameObject.SetActive(false);
            }
        }

        public void Destroy()
        {
            foreach (var icon in _IngredientIcons)
            {
                Destroy(icon.gameObject);
            }

            _IngredientIcons.Clear();
        }
    }
}