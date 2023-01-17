using UnityEngine;

namespace TDC.Core.Type
{
    [System.Serializable]
    public class Range
    {
        [SerializeField] public float Min;
        [SerializeField] public float Max;

        public bool Evaluate(float value) => (value <= Max && Min <= value);

        public float Random() => UnityEngine.Random.Range(Min, Max);
    }
}