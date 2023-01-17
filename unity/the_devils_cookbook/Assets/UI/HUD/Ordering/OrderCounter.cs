using UnityEngine;
using TMPro;
using TDC.Core.Manager;
using TDC.UI;

namespace TDC
{
    public class OrderCounter : MonoBehaviour, IHideable
    {
        public TextMeshProUGUI Counter;

        private void Start()
        {
            GameManager.RunOnLevelInitialisation(() =>
            {
                if (GameManager.CurrentLevelData.OrderCount <= 0) gameObject.SetActive(false);
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.LevelInitialisedAsync.IsSet) return;
            Counter.text = string.Format("{0}/{1}", GameManager.OrderManager.CompletedOrders.Count + GameManager.OrderManager.FailedOrders, GameManager.CurrentLevelData.OrderCount);
            Counter.color = new Color(1, 1 - 0.2f * GameManager.OrderManager.FailedOrders, 1 - 0.2f * GameManager.OrderManager.FailedOrders);
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}
