using System.Collections.Generic;
using UnityEngine;
namespace CYM.Cam
{
    [RequireComponent(typeof(Camera))]
    public class Shake : BaseMono
    {
        CoroutineHandle coroutine;
        public AnimationCurve camShakeY;
        public AnimationCurve camShakeX;
        public AnimationCurve camShakeZ;
        public float multiplier = 1f;
        public bool randomize; //randomizes the direction of the animationcurves by multiplying them with -1 or 1
        public float time = .5f;

        public void Do(float intensity)
        {
            coroutine = BaseGlobal.CommonCorouter.Run(DoShake(intensity));
        }

        IEnumerator<float> DoShake(float scale)
        {

            Vector3 rand = new Vector3(getRandomValue(), getRandomValue(), getRandomValue());
            scale *= multiplier;

            float t = 0;
            while (t < time)
            {
                if (randomize)
                {
                    transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale * rand.x, camShakeY.Evaluate(t) * scale * rand.y, camShakeZ.Evaluate(t) * scale * rand.z);
                }
                else
                {
                    transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale, camShakeY.Evaluate(t) * scale, camShakeZ.Evaluate(t) * scale);
                }

                t += Time.deltaTime / time;
                yield return Timing.WaitForOneFrame;
            }
            transform.localPosition = Vector3.zero;
        }

        //returns a value of -1 or 1
        int getRandomValue()
        {
            int[] i = { -1, 1 };
            return i[Random.Range(0, 2)];
        }
    }
}