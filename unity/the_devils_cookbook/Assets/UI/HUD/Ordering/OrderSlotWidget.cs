using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TDC.Core.Utility;
using TDC.Ordering;
using UnityEngine;
using UnityEngine.UI;
using Math = TDC.Core.Utility.Math;

namespace TDC.UI.HUD.Ordering
{
    public class OrderSlotWidget : MonoBehaviour
    {
        [SerializeField, SerializedValueRequired]
        private Image _FoodImage;

        [SerializeField, SerializedValueRequired]
        private Image _BackgroundFill;

        [SerializeField, SerializedValueRequired]
        private Image _WindowIdentifierImage;

        [SerializeField, SerializedValueRequired]
        private Image _WindowIdentifierBackground;

        [SerializeField, SerializedValueRequired]
        private RectTransform _IngredientGrid;

        [SerializeField] private float _SpawnLerpTime = 0.5f;
        [SerializeField] private float _SpawnEaseAmplitude = 0.2f;
        [SerializeField] private float _SpawnEasePeriod = 0.1f;
        
        internal OrderBarWidget OrderBar;
        internal Order AssignedOrder;

        private List<Image> _IngredientImages = new List<Image>(4);
        private RectTransform _LerpContainerRectTransform;

        public void AssignOrder(Order order)
        {
            if (AssignedOrder != null || order == AssignedOrder)
            {
                // TODO: Log?
                return;
            }

            Deregister();

            AssignedOrder = order;

            Register();
        }

        public void UpdateOrderTimer(float current, float delta)
        {
            _BackgroundFill.fillAmount = 1 - Math.Percentage(current, AssignedOrder.Time);
        }

        private void Awake()
        {
            _LerpContainerRectTransform = transform.GetChild(0).GetComponent<RectTransform>();
            
            _LerpContainerRectTransform.anchorMin = new Vector2(_LerpContainerRectTransform.anchorMin.x, -1);
            _LerpContainerRectTransform.anchorMax = new Vector2(_LerpContainerRectTransform.anchorMax.x, 0);
        }

        public void Register()
        {
            if (AssignedOrder == null || !gameObject.active) return;

            _FoodImage.sprite = AssignedOrder.Food?.Output[0].Icon;
            _WindowIdentifierImage.sprite = AssignedOrder.PatronWindow?.WindowIdentificationSprite;
            SetIngredientIcons();

            AssignedOrder.OnTimerUpdated += UpdateOrderTimer;

            _LerpContainerRectTransform.DOAnchorMin(new Vector2(_LerpContainerRectTransform.anchorMin.x, 0),
                _SpawnLerpTime).SetEase(Ease.OutElastic, _SpawnEaseAmplitude, _SpawnEasePeriod);
            _LerpContainerRectTransform.DOAnchorMax(new Vector2(_LerpContainerRectTransform.anchorMax.x, 1),
                _SpawnLerpTime).SetEase(Ease.OutElastic, _SpawnEaseAmplitude, _SpawnEasePeriod);
        }

        public void Deregister()
        {
            if (AssignedOrder == null) return;

            _FoodImage.sprite = null;
            _WindowIdentifierImage.sprite = null;

            AssignedOrder.OnTimerUpdated -= UpdateOrderTimer;
        }

        private Image CreateIngredientIcon()
        {
            var icon = new GameObject("IngredientIcon").AddComponent<Image>();
            Transform iconTransform = icon.transform;
            iconTransform.SetParent(_IngredientGrid);
            iconTransform.localScale = Vector3.one;
            return icon;
        }
        private void SetIngredientIcons()
        {
            var i = 0;
            foreach (Food food in AssignedOrder.Food.Input)
            {
                Image icon = _IngredientImages.ElementAtOrDefault(i) ?? CreateIngredientIcon();
                icon.sprite = food.Icon;
                icon.gameObject.SetActive(true);
                i++;
            }

            for (;i < _IngredientImages.Count; i++)
            {
                _IngredientImages[i].gameObject.SetActive(false);
            }
        }
    }
}