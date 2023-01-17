using System.Collections.Generic;
using TDC.Core.Manager;
using TDC.Items;
using TDC.UI.HUD.InventoryWindow;
using UnityEngine;

namespace TDC
{
    public class InventorySectionWidget : MonoBehaviour
    {
        [HideInInspector] public Inventory Inventory;
        [HideInInspector] public Section Section;
        [HideInInspector] public int SectionIndex;

        public InventorySlotWidget SlotPrefab;
        public float Radius;
        public List<InventorySlotWidget> Slots;

        public void Regenerate()
        {
            Slots = new List<InventorySlotWidget>();
            foreach (InventorySlotWidget slot in GetComponentsInChildren<InventorySlotWidget>())
            {
                DestroyImmediate(slot.gameObject);
            }
            if (SlotPrefab != null)
            {
                for (int i = 0; i < Section.Slots; i++)
                {
                    float angle = (360.0f / Mathf.Max(Section.Items.Count,1)) * i;
                    InventorySlotWidget Slot = Instantiate(SlotPrefab, transform);
                    Slot.GetComponent<RectTransform>().anchoredPosition3D = Quaternion.Euler(0, 0, angle).normalized * new Vector3(0, Radius, 0);
                    Slot.Inventory = Inventory;
                    if (Section.StoredTypes == ItemTypes.Ingedient)
                    {
                        Slot.Index = (SectionIndex, GameManager.OrderManager.Ingredients[i]);
                        Slot.ItemType = ItemTypes.Ingedient;
                    }
                    else
                    {
                        Slot.Index = (SectionIndex, null);
                        Slot.ItemType = ItemTypes.OrderableFood;
                    }
                    Slot.Initialse();
                    Slots.Add(Slot);
                }
            }
        }

        public void SetDeleteMode(bool deleteMode)
        {
            foreach (InventorySlotWidget slot in Slots)
            {
                slot.DeleteMode = deleteMode;
            }
        }
    }
}
