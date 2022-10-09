using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    //数学计算通用类
    public partial class MathUtil
    {
        #region switch direction
        public static bool IsLeft(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Cross(self.Forward, target.Pos).y <= 0;
        }
        public static bool IsRight(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Cross(self.Forward, target.Pos).y > 0;
        }
        // 是否方向相同,m==0表示2个向量垂直。m<0表示2个向量角度>90度。m>0表示2个向量角度<90度。
        public static bool IsSameDirect(Vector3 dir1, Vector3 dir2, float val = 0.5f)
        {
            return Vector3.Dot(dir1, dir2) >= val;
        }
        // 是否方向不同,m==0表示2个向量垂直。m<0表示2个向量角度>90度。m>0表示2个向量角度<90度。
        public static bool IsDiffDirect(Vector3 dir1, Vector3 dir2, float val = 0.0f)
        {
            return Vector3.Dot(dir1, dir2) <= val;
        }
        // 是否面对
        public static bool IsFace(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Dot(self.Forward, target.Pos - self.Pos) >= 0 ? true : false;
        }
        // 是否面对
        public static bool IsFace(BaseCoreMono self, Vector3 target)
        {
            return Vector3.Dot(self.Forward, target - self.Pos) >= 0 ? true : false;
        }
        #endregion

        #region misc
        public static float AutoSqrDistance(Vector3 a, Vector3 b)
        {
            if (IsOrthographic())
                return MathUtil.XYSqrDistance(a, b);
            return MathUtil.XZSqrDistance(a, b);

            bool IsOrthographic()
            {
                if (BaseGlobal.MainCamera == null)
                    return false;
                return BaseGlobal.MainCamera.orthographic;
            }
        }
        #endregion

        #region const
        public const float TAU = 6.28318530717959f;
        public const float PI = 3.14159265359f;
        public const float E = 2.71828182846f;
        public const float GOLDEN_RATIO = 1.61803398875f;
        public const float SQRT2 = 1.41421356237f;
        public const float Infinity = Single.PositiveInfinity;
        public const float NegativeInfinity = Single.NegativeInfinity;
        public const float Deg2Rad = TAU / 360f;
        public const float Rad2Deg = 360f / TAU;
        public static readonly float Epsilon = UnityEngineInternal.MathfInternal.IsFlushToZeroEnabled ? UnityEngineInternal.MathfInternal.FloatMinNormal : UnityEngineInternal.MathfInternal.FloatMinDenormal;
        #endregion

        #region distance
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            return c.sqrMagnitude;
        }
        public static float XZSqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.y = 0;
            return c.sqrMagnitude;
        }

        public static float XZDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.y = 0;
            return c.magnitude;
        }
        public static float Distance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            return c.magnitude;
        }
        public static float XYDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.z = 0;
            return c.magnitude;
        }
        public static float XYSqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.z = 0;
            return c.sqrMagnitude;
        }
        #endregion

        #region Approximately
        // 两个向量是否近似
        public static bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                Mathf.Approximately(a.y, b.y) &&
                Mathf.Approximately(a.z, b.z);
        }
        public static bool Approximately(Vector3 a, Vector3 b, float val)
        {
            return Mathf.Abs(a.x - b.x) <= val &&
                Mathf.Abs(a.y - b.y) <= val &&
                Mathf.Abs(a.z - b.z) <= val;
        }
        public static bool Approximately(float a, float b, float val)
        {
            return Mathf.Abs(a - b) <= val;
        }
        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }
        public static bool ApproximatelyXZ(Vector3 a, Vector3 b, float val)
        {
            return Mathf.Abs(a.x - b.x) <= val &&
                Mathf.Abs(a.z - b.z) <= val;
        }
        #endregion

        #region round
        public static float Round(float val, float decimalCount = 2)
        {
            float endval = Mathf.Pow(10, decimalCount);
            return (float)(Mathf.Round(val * endval)) / endval;
        }
        public static float Round(float val, int count)
        {
            return (float)System.Math.Round(val, count);
        }
        #endregion

        #region clamp
        public static float Clamp0(float val)
        {
            return Mathf.Clamp(val, 0.0f, float.MaxValue);
        }
        public static int Clamp0(int val)
        {
            return Mathf.Clamp(val, 0, int.MaxValue);
        }
        public static float Clamp(float val, float min, float max)
        {
            return Mathf.Clamp(val, min, max);
        }
        public static int Clamp(int val, int min, int max)
        {
            return Mathf.Clamp(val, min, max);
        }
        public static float Clamp01(float value)
        {
            if (value < 0f) value = 0f;
            if (value > 1f) value = 1f;
            return value;
        }

        public static float ClampNeg1to1(float value)
        {
            if (value < -1f) value = -1f;
            if (value > 1f) value = 1f;
            return value;
        }
        #endregion

        #region Compare
        // 比较两个位置点是否相近
        public static bool PosCompare(Vector3 a, Vector3 b, float k, bool invers = false)
        {
            if (!invers)
            {
                return Mathf.Abs(a.x - b.x) <= k &&
                Mathf.Abs(a.y - b.y) <= k &&
                Mathf.Abs(a.z - b.z) <= k;
            }
            else
            {
                return Mathf.Abs(a.x - b.x) >= k ||
                Mathf.Abs(a.y - b.y) >= k ||
                Mathf.Abs(a.z - b.z) >= k;
            }
        }
        // 比较两个位置点是否相近
        public static bool XZPosCompare(Vector3 a, Vector3 b, float k, bool invers = false)
        {
            if (!invers)
            {
                return Mathf.Abs(a.x - b.x) <= k &&
                    Mathf.Abs(a.z - b.z) <= k;
            }
            else
            {
                return Mathf.Abs(a.x - b.x) >= k ||
                    Mathf.Abs(a.z - b.z) >= k;
            }
        }
        #endregion

        #region spline
        public static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime)
        {
            // References used:
            // p.266 GemsV1
            //
            // tension is often set to 0.5 but you can use any reasonable value:
            // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
            //
            // bias and tension controls:
            // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

            float percentComplete = elapsedTime;
            float percentCompleteSquared = percentComplete * percentComplete;
            float percentCompleteCubed = percentCompleteSquared * percentComplete;

            return
                previous * (-0.5F * percentCompleteCubed +
                            percentCompleteSquared -
                            0.5F * percentComplete) +

                start *
                (1.5F * percentCompleteCubed +
                 -2.5F * percentCompleteSquared + 1.0F) +

                end *
                (-1.5F * percentCompleteCubed +
                 2.0F * percentCompleteSquared +
                 0.5F * percentComplete) +

                next *
                (0.5F * percentCompleteCubed -
                 0.5F * percentCompleteSquared);
        }

        /// <summary>Returns a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
        public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = 1 - t;
            return t2 * t2 * t2 * p0 + 3 * t2 * t2 * t * p1 + 3 * t2 * t * t * p2 + t * t * t * p3;
        }

        /// <summary>Returns the derivative for a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
        public static Vector3 CubicBezierDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = 1 - t;
            return 3 * t2 * t2 * (p1 - p0) + 6 * t2 * t * (p2 - p1) + 3 * t * t * (p3 - p2);
        }

        /// <summary>Returns the second derivative for a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
        public static Vector3 CubicBezierSecondDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = 1 - t;
            return 6 * t2 * (p2 - 2 * p1 + p0) + 6 * t * (p3 - 2 * p2 + p1);
        }
        #endregion

        #region angle
        // 转换角度
        static public float WrapAngle(float a)
        {
            while (a < -180.0f) a += 360.0f;
            while (a > 180.0f) a -= 360.0f;
            return a;
        }
        public static bool IsOdd(int num)
        {
            return (num & 1) == 1;
        }
        public static float CheckAngle(float value)  // 将大于180度角进行以负数形式输出
        {
            float angle = value - 180;

            if (angle > 0)
            {
                return angle - 180;
            }

            if (value == 0)
            {
                return 0;
            }

            return angle + 180;
        }
        #endregion

        #region lerp
        //curve calculation for ease out effect
        public static float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }
        //curve calculation for ease in effect
        public static float Coserp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }
        //curve calculation for easing at start + end
        public static float CoSinLerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }
        #endregion

        #region array
        public static T GetSafeArray<T>(T[] array, int index)
        {
            if (array == null) return default;
            if (array.Length <= 0) return default;
            if (index >= array.Length) return default;
            return array[index];
        }
        public static T GetSafeArray<T>(List<T> array, int index)
        {
            if (array == null) return default;
            if (array.Count <= 0) return default;
            if (index >= array.Count) return default;
            return array[index];
        }
        #endregion

        #region direction
        public static Direct UpOrDown(Vector3 up, Vector3 forward2)
        {
            float val = Vector3.Dot(up, forward2);
            return val > 0 ? Direct.Up : Direct.Down;
        }
        public static Direct UpOrDown(BaseMono self, BaseMono target)
        {
            if (self == null) throw new System.Exception("self 为空");
            if (target == null) throw new System.Exception("target 为空");
            return UpOrDown(self.Trans.forward, target.Pos - self.Pos);
        }
        public static Direct LeftOrRight(Vector3 forward, Vector3 forward2)
        {
            float val = Vector3.Cross(forward, forward2).y;
            return val > 0 ? Direct.Right : Direct.Left;
        }
        public static Direct LeftOrRight(BaseMono self, BaseMono target)
        {
            if (self == null) throw new System.Exception("self 为空");
            if (target == null) throw new System.Exception("target 为空");
            return LeftOrRight(self.Trans.forward, target.Pos - self.Pos);
        }
        public static Direct Invert(Direct dir)
        {
            return (Direct)(-(int)dir);
        }
        #endregion

        #region other
        /// <summary>
        /// 平衡0-1,p1 必须正，p2 必须负
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float Auncel(float p1, float p2)
        {
            return (float)(p1 + p2) / (p1 - p2) / 2 + 0.5f;
        }
        /// <summary>
        /// 平衡0-1,p1 必须正，p2 必须正
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float Libra(float p1, float p2)
        {
            return Auncel(p1, -p2);
        }
        #endregion

        #region tween
        public static float Tween(TweenType tweenType, float start, float end, float time)
        {
            if (tweenType == TweenType.immediate || time == 0)
            {
                // if the easing is immediate or the time is zero, just jump to the end position
                return end;
            }
            else
            {
                switch (tweenType)
                {
                    case TweenType.linear: return linear(start, end, time);
                    case TweenType.spring: return spring(start, end, time);
                    case TweenType.easeInQuad: return easeInQuad(start, end, time);
                    case TweenType.easeOutQuad: return easeOutQuad(start, end, time);
                    case TweenType.easeInOutQuad: return easeInOutQuad(start, end, time);
                    case TweenType.easeInCubic: return easeInCubic(start, end, time);
                    case TweenType.easeOutCubic: return easeOutCubic(start, end, time);
                    case TweenType.easeInOutCubic: return easeInOutCubic(start, end, time);
                    case TweenType.easeInQuart: return easeInQuart(start, end, time);
                    case TweenType.easeOutQuart: return easeOutQuart(start, end, time);
                    case TweenType.easeInOutQuart: return easeInOutQuart(start, end, time);
                    case TweenType.easeInQuint: return easeInQuint(start, end, time);
                    case TweenType.easeOutQuint: return easeOutQuint(start, end, time);
                    case TweenType.easeInOutQuint: return easeInOutQuint(start, end, time);
                    case TweenType.easeInSine: return easeInSine(start, end, time);
                    case TweenType.easeOutSine: return easeOutSine(start, end, time);
                    case TweenType.easeInOutSine: return easeInOutSine(start, end, time);
                    case TweenType.easeInExpo: return easeInExpo(start, end, time);
                    case TweenType.easeOutExpo: return easeOutExpo(start, end, time);
                    case TweenType.easeInOutExpo: return easeInOutExpo(start, end, time);
                    case TweenType.easeInCirc: return easeInCirc(start, end, time);
                    case TweenType.easeOutCirc: return easeOutCirc(start, end, time);
                    case TweenType.easeInOutCirc: return easeInOutCirc(start, end, time);
                    case TweenType.easeInBounce: return easeInBounce(start, end, time);
                    case TweenType.easeOutBounce: return easeOutBounce(start, end, time);
                    case TweenType.easeInOutBounce: return easeInOutBounce(start, end, time);
                    case TweenType.easeInBack: return easeInBack(start, end, time);
                    case TweenType.easeOutBack: return easeOutBack(start, end, time);
                    case TweenType.easeInOutBack: return easeInOutBack(start, end, time);
                    case TweenType.easeInElastic: return easeInElastic(start, end, time);
                    case TweenType.easeOutElastic: return easeOutElastic(start, end, time);
                    case TweenType.easeInOutElastic: return easeInOutElastic(start, end, time);
                }
            }
            return 0;
        }
        private static float linear(float start, float end, float val)
        {
            return Mathf.Lerp(start, end, val);
        }

        private static float spring(float start, float end, float val)
        {
            val = Mathf.Clamp01(val);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
            return start + (end - start) * val;
        }

        private static float easeInQuad(float start, float end, float val)
        {
            end -= start;
            return end * val * val + start;
        }

        private static float easeOutQuad(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }

        private static float easeInOutQuad(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val + start;
            val--;
            return -end / 2 * (val * (val - 2) - 1) + start;
        }

        private static float easeInCubic(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val + start;
        }

        private static float easeOutCubic(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val + 1) + start;
        }

        private static float easeInOutCubic(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val + 2) + start;
        }

        private static float easeInQuart(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val + start;
        }

        private static float easeOutQuart(float start, float end, float val)
        {
            val--;
            end -= start;
            return -end * (val * val * val * val - 1) + start;
        }

        private static float easeInOutQuart(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val + start;
            val -= 2;
            return -end / 2 * (val * val * val * val - 2) + start;
        }

        private static float easeInQuint(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val * val + start;
        }

        private static float easeOutQuint(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val * val * val + 1) + start;
        }

        private static float easeInOutQuint(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val * val * val + 2) + start;
        }

        private static float easeInSine(float start, float end, float val)
        {
            end -= start;
            return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
        }

        private static float easeOutSine(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
        }

        private static float easeInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
        }

        private static float easeInExpo(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
        }

        private static float easeOutExpo(float start, float end, float val)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
        }

        private static float easeInOutExpo(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
            val--;
            return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
        }

        private static float easeInCirc(float start, float end, float val)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
        }

        private static float easeOutCirc(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * Mathf.Sqrt(1 - val * val) + start;
        }

        private static float easeInOutCirc(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
            val -= 2;
            return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
        }

        private static float easeInBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d - val) + start;
        }

        private static float easeOutBounce(float start, float end, float val)
        {
            val /= 1f;
            end -= start;
            if (val < (1 / 2.75f))
            {
                return end * (7.5625f * val * val) + start;
            }
            else if (val < (2 / 2.75f))
            {
                val -= (1.5f / 2.75f);
                return end * (7.5625f * (val) * val + .75f) + start;
            }
            else if (val < (2.5 / 2.75))
            {
                val -= (2.25f / 2.75f);
                return end * (7.5625f * (val) * val + .9375f) + start;
            }
            else
            {
                val -= (2.625f / 2.75f);
                return end * (7.5625f * (val) * val + .984375f) + start;
            }
        }

        private static float easeInOutBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            if (val < d / 2) return easeInBounce(0, end, val * 2) * 0.5f + start;
            else return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
        }

        private static float easeInBack(float start, float end, float val)
        {
            end -= start;
            val /= 1;
            float s = 1.70158f;
            return end * (val) * val * ((s + 1) * val - s) + start;
        }

        private static float easeOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val = (val / 1) - 1;
            return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
        }

        private static float easeInOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val /= .5f;
            if ((val) < 1)
            {
                s *= (1.525f);
                return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
            }
            val -= 2;
            s *= (1.525f);
            return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
        }

        private static float easeInElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;
            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
            val = val - 1;
            return -(a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float easeOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        private static float easeInOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / (d / 2);
            if (val == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (val < 1)
            {
                val = val - 1;
                return -0.5f * (a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
            }
            val = val - 1;
            return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }
        #endregion

        #region Math operations
        public static float Sqrt(float value) => (float)Math.Sqrt(value);
        public static float Pow(float @base, float exponent) => (float)Math.Pow(@base, exponent);
        public static float Exp(float power) => (float)Math.Exp(power);
        public static float Log(float value, float @base) => (float)Math.Log(value, @base);
        public static float Log(float value) => (float)Math.Log(value);
        public static float Log10(float value) => (float)Math.Log10(value);
        #endregion

        #region Trig
        public static float Sin(float angRad) => (float)Math.Sin(angRad);
        public static float Cos(float angRad) => (float)Math.Cos(angRad);
        public static float Tan(float angRad) => (float)Math.Tan(angRad);
        public static float Asin(float value) => (float)Math.Asin(value);
        public static float Acos(float value) => (float)Math.Acos(value);
        public static float Atan(float value) => (float)Math.Atan(value);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Csc(float x) => 1f / (float)Math.Sin(x);
        public static float Sec(float x) => 1f / (float)Math.Cos(x);
        public static float Cot(float x) => 1f / (float)Math.Tan(x);
        public static float Ver(float x) => 1 - (float)Math.Cos(x);
        public static float Cvs(float x) => 1 - (float)Math.Sin(x);
        public static float Crd(float x) => 2 * (float)Math.Sin(x / 2);
        #endregion

        #region Absolute values
        public static float Abs(float value) => Math.Abs(value);
        public static int Abs(int value) => Math.Abs(value);
        public static Vector2 Abs(Vector2 v) => new Vector2(Abs(v.x), Abs(v.y));
        public static Vector3 Abs(Vector3 v) => new Vector3(Abs(v.x), Abs(v.y), Abs(v.z));
        public static Vector4 Abs(Vector4 v) => new Vector4(Abs(v.x), Abs(v.y), Abs(v.z), Abs(v.w));
        #endregion

        #region Min & Max
        public static float Min(float a, float b) => a < b ? a : b;
        public static float Min(float a, float b, float c) => Min(Min(a, b), c);
        public static float Min(float a, float b, float c, float d) => Min(Min(a, b), Min(c, d));
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Max(float a, float b, float c) => Max(Max(a, b), c);
        public static float Max(float a, float b, float c, float d) => Max(Max(a, b), Max(c, d));
        public static int Min(int a, int b) => a < b ? a : b;
        public static int Min(int a, int b, int c) => Min(Min(a, b), c);
        public static int Max(int a, int b) => a > b ? a : b;
        public static int Max(int a, int b, int c) => Max(Max(a, b), c);

        public static float Min(params float[] values) => Mathf.Min(values);
        public static float Max(params float[] values) => Mathf.Max(values);
        public static int Min(params int[] values) => Mathf.Min(values);
        public static int Max(params int[] values) => Mathf.Max(values);
        #endregion

        #region Rounding
        public static float Sign(float value) => value >= 0f ? 1f : -1f;
        public static float Floor(float value) => (float)Math.Floor(value);
        public static Vector2 Floor(Vector2 value) => new Vector2((float)Math.Floor(value.x), (float)Math.Floor(value.y));
        public static Vector3 Floor(Vector3 value) => new Vector3((float)Math.Floor(value.x), (float)Math.Floor(value.y), (float)Math.Floor(value.z));
        public static Vector4 Floor(Vector4 value) => new Vector4((float)Math.Floor(value.x), (float)Math.Floor(value.y), (float)Math.Floor(value.z), (float)Math.Floor(value.w));
        public static float Ceil(float value) => (float)Math.Ceiling(value);
        public static float Round(float value) => (float)Math.Round(value);
        public static int FloorToInt(float value) => (int)Math.Floor(value);
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);
        public static int RoundToInt(float value) => (int)Math.Round(value);
        #endregion

        #region Repeating
        public static float Frac(float x) => x - Floor(x);
        public static Vector2 Frac(Vector2 x) => x - Floor(x);
        public static Vector3 Frac(Vector3 x) => x - Floor(x);
        public static Vector4 Frac(Vector4 x) => x - Floor(x);
        public static float Repeat(float value, float length) => Clamp(value - Floor(value / length) * length, 0.0f, length);
        public static int Mod(int value, int length) => (value % length + length) % length; // modulo

        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Abs(t - length);
        }
        #endregion

        #region Smoothing & Curves
        public static float Smooth01(float x) => x * x * (3 - 2 * x);
        public static float Smoother01(float x) => x * x * x * (x * (x * 6 - 15) + 10);
        public static float SmoothCos01(float x) => Cos(x * PI) * -0.5f + 0.5f;

        public static float Gamma(float value, float absmax, float gamma)
        {
            bool negative = value < 0F;
            float absval = Abs(value);
            if (absval > absmax)
                return negative ? -absval : absval;

            float result = Pow(absval / absmax, gamma) * absmax;
            return negative ? -result : result;
        }
        #endregion

        #region Interpolation & Remapping
        public static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
        public static float InverseLerpClamped(float a, float b, float value) => Clamp01((value - a) / (b - a));
        public static float Lerp(float a, float b, float t) => (1f - t) * a + t * b;

        public static float LerpClamped(float a, float b, float t)
        {
            t = Clamp01(t);
            return (1f - t) * a + t * b;
        }

        public static float Eerp(float a, float b, float t) => Mathf.Pow(a, 1 - t) * Mathf.Pow(b, t);
        public static float InverseEerp(float a, float b, float v) => Mathf.Log(a / v) / Mathf.Log(a / b);

        public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t) => new Vector2(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y));
        public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 v) => (v - a) / (b - a);

        public static Vector2 Remap(Rect iRect, Rect oRect, Vector2 iPos)
        {
            return Remap(iRect.min, iRect.max, oRect.min, oRect.max, iPos);
        }

        public static Vector2 Remap(Vector2 iMin, Vector2 iMax, Vector2 oMin, Vector2 oMax, Vector2 value)
        {
            Vector2 t = InverseLerp(iMin, iMax, value);
            return Lerp(oMin, oMax, t);
        }

        public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = InverseLerp(iMin, iMax, value);
            return Lerp(oMin, oMax, t);
        }

        public static float RemapClamped(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = InverseLerpClamped(iMin, iMax, value);
            return Lerp(oMin, oMax, t);
        }

        public static float InverseLerpSmooth(float a, float b, float value) => Smooth01(Clamp01((value - a) / (b - a)));

        public static float LerpSmooth(float a, float b, float t)
        {
            t = Smooth01(Clamp01(t));
            return (1f - t) * a + t * b;
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Mathf.Abs(target - current) <= maxDelta)
                return target;
            return current + Mathf.Sign(target - current) * maxDelta;
        }
        #endregion

        #region Vector math
        public static float Determinant /*or Cross*/(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x; // 2D "cross product"
        public static Vector2 Dir(Vector2 from, Vector2 to) => (to - from).normalized;
        public static Vector3 Dir(Vector3 from, Vector3 to) => (to - from).normalized;
        public static Vector2 FromTo(Vector2 from, Vector2 to) => to - from;
        public static Vector3 FromTo(Vector3 from, Vector3 to) => to - from;
        public static Vector2 CenterPos(Vector2 a, Vector2 b) => (a + b) / 2f;
        public static Vector3 CenterPos(Vector3 a, Vector3 b) => (a + b) / 2f;
        public static Vector2 CenterDir(Vector2 aDir, Vector2 bDir) => (aDir + bDir).normalized;
        public static Vector3 CenterDir(Vector3 aDir, Vector3 bDir) => (aDir + bDir).normalized;
        public static Vector2 Rotate90CW(Vector2 v) => new Vector2(v.y, -v.x);
        public static Vector2 Rotate90CCW(Vector2 v) => new Vector2(-v.y, v.x);
        #endregion


        #region Angles & Rotation
        public static Vector2 AngToDir(float aRad) => new Vector2(Mathf.Cos(aRad), Mathf.Sin(aRad));
        public static float DirToAng(Vector2 dir) => Mathf.Atan2(dir.y, dir.x);


        public static float LerpAngle(float aRad, float bRad, float t)
        {
            float delta = Repeat((bRad - aRad), TAU);
            if (delta > PI)
                delta -= TAU;
            return aRad + delta * Clamp01(t);
        }
        #endregion


        #region coordinate shenanigans
        public static Vector2 SquareToDisc(Vector2 c)
        {
            float u = c.x * Sqrt(1 - (c.y * c.y) / 2);
            float v = c.y * Sqrt(1 - (c.x * c.x) / 2);
            return new Vector2(u, v);
        }

        public static Vector2 DiscToSquare(Vector2 c)
        {
            float u = c.x;
            float v = c.y;
            float u2 = c.x * c.x;
            float v2 = c.y * c.y;

            Vector2 n = new Vector2(1, -1);
            Vector2 p = new Vector2(2, 2) + n * (u2 - v2);
            Vector2 q = 2 * SQRT2 * c;
            Vector2 smolVec = Vector2.one * 0.0001f;
            Vector2 Sqrt(Vector2 noot) => new Vector2(Mathf.Sqrt(noot.x), Mathf.Sqrt(noot.y));
            return 0.5f * (Sqrt(Vector2.Max(smolVec, p + q)) - Sqrt(Vector2.Max(smolVec, p - q)));
        }
        #endregion

    }
}

