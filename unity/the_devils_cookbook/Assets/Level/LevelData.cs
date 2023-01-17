using System.Collections.Generic;
using TDC.Cooking;
using TDC.Core.Type;
using TDC.Core.Utility;
using TDC.Core.Utility.Samplers;
using TDC.ThirdParty.SerializableDictionary;
using UnityEngine;

namespace TDC.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "TDC/Level/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<Recipe> RecipePool;
        public bool AutoGenerateOrders = true;
        public int OrderCount;
        public SerializableDictionary<Difficulty, float> TimeDifficulties;
        public float WindowCooldown;
        public Range OrderSpawnRange;
        public int CurrencyEarned;
        public int CatchBonus;
        public int LevelSeed;
        public bool DoCountdown = true;

        [Header("Level Select UI Information")]
        public Sprite Icon;

        public string LevelName;

        [SerializeField, HideInInspector]
        public PoissonDisc PoissonDisc;
    }
}