using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TDC.Items;
using TDC.Core.Type;

namespace TDC.UI.HUD.InventoryWindow
{
#if UNITY_EDITOR
    [CustomEditor(typeof(InventoryWidget))]
    public class InventoryWidgetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Rebuild Widgets"))
            {
                ((InventoryWidget)target).RebuildSlotWidgets();
            }
        }
    }
#endif

    public class InventoryWidget : MonoBehaviour, IHideable
    {
        public Inventory Inventory;
        public List<InventorySectionWidget> SectionGroups;
        public DirtyProperty<bool> DeleteMode;

        public void RebuildSlotWidgets()
        {
            for (int i = 0; i < SectionGroups.Count; i++)
            {
                SectionGroups[i].Inventory = Inventory;
                SectionGroups[i].Section = Inventory.Sections[i];
                SectionGroups[i].SectionIndex = i;
                SectionGroups[i].Regenerate();
            }
        }

        public void SetDeleteMode(bool deleteMode)
        {
            foreach (InventorySectionWidget section in SectionGroups)
            {
                section.SetDeleteMode(deleteMode);
            }
        }

        // Start is called before the first frame update
        async void Awake()
        {
            await Inventory.InventoryInitalised.WaitAsync();
            Initalise();
        }

        private void Initalise()
        {
            RebuildSlotWidgets();
            DeleteMode.OnValueSet += SetDeleteMode;
            SetDeleteMode(DeleteMode.Value);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}
