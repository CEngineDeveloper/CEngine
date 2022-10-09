using System.Collections;
using UnityEngine;
namespace CYM.Cam
{
    public class SlowMotion : BaseMono
    {

        public float slowMotionTimeScale = 0.5f;

        public void Do(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(SlowMotionRoutine(duration));
        }

        //slow motion delay
        IEnumerator SlowMotionRoutine(float duration)
        {

            //set timescale
            Time.timeScale = slowMotionTimeScale;

            //wait a moment...
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < (startTime + duration))
            {
                yield return null;
            }

            //reset timescale
            Time.timeScale = 1;
        }
    }
}
