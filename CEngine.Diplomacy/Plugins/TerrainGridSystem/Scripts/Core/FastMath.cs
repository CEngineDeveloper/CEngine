using System;

namespace TGS {

    public static class FastMath {
        private static readonly double[] RoundLookup = CreateRoundLookup();

        private static double[] CreateRoundLookup() {
            double[] result = new double[15];
            for (int i = 0; i < result.Length; i++) {
                result[i] = Math.Pow(10, i);
            }

            return result;
        }

        public static double Round(double value) {
            return Math.Floor(value + 0.5);
        }

        public static double Round(double value, int decimalPlaces) {
            double adjustment = RoundLookup[decimalPlaces];
            return Math.Floor(value * adjustment + 0.5) / adjustment;
        }

        public static void Round(ref double value1, ref double value2, int decimalPlaces) {
            double adjustment = RoundLookup[decimalPlaces];
            value1 = Math.Floor(value1 * adjustment + 0.5) / adjustment;
            value2 = Math.Floor(value2 * adjustment + 0.5) / adjustment;
        }

        public static int RoundToInt(double value) {
            return (int)Math.Floor(value + 0.5);
        }
    }

}

