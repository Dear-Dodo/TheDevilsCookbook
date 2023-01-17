using System;
using System.Collections.Generic;
using TDC.Cooking;
using TDC.Core.Utility;
using TDC.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu
{
    public enum SlotType
    {
        Food,
        Recipe
    }

    // #if UNITY_EDITOR
    //     [CustomEditor(typeof(RadialMenu))]
    //     public class InventoryWidgetEditor : Editor
    //     {
    //         public override void OnInspectorGUI()
    //         {
    //             DrawDefaultInspector();
    //             ((RadialMenu)target).UpdateSlots();
    //         }
    //     }
    // #endif

    public class RadialMenu : MonoBehaviour
    {
        public RadialMenuSlot SlotPrefab;

        [SerializeField, SerializedValueRequired]
        private RectTransform _BackgroundImage;

        [SerializeField] private float _BackgroundPadding = 0;

        public float MinRadius;
        public float MaxRadius;

        public float Spacing;

        public bool ScaleBackground;

        [Min(0)]
        public float SlotSize;

        public Action<Food> OnFoodSelected;
        public Action<Recipe> OnRecipeSelected;
        public Action<RadialMenuSlot> OnPointerEnter;
        public Action<RadialMenuSlot> OnPointerExit;

        public List<RadialMenuSlot> Slots;
        public SlotType Type;

        public List<Food> Foods = new List<Food>();
        public List<Recipe> Recipes = new List<Recipe>();
        private static readonly int _Radius = Shader.PropertyToID("Radius");
        private static readonly int _Thickness = Shader.PropertyToID("Thickness");

        private Action<int, int, StorableObject> OnInventoryChanged;
        

        public void Start()
        {
            //OnInventoryChanged = (section, index, item) => { UpdateSlots(); };
            //GameManager.PlayerCharacter.Inventory.InventoryChanged += OnInventoryChanged; 
            if (!ScaleBackground)
            {
                float backgroundSize = MaxRadius * 2 + SlotSize + _BackgroundPadding;
                _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundSize);
                _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundSize);
            }
        }

        public void OnDestroy()
        {
            //GameManager.PlayerCharacter.Inventory.InventoryChanged -= OnInventoryChanged;
        }

        public void UpdateSlots()
        {
            Slots = new List<RadialMenuSlot>();
            foreach (RadialMenuSlot slot in GetComponentsInChildren<RadialMenuSlot>())
            {
                if(slot != null)
                DestroyImmediate(slot.gameObject);
            }
            float angle;
            float totalAngle;
            float radius;
            float backgroundSize;
            float shaderRadius;
            switch (Type)
            {
                case SlotType.Food:
                    angle = 360.0f / Foods.Count;
                    totalAngle = 0.0f;
                    radius = Mathf.Clamp(((SlotSize + Spacing) * Foods.Count) / (2 * Mathf.PI), MinRadius,MaxRadius);
                    backgroundSize = radius * 2 + SlotSize + _BackgroundPadding;
                    shaderRadius = backgroundSize / 2;
                    _BackgroundImage.GetComponent<Image>().material.SetFloat(_Radius, shaderRadius);
                    _BackgroundImage.GetComponent<Image>().material.SetFloat(_Thickness, SlotSize + _BackgroundPadding);
                    if (ScaleBackground)
                    {
                        _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundSize);
                        _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundSize);
                    }

                    foreach (Food food in Foods)
                    {
                        RadialMenuSlot slot = Instantiate(SlotPrefab, transform);
                        slot.GetComponent<RectTransform>().anchoredPosition3D = Quaternion.Euler(0, 0, totalAngle) * new Vector3(0, radius, 0);
                        totalAngle += angle;
                        slot.CacheVariables();
                        slot.SetData(food);
                        slot.SetSize(SlotSize);
                        slot.button.onClick.AddListener(() => { OnClicked(slot); });
                        slot.OnPointerEnterSlot += OnPointerEnterSlot;
                        slot.OnPointerExitSlot += OnPointerExitSlot;
                        Slots.Add(slot);
                    }
                    break;

                case SlotType.Recipe:
                    angle = 360.0f / Recipes.Count;
                    totalAngle = 0.0f;
                    radius = Mathf.Clamp(((SlotSize + Spacing) * Recipes.Count) / (2 * Mathf.PI), MinRadius, MaxRadius);
                    backgroundSize = radius * 2 + SlotSize + _BackgroundPadding;
                    shaderRadius = backgroundSize / 2;
                    _BackgroundImage.GetComponent<Image>().material.SetFloat(_Radius, shaderRadius);
                    _BackgroundImage.GetComponent<Image>().material.SetFloat(_Thickness, SlotSize + _BackgroundPadding);
                    if (ScaleBackground)
                    {
                        _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundSize);
                        _BackgroundImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundSize);
                    }
                    foreach (Recipe recipe in Recipes)
                    {
                        RadialMenuSlot slot = Instantiate(SlotPrefab, transform);
                        slot.GetComponent<RectTransform>().anchoredPosition3D = Quaternion.Euler(0, 0, totalAngle) * new Vector3(0, radius, 0);
                        totalAngle += angle;
                        slot.CacheVariables();
                        slot.SetData(recipe);
                        slot.SetSize(SlotSize);
                        slot.button.onClick.AddListener(delegate { OnClicked(slot); });
                        slot.OnPointerEnterSlot += OnPointerEnterSlot;
                        slot.OnPointerExitSlot += OnPointerExitSlot;
                        Slots.Add(slot);
                    }
                    break;
            }
        }

        public void SetData(List<Food> foods)
        {
            Type = SlotType.Food;
            Foods = foods;
            UpdateSlots();
        }

        public void SetData(List<Recipe> recipes)
        {
            Type = SlotType.Recipe;
            Recipes = recipes;
            UpdateSlots();
        }

        private void OnClicked(RadialMenuSlot slotClicked)
        {
            if (slotClicked.Avaliable)
            {
                switch (slotClicked.Type)
                {
                    case SlotType.Food:
                        OnFoodSelected?.Invoke(slotClicked.Food);
                        break;

                    case SlotType.Recipe:
                        OnRecipeSelected?.Invoke(slotClicked.Recipe);
                        break;
                }
            }
        }

        private void OnPointerEnterSlot(RadialMenuSlot slot)
        {
            OnPointerEnter?.Invoke(slot);
        }

        private void OnPointerExitSlot(RadialMenuSlot slot)
        {
            OnPointerExit?.Invoke(slot);
        }
    }
}