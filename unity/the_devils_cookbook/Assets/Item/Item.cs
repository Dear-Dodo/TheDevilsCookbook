using System.Collections.Generic;
using DG.Tweening;
using UnityAsync;
using UnityEngine;

namespace TDC.Items
{
    public class Item : MonoBehaviour
    {
        private const float DropTweenDuration = 0.25f;
        private const float PickupTweenDuration = 0.25f;
        
        public StorableObject Data;

        public void Drop(Vector3 target)
        {
            gameObject.SetActive(true);
            transform.DOMove(target, DropTweenDuration);
        }

        public async void Pickup(Transform target)
        {
            Transform selfTransform = transform;
            selfTransform.DOMove(target.position, PickupTweenDuration);
            await Await.Seconds(PickupTweenDuration);
            selfTransform.SetParent(target);
            selfTransform.position = target.position;
            gameObject.SetActive(false);
        }

        public static Item CreateItem(StorableObject itemData)
        {
            GameObject itemObject = new GameObject();
            Item item = itemObject.AddComponent<Item>();
            item.Data = itemData;
            itemObject.SetActive(false);
            return item;
        }

        public static Dictionary<StorableObject, int> ToItems(List<StorableObject> dataObjects)
        {
            Dictionary<StorableObject, int> items = new Dictionary<StorableObject, int>();
            foreach (StorableObject dataObject in dataObjects)
            {
                items[dataObject]++;
            }
            return items;
        }

        public static Dictionary<StorableObject, int> ToItems(StorableObject[] dataObjects)
        {
            Dictionary<StorableObject, int> items = new Dictionary<StorableObject, int>();
            foreach (StorableObject dataObject in new List<StorableObject>(dataObjects))
            {
                if (items.ContainsKey(dataObject))
                {
                    items[dataObject]++;
                } else
                {
                    items.Add(dataObject, 1);
                }
            }
            return items;
        }
    }
}
