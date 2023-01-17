using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TDC.Core.Manager;
using UnityEngine;

namespace TDC.Items
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField]
        public List<Section> Sections = new List<Section>();
        public Action<int, StorableObject> InventoryChanged;
        public AsyncManualResetEvent InventoryInitalised = new AsyncManualResetEvent();

        public event Action<StorableObject> ItemRemoved;
        public event Action<StorableObject> ItemAdded;
        
        public void Awake()
        {
            InventoryInitalised.Reset();
            GameManager.RunOnInitialisation(Initialise);
        }

        private void Initialise()
        {
            for (var i = 0; i < Sections.Count; i++)
            {
                Section section = Sections[i];
                Sections[i] = new Section(section.StoredTypes, section.Slots, section.Capacity);
            }
            InventoryInitalised.Set();
        }

        public int GetTotalSize()
        {
            return Sections.Aggregate(0, (s1, s2) => s1 + s2.Capacity);
        }
        
        public int GetItemCount()
        {
            int items = 0;
            foreach (Section section in Sections)
            {
                foreach (int item in section.Items.Values)
                {
                        items += item;
                }
            }
            return items;
        }

        public Section AddSection(Section section)
        {
            Sections.Add(section);
            InventoryChanged?.Invoke(Sections.IndexOf(section), null);
            return section;
        }

        public bool DepositItems(Dictionary<StorableObject, int> items, ItemTypes itemType, out Dictionary<StorableObject, int> remainingItems)
        {
            remainingItems = new Dictionary<StorableObject, int>(items);
            foreach (StorableObject item in items.Keys) {
                foreach (Section section in Sections)
                {
                    if (section.StoredTypes == itemType)
                    {
                        int count = 0;
                        foreach (int number in section.Items.Values)
                        {
                            count += number;
                        }
                        if (count < section.Capacity)
                        {
                            if (section.Items.ContainsKey(item))
                            {
                                section.Items[item] += items[item];
                            }
                            else
                            {
                                section.Items.Add(item, 1);
                            }
                            remainingItems.Remove(item);
                            ItemAdded?.Invoke(item);
                            InventoryChanged?.Invoke(Sections.IndexOf(section), item);
                        }
                    }
                }
            }
            return (remainingItems.Count == 0);
        }

        public bool DepositItems(Dictionary<StorableObject, int> items, Section section, out Dictionary<StorableObject, int> remainingItems)
        {
            remainingItems = new Dictionary<StorableObject, int>(items);
            foreach (StorableObject item in items.Keys)
            {
                int count = 0;
                foreach (int number in section.Items.Values)
                {
                    count += number;
                }
                if (count < section.Capacity)
                {
                    if (section.Items.ContainsKey(item))
                    {
                        section.Items[item] += items[item];
                    }
                    else
                    {
                        section.Items.Add(item, 1);
                    }
                    remainingItems.Remove(item);
                    ItemAdded?.Invoke(item);
                    InventoryChanged?.Invoke(Sections.IndexOf(section), item);
                }
            }
            return (remainingItems.Count == 0);
        }

        public bool DepositItems(Dictionary<StorableObject, int> items, ItemTypes itemType)
        {
            Dictionary<Item, int> remainingItems;
            return DepositItems(items, itemType, out Dictionary<StorableObject, int> _);
        }

        public bool DepositItems(Dictionary<StorableObject, int> items, Section section)
        {
            return DepositItems(items, section, out Dictionary<StorableObject, int> _);
        }

        public bool TryWithdrawItems(out Dictionary<StorableObject, int> items, params Query[] queries)
        {
            int targetItemCount = 0;
            int foundItemCount = 0;
            items = new Dictionary<StorableObject, int>();
            foreach (Query query in queries)
            {
                int queryItemCount = 0;
                targetItemCount += query.Quantity;
                foreach (Section section in Sections)
                {
                    foreach (StorableObject item in new List<StorableObject>(section.Items.Keys))
                    {
                        if (queryItemCount < query.Quantity && item != null && item.Name == query.ToFind.Name)
                        {
                            int foundItems = section.Items.ContainsKey(item) ? Mathf.Clamp(section.Items[item],0, query.Quantity) : 0;
                            foundItemCount += foundItems;
                            queryItemCount += foundItems;
                            
                            if (items.ContainsKey(item))
                            {
                                items[item] += foundItems;
                            }
                            else
                            {
                                items.Add(item, foundItems);
                            }
                            if (section.Items.ContainsKey(item))
                            {
                                section.Items[item] -= foundItems;
                            }
                            InventoryChanged?.Invoke(Sections.IndexOf(section), item);
                        }
                    }
                }
            }
            return (targetItemCount == foundItemCount);
        }

        public bool RemoveItem(int section, StorableObject item)
        {
            bool removed = Sections[section].Items[item] < 0;
            Sections[section].Items[item]--;
            if (Sections[section].Items[item] < 0)
                Sections[section].Items[item] = 0;
            ItemRemoved?.Invoke(item);
            InventoryChanged?.Invoke(section, item);
            return removed;
        }

        public bool RemoveItem((int, StorableObject) index)
        {
            return RemoveItem(index.Item1, index.Item2);
        }

        public bool RemoveItems(params Query[] queries)
        {
            int targetItemCount = 0;
            int foundItemCount = 0;
            foreach (Query query in queries)
            {
                int queryItemCount = 0;
                targetItemCount += query.Quantity;
                foreach (Section section in Sections)
                {
                    foreach (StorableObject item in new List<StorableObject>(section.Items.Keys))
                    {
                        if (queryItemCount < query.Quantity && item != null && item.name == query.ToFind.name)
                        {
                            int foundItems = section.Items.ContainsKey(item) ? Mathf.Clamp(section.Items[item], 0, query.Quantity) : 0;
                            foundItemCount += foundItems;
                            queryItemCount += foundItems;
                            section.Items[item] -= query.Quantity;
                            if (section.Items[item] < 0)
                                section.Items[item] = 0;
                            InventoryChanged?.Invoke(Sections.IndexOf(section), item);
                        }
                    }
                }
            }
            return (targetItemCount == foundItemCount);
        }

        public bool HasItem(int section, StorableObject item)
        {
            return (Sections.Count > section) && GetItem(section,item) > 0;
        }
        public bool HasItem((int, StorableObject) index)
        {
            return (Sections.Count > index.Item1) && GetItem(index.Item1, index.Item2) > 0;
        }


        public int GetItem(int section, StorableObject item)
        {
            return item != null ? Sections[section].Items.ContainsKey(item) ? Sections[section].Items[item] : 0 : 0;
        }
        
        public int GetItem((int, StorableObject) index)
        {
            return index.Item2 != null ? Sections[index.Item1].Items.ContainsKey(index.Item2) ? Sections[index.Item1].Items[index.Item2] : 0 : 0;
        }

        public bool HasItems(params Query[] queries)
        {
            int targetItemCount = 0;
            int foundItemCount = 0;
            Dictionary<Section,Dictionary<StorableObject,int>> usedItems = new Dictionary<Section, Dictionary<StorableObject, int>>();
            foreach (Query query in queries)
            {
                targetItemCount += query.Quantity;
                int queryItemCount = 0;
                foreach (Section section in Sections)
                {
                    foreach (StorableObject item in section.Items.Keys)
                    {
                        if (queryItemCount < query.Quantity && item != null && item.Name == query.ToFind.Name)
                        {
                            
                            int foundItems = section.Items.ContainsKey(item) ? Mathf.Clamp(section.Items[item] - (usedItems.ContainsKey(section) ? (usedItems[section].ContainsKey(item) ? usedItems[section][item] : 0) : 0), 0, query.Quantity) : 0;
                            if (usedItems.ContainsKey(section) && usedItems[section].ContainsKey(item))
                            {
                                usedItems[section][item] += foundItems;
                            } else if (usedItems.ContainsKey(section))
                            {
                                usedItems[section].Add(item,foundItems);
                            } else
                            {
                                usedItems.Add(section,new Dictionary<StorableObject,int> { {item, foundItems } });
                            }
                            foundItemCount += foundItems;
                            queryItemCount += foundItems;
                        }
                    }
                }
            }
            return (targetItemCount <= foundItemCount);
        }

        public bool HasSpaceForType(ItemTypes itemType, int quantity)
        {
            foreach (Section section in Sections)
            {
                if (section.StoredTypes == itemType)
                {
                    quantity -= section.Capacity;
                }
            }
            return quantity <= 0;
        }

        public void Clear()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                Sections[i].Items.Clear();
            }
        }
    }
}