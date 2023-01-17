using System;
using System.Collections.Generic;
using TDC.Core.Manager;

namespace TDC.Items
{
    [Serializable]
    public struct Section
    {
        public ItemTypes StoredTypes;
        public int Slots;
        public int Capacity;
        public Dictionary<StorableObject,int> Items;

        public Section(ItemTypes storedTypes, int slots, int capacity)
        {
            StoredTypes = storedTypes;
            Capacity = capacity == -1 ? int.MaxValue : capacity;
            Slots = slots;
            Items = new Dictionary<StorableObject, int>();
            if (storedTypes == ItemTypes.Ingedient)
            {
                foreach (StorableObject ingredient in GameManager.OrderManager.Ingredients)
                {
                    Items.Add(ingredient, 0);
                }
            }
        }
    }
}
