using UnityEngine;

namespace CYM.AI
{
    public class AIWanderPosFinder
    {
        public Vector3 CachePos { get; private set; }
        Vector3 centerPos;
        MonoBehaviour Mono;

        public AIWanderPosFinder(MonoBehaviour mono, float minWalkDistance, float maxWalkDistance)
        {
            if (mono == null)
            {
                throw new System.ArgumentNullException();
            }

            Mono = mono;
            this.centerPos = Mono.transform.position;
            SetWanderRange(minWalkDistance, maxWalkDistance);
        }

        public void SetCenter(Vector3 pos)
        {
            centerPos = pos;
        }

        float MaxWalkDistance
        {
            get;
            set;
        }

        float MinWalkDistance
        {
            get;
            set;
        }

        public void SetWanderRange(float min, float max)
        {
            if (min > max)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            MinWalkDistance = min;
            MaxWalkDistance = max;
        }

        public Vector3 GetNewTarget()
        {
            Vector2 rc = Random.insideUnitCircle;
            Vector3 r = new Vector3(rc.x, 0, rc.y);
            r = Random.Range(MinWalkDistance, MaxWalkDistance) * r.normalized;
            Vector3 result = r + centerPos;
            return result;
        }

        public void GeneratePos()
        {
            CachePos = GetNewTarget();
        }
    }
}
