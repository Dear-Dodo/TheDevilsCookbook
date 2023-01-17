using System;
using TDC.Cooking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public class RadialMenuSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public SlotType Type;
        public Food Food;
        public Recipe Recipe;
        public Button button;
        public Image Icon;
        public bool Avaliable;
        private RectTransform _Transform;
        private Material _Material;

        public Action<RadialMenuSlot> OnPointerEnterSlot;
        public Action<RadialMenuSlot> OnPointerExitSlot;

        // Start is called before the first frame update
        void Start()
        {
            CacheVariables();
            UpdateIcon();
        }

        // Update is called once per frame
        void Update()
        {
            button.interactable = Avaliable;
            if (Avaliable)
            {
                _Material.SetFloat("Strength", 0);
            } else
            {
                _Material.SetFloat("Strength", 1);
            }
        }

        public void CacheVariables()
        {
            _Transform = GetComponent<RectTransform>();
            _Material = Instantiate(Icon.material);
            Icon.material = _Material;
        }
            

        public void UpdateIcon()
        {
            switch (Type)
            {
                case SlotType.Food:
                    Icon.sprite = Food.Icon;
                    break;
                case SlotType.Recipe:
                    Icon.sprite = Recipe.Output[0].Icon;
                    break;
            }
        }

        public void SetData(Food food)
        {
            Type = SlotType.Food;
            Food = food;
            UpdateIcon();
        }

        public void SetData(Recipe recipe)
        {
            Type = SlotType.Recipe;
            Recipe = recipe;
            UpdateIcon();
        }

        public void SetSize(float size)
        {
            _Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size * 0.8f);
            _Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size * 0.8f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterSlot?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitSlot?.Invoke(this);
        }
    }
}
