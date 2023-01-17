using UnityEngine;

namespace TDC.Core.Utility
{
    public static class Math
    {
        public static readonly float[] PrecalculatedFactorials = {
            1.0f,
            1.0f,
            2.0f,
            6.0f,
            24.0f,
            120.0f,
            720.0f,
            5040.0f,
            40320.0f,
            362880.0f,
            3628800.0f,
            39916800.0f,
            479001600.0f,
            6227020800.0f,
            87178291200.0f,
            1307674368000.0f,
            20922789888000.0f,
        };

        public static float CalculateFactorial(int input)
        {
            if (Range(input, 0, PrecalculatedFactorials.Length))
            {
                return PrecalculatedFactorials[input];
            }

            var returnValue = (ulong)input;
            for (ulong i = returnValue - 1; i > 0; i--)
            {
                returnValue *= i;
            }
            return returnValue;
        }

        public static float BinomialCoefficient(int n, int i)
        {
            float a1 = CalculateFactorial(n);
            float a2 = CalculateFactorial(i);
            float a3 = CalculateFactorial(n - 1);
            float ni = a1 / (a2 * a3);
            return ni;
        }

        public static float BernsteinPolynomial(int n, int i, float t) => BinomialCoefficient(n, i) * Mathf.Pow(t, i) * Mathf.Pow((1 - t), (n - i));

        public static bool Range(int input, int min, int max) => (input) >= min && input <= max;

        public static float Percentage(float value, float max) => value / max;
    }
}