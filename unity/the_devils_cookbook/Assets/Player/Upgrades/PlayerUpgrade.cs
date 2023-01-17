using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDC.Player.Upgrades
{
    [Serializable]
    public class PlayerUpgrade<T>
    {
        [Serializable]
        public struct UpgradeTier
        {
            public T Item;
            public int Cost;
            public string Description;
        }

        [SerializeField] public string Name;
        [SerializeField] public Sprite UpgradeSprite;
        [SerializeField] public int CurrentTier;
        [SerializeField] public List<UpgradeTier> Tiers;
        [SerializeField] public Action<T> OnChanged;

        public T GetValue() => GetCurrentTier().Item;

        public UpgradeTier GetCurrentTier() => Tiers[Mathf.Clamp(CurrentTier, 0, Tiers.Count - 1)];
        public UpgradeTier GetTier(int index) => Tiers[Mathf.Clamp(index, 0, Tiers.Count - 1)];
        public UpgradeTier GetNextTier() => Tiers[Mathf.Clamp(CurrentTier + 1, 0, Tiers.Count - 1)];

        public void SetTier(int index) => CurrentTier = Mathf.Clamp(index, 0, Tiers.Count - 1);

        public void Upgrade(PlayerStats stats)
        {
            if (stats.Currency.Value >= GetNextTier().Cost)
            {
                CurrentTier++;
                stats.Currency.Value -= GetCurrentTier().Cost;
                Debug.Log($"Upgraded to tier {CurrentTier}, player currency {stats.Currency}");
                OnChanged?.Invoke(GetValue());
            }
        }

        public void Downgrade(PlayerStats stats)
        {
            CurrentTier--;
            stats.Currency.Value += GetCurrentTier().Cost;
            OnChanged?.Invoke(GetValue());
        }
    }
}