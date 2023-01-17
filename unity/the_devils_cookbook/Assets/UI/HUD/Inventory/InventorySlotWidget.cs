using UnityEngine;
using UnityEngine.UI;
using TDC.Items;
using System.Linq;
using TMPro;

namespace TDC.UI.HUD.InventoryWindow
{
    public class InventorySlotWidget : MonoBehaviour
    {
        [HideInInspector]
        public Inventory Inventory;
        public ItemTypes ItemType;
        public (int, StorableObject) Index;
        public Image Icon;
        public TextMeshProUGUI Counter;
        public Button Button;
        [Header("Normal Colours")]
        public ColorBlock NormalColours;
        [Header("Delete Colours")]
        public ColorBlock DeleteColours;
        [Space]
        public bool DeleteMode = false;
        private Material _Material;

        public void Awake()
        {
            _Material = Instantiate(Icon.material);
            Icon.material = _Material;
        }

        public void Initialse()
        {
            UpdateSprite();
            Inventory.InventoryChanged += OnInventoryChanged;
            Button.onClick.AddListener(OnDelete);
        }

        private void Update()
        {
            if (DeleteMode)
            {
                Button.enabled = true;
                Button.colors = DeleteColours;
            } else
            {
                Button.enabled = false;
                Button.colors = NormalColours;
            }
            UpdateSprite();
            UpdateText();
        }

        void OnDelete()
        {
            if (Inventory != null && Inventory.GetItem(Index) > 0)
            {
                Inventory.RemoveItem(Index);
            }
        }

        private void OnDestroy()
        {
            if (Inventory)
            Inventory.InventoryChanged -= OnInventoryChanged;
        }

        void OnInventoryChanged(int index1, StorableObject item)
        {
            UpdateSprite();
            UpdateText();
        }

        void UpdateSprite()
        {
            if (ItemType != ItemTypes.Ingedient)
            {
                Index.Item2 = null;
                if (Inventory.Sections[Index.Item1].Items.Count() > 0)
                {
                    foreach (StorableObject item in Inventory.Sections[Index.Item1].Items.Keys)
                    {
                        if (Inventory.Sections[Index.Item1].Items[item] > 0)
                        {
                            Index.Item2 = item;
                        }
                    }
                }
            }
            if (Index.Item2 != null && Index.Item2.StorageTypes == ItemTypes.Ingedient)
                Icon.sprite = Index.Item2.Icon;
            if (Inventory.HasItem(Index))
            {
                Icon.sprite = Index.Item2.Icon;
                Icon.color = Button.colors.normalColor;
                Button.interactable = true;
                _Material.SetFloat("Strength", 0);
                if (Index.Item2 == null || Index.Item2.StorageTypes != ItemTypes.Ingedient)
                    Icon.enabled = true;
            } else
            {
                Icon.color = Button.colors.disabledColor;
                Button.interactable = false;
                _Material.SetFloat("Strength", 1);
                if (Index.Item2 == null || Index.Item2.StorageTypes != ItemTypes.Ingedient)
                    Icon.enabled = false;
            }
        }

        void UpdateText()
        {
            if (Counter != null)
            {
                string count;
                if (Index.Item2 != null)
                {
                    count = Inventory.Sections[Index.Item1].Items[Index.Item2].ToString();
                }
                else
                {
                    count = "0";
                }
                count += "x";
                Counter.SetText(count);
            }
        }
    }
}
