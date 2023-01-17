using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Type;
using TDC.Player.Upgrades;
using TDC.Spellcasting;
using TDC.ThirdParty.SerializableDictionary;
using UnityEngine;

namespace TDC.Player
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "TDC/Player/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [SerializeField] public PlayerUpgrade<int> InventorySize;
        [SerializeField] public PlayerUpgrade<float> Speed;
        [SerializeField] public List<Spell> ActiveSpells;
        [SerializeField] public Dictionary<Type, PlayerUpgrade<Spell>> SpellUpgrades = new Dictionary<Type, PlayerUpgrade<Spell>>();
        [SerializeField] private SerializableDictionary<Spell, PlayerUpgrade<Spell>> _SpellUpgrades = new SerializableDictionary<Spell, PlayerUpgrade<Spell>>();
        [SerializeField] public bool ShopTutorialPlayed = false;

        [HideInInspector][SerializeField] public string Build;

        public DirtyProperty<int> Health;
        public DirtyProperty<int> Currency;

        public Func<PlayerStats, int, int> PlayerDamageModifiers;

        public int MaxHealth;

        public void Damage(int value)
        {
            int modifiedValue = PlayerDamageModifiers?.GetInvocationList()
                                    .Aggregate(value, (current, del) => ((Func<PlayerStats, int, int>)del)
                                        .Invoke(this, current)) ?? value;

            Health.Value -= modifiedValue;
        }

        public Task Initialize()
        {
            Build = Application.version;
            foreach (KeyValuePair<Spell, PlayerUpgrade<Spell>> kvp in _SpellUpgrades)
            {
                SpellUpgrades.Add(kvp.Key.GetType(), kvp.Value);
            }
            Health.Value = MaxHealth;
            return Task.CompletedTask;
        }

        public void OnValidate()
        {
            foreach (KeyValuePair<Spell, PlayerUpgrade<Spell>> kvp in _SpellUpgrades)
            {
                if (kvp.Key == null) continue;

                var toRemove = new List<PlayerUpgrade<Spell>.UpgradeTier>();

                foreach (var tier in kvp.Value.Tiers)
                {
                    if (kvp.Key.GetType() != tier.Item.GetType())
                    {
                        toRemove.Add(tier);
                    }
                }

                foreach (PlayerUpgrade<Spell>.UpgradeTier tier in toRemove)
                {
                    kvp.Value.Tiers.Remove(tier);
                }
            }
        }

        public async Task<PlayerStats> Load(string SaveData)
        {
            if (File.Exists(SaveData))
            {
                await GameManager.PlayerDataWriteAsync.WaitAsync();
                StreamReader file = new StreamReader(SaveData);

                PlayerStats newStats = Instantiate(this);
                bool LoadSucceeded = true;
                try
                {
                    JsonUtility.FromJsonOverwrite(await file.ReadToEndAsync(), newStats);
                    LoadSucceeded = LoadSucceeded && newStats.Build == Application.version;
                    await newStats.Initialize();
                    await GameManager.PlayerInitialised.WaitAsync();
                    for (int i = 0; i < GameManager.PlayerCharacter.Spellcaster.Spells.Length; i++)
                    {
                        LoadSucceeded = LoadSucceeded && newStats.SpellUpgrades.ContainsKey(newStats.ActiveSpells[i].GetType());
                    }
                }
                catch
                {
                    LoadSucceeded = false;
                }
                file.Close();
                if (LoadSucceeded)
                {
                    return newStats;
                }
            }
            return this;
        }

        public void Save(string SaveData, bool destroy)
        {
            GameManager.PlayerDataWriteAsync.Reset();
            StreamWriter file = new StreamWriter(SaveData);

            file.WriteAsync(JsonUtility.ToJson(this)).Wait();
            file.Close();
            GameManager.PlayerDataWriteAsync.Set();
            if (destroy)
            {
                Destroy(this);
            }
        }
    }
}