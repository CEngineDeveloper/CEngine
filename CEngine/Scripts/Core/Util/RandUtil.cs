using System.Collections.Generic;
using UnityEngine;
using URandom;
using Normalization = UnityRandom.Normalization;

namespace CYM
{
    public partial class RandUtil
    {
        private static UnityRandom URAND;
        [RuntimeInitializeOnLoadMethod]
        static void InitRandom()
        {
            URAND = new UnityRandom(System.DateTime.Now.Millisecond);
            Random.InitState(System.DateTime.Now.Millisecond);
        }

        #region normal
        public static void RandForwardY(BaseUnit unit, Vector3 upwards)
        {
            if (upwards == Vector3.up)
                unit.Trans.eulerAngles = new Vector3(unit.Trans.eulerAngles.x, Range(0.0f, 360.0f), unit.Trans.eulerAngles.z);
            else if (upwards == Vector3.forward)
                unit.Trans.eulerAngles = new Vector3(unit.Trans.eulerAngles.x, unit.Trans.eulerAngles.y, Range(0.0f, 360.0f));
            else if (upwards == Vector3.right)
                unit.Trans.eulerAngles = new Vector3(Range(0.0f, 360.0f), unit.Trans.eulerAngles.y, unit.Trans.eulerAngles.z);
        }
        #endregion

        #region urand
        // VALUE Return a Float 0 - 1
        public static float Value() => URAND.Value();
        // VALUE Return a Float 0 - 1
        public static float Value(Normalization n, float t) => URAND.Value(n, t);
        // POISSON Return a Float
        public static float Possion(float lambda) => URAND.Possion(lambda);
        // EXPONENTIAL Return a Float
        public static float Exponential(float lambda) => URAND.Exponential(lambda);
        // GAMMA Return a Float
        public static float Gamma(float order) => URAND.Gamma(order);
        // POINT IN A SQUARE Return a Vector2
        public static Vector2 PointInASquare() => URAND.PointInASquare();
        // POINT IN A SQUARE Return a Vector2
        public static Vector2 PointInASquare(Normalization n, float t) => URAND.PointInASquare(n, t);
        // RANDOM POINT IN A CIRCLE centered at 0
        // FROM http://mathworld.wolfram.com/CirclePointPicking.html
        // Take a number between 0 and 2PI and move to Cartesian Coordinates
        public static Vector2 PointInACircle() => URAND.PointInACircle();
        // RANDOM POINT IN A CIRCLE centered at 0
        // FROM http://mathworld.wolfram.com/CirclePointPicking.html
        // Take a number between 0 and 2PI and move to Cartesian Coordinates
        public Vector2 PointInACircle(Normalization n, float t) => URAND.PointInACircle(n, t);
        // RANDOM POINT in a DISK
        // FROM http://mathworld.wolfram.com/DiskPointPicking.html
        public Vector2 PointInADisk() => URAND.PointInADisk();
        // RANDOM POINT in a DISK
        // FROM http://mathworld.wolfram.com/DiskPointPicking.html
        public Vector2 PointInADisk(Normalization n, float t) => URAND.PointInADisk(n, t);
        // RANDOM POINT IN A CUBE. Return a Vector3
        public Vector3 PointInACube() => URAND.PointInACube();
        // RANDOM POINT IN A CUBE. Return a Vector3
        public Vector3 PointInACube(Normalization n, float t) => URAND.PointInACube(n, t);
        // RANDOM POINT ON A CUBE. Return a Vector3
        public Vector3 PointOnACube() => URAND.PointOnACube();
        // RANDOM POINT ON A CUBE. Return a Vector3
        public Vector3 PointOnACube(Normalization n, float t) => URAND.PointOnACube(n, t);
        // RANDOM POINT ON A SPHERE. Return a Vector3
        public Vector3 PointOnASphere() => URAND.PointOnASphere();
        // RANDOM POINT ON A SPHERE. Return a Vector3
        public Vector3 PointOnASphere(Normalization n, float t) => URAND.PointOnASphere(n, t);
        // RANDOM POINT IN A SPHERE. Return a Vector3
        public Vector3 PointInASphere() => URAND.PointInASphere();
        // RANDOM POINT IN A SPHERE. Return a Vector3
        public Vector3 PointInASphere(Normalization n, float t) => URAND.PointInASphere(n, t);
        // RANDOM POINT IN A CAP. Return a Vector3 
        // TODO: see RandomSphere GetPointOnCap(float spotAngle, ref NPack.MersenneTwister _rand, Quaternion orientation)
        public Vector3 PointOnCap(float spotAngle) => URAND.PointOnCap(spotAngle);
        public Vector3 PointOnCap(float spotAngle, Normalization n, float t) => URAND.PointOnCap(spotAngle, n, t);
        // RANDOM POINT IN A RING on a SPHERE. Return a Vector3 
        // TODO: see RandomSphere public static Vector3 GetPointOnRing(float innerSpotAngle, float outerSpotAngle, ref NPack.MersenneTwister _rand, Quaternion orientation)
        public Vector3 PointOnRing(float innerAngle, float outerAngle) => URAND.PointOnRing(innerAngle, outerAngle);
        public Vector3 PointOnRing(float innerAngle, float outerAngle, Normalization n, float t) => URAND.PointOnRing(innerAngle, outerAngle, n, t);
        // RANDOM RAINBOW COLOR
        public Color Rainbow() => URAND.Rainbow();
        // RANDOM RAINBOW COLOR
        public Color Rainbow(Normalization n, float t) => URAND.Rainbow(n, t);
        // RANDOM DICES
        public DiceRoll RollDice(int size, DiceRoll.DiceType type) => URAND.RollDice(size, type);
        // START a FLOAT SHUFFLE BAG
        // Note the a value can be shuffled with himself
        public ShuffleBagCollection<float> ShuffleBag(float[] values) => URAND.ShuffleBag(values);
        // START a WIGHTED FLOAT SHUFFLE BAG, the trick is the it is added many times
        // Note the a value can be shuffled with himself
        public ShuffleBagCollection<float> ShuffleBag(Dictionary<float, int> dict) => URAND.ShuffleBag(dict);
        #endregion

