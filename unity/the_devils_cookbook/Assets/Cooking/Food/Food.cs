using UnityEngine;
using TDC.Core.Utility;
using TDC.Ingredient;
using TDC.Items;


namespace TDC
{
    [CreateAssetMenu(fileName = "Food", menuName = "TDC/Cooking/Food")]
    public class Food : StorableObject
    {
        public bool IsOrderable;
        public Difficulty ValidDifficulties;
        public float Weighting;
        public Creature Creature;
    }
}