        #region normal
        /// <summary>
        /// 0-1.0f
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool Rand(float prop)
        {
            if (prop >= 1.0f) return true;
            if (prop <= 0.0f) return false;
            return prop * 100.0f >= Random.Range(0.0f, 100.0f);
        }
        /// <summary>
        /// min [inclusive] and max [exclusive]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandInt(int min, int max)
        {
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        public static float RandFloat(float min, float max)
        {
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        public static T Rand<T>(params System.Func<T>[] actions)
        {
            if (actions == null)
                return default;
            foreach (var item in actions)
            {
                if (Rand(1.0f / actions.Length))
                {
                    return item.Invoke();
                }
            }
            return RandArray(actions).Invoke();
        }
        // 随机范围,包含min,不包含max
        public static int Range(int min, int max)
        {
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        // 随机范围,包含mini,不包含max
        public static float Range(float min, float max)
        {
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        public static int RangeInt(Range range)
        {
            int min = (int)range.Min;
            int max = (int)range.Max;
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        public static float RangeFloat(Range range)
        {
            float min = range.Min;
            float max = range.Max;
            if (min >= max)
                return min;
            return Random.Range(min, max);
        }
        public static int RangeArray(int max)
        {
            if (max <= 0)
                return 0;
            return Range(0, max);
        }
        public static T RandArray<T>(T[] array)
        {
            if (array == null) return default;
            if (array.Length <= 0) return default;
            return array[RangeArray(array.Length)];
        }
        public static T RandArray<T>(HashList<T> array)
            where T : class
        {
            if (array == null) return default;
            if (array.Count <= 0) return default;
            return array[RangeArray(array.Count)];
        }
        public static T RandArray<T>(List<T> array)
        {
            if (array == null) return default;
            if (array.Count <= 0) return default;
            return array[RangeArray(array.Count)];
        }
        public static Vector3 RandCirclePoint(Vector3 center, float radius)
        {
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 36) * 10, 0f);
            Vector3 newPos = center + rotation * new Vector3(0, 0f, radius);
            return newPos;
        }
        public static Quaternion RandQuaterion()
        {
            return Quaternion.Euler(0f, Random.Range(0, 36) * 10, 0f);
        }
        public static Vector3 RandInsideCirclePoint(Vector3 center, float radius)
        {
            Vector2 radom_pos = Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(radom_pos.x, 0, radom_pos.y);
            return pos;
        }

        public static Vector3 RandOnSpherePoint(Vector3 center, float radius)
        {
            Vector3 radom_pos = Random.onUnitSphere * radius;
            Vector3 pos = center + radom_pos;
            return pos;
        }

        public static int GaussianRandomRange(int min, int max)
        {
            return GaussianRandom.Range(min, max);
        }
        #endregion

        #region GaussianRandom
        class GaussianRandom
        {
            // x是右边的面积，y是标准差的数量
            // 精确度0.01
            // 计算器: http://onlinestatbook.com/2/calculators/inverse_normal_dist.html
            static readonly float[] QTable ={
            3f,//0.00
            2.327f,
            2.054f,
            1.881f,
            1.751f,
            1.654f,
            1.555f,
            1.476f,
            1.405f,
            1.341f,
            1.282f,//0.10
            1.227f,
            1.175f,
            1.126f,
            1.08f,
            1.036f,
            0.994f,
            0.954f,
            0.915f,
            0.878f,
            0.841f,//0.20
            0.806f,
            0.772f,
            0.739f,
            0.706f,
            0.674f,
            0.643f,
            0.612f,
            0.582f,
            0.553f,
            0.524f,//0.30
            0.495f,
            0.467f,
            0.439f,
            0.412f,
            0.385f,
            0.358f,
            0.331f,
            0.305f,
            0.279f,
            0.253f,//0.40
            0.227f,
            0.202f,
            0.176f,
            0.151f,
            0.125f,
            0.1f,
            0.075f,
            0.05f,
            0.025f,
            0f,//0.50
        };

            // f是右边的面积
            // f>=0 && f<=0.5
            static float LookUpTableDirect(float f)
            {
                int x = Mathf.FloorToInt(f * 100);
                int xPlus = x + 1;
                float y = QTable[x];
                float yPlus = 0;
                // 考虑数组越界情况
                if (xPlus > 50)
                {
                    yPlus = 0;
                }
                else
                {
                    yPlus = QTable[xPlus];
                }
                //float t = f - x;
                return Mathf.Lerp(y, yPlus, f - x);
            }

            // f>=0 && f<=1
            static float LookUpTable(float f)
            {
                if (f <= 0.5f)
                {
                    return LookUpTableDirect(f);
                }
                else
                {
                    return -LookUpTableDirect(1 - f);
                }
            }

            // p是左边的面积
            // n是标注差数量
            static float GetNForP(float p)
            {
                return LookUpTable(1 - p);
            }

            public static float NextN
            {
                get
                {
                    return GetNForP(Random.value);
                }
            }

            // mean平均值
            // deviation标准差
            private static float GetGaussianRandom(float mean, float deviation)
            {
                float n = NextN;
                return n * deviation + mean;
            }

            // 假定范围为6个标准差
            public static int Range(int min, int max)
            {
                float mean = (max + min) / 2f;
                float deviation = (max - min) / 6f;
                return Mathf.Clamp(Mathf.RoundToInt(GetGaussianRandom(mean, deviation)), min, max);
            }
        }
        #endregion
    }
}