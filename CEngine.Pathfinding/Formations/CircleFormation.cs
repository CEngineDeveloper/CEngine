using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding.Formations
{

    /// <summary>
    /// Formation that positions units in a circle
    /// with specified angle and spacing between units.
    /// </summary>
    struct CircleFormation : IFormation
    {

        private float spacing;
        private float circleAngle;

        /// <summary>
        /// Instantiates circle formation.
        /// </summary>
        /// <param name="spacing">Specifies spacing between units in cricle</param>
        /// <param name="circleAngle">Specifies angle for units to be placed,
        /// 360 degree means that the units will go entire path around the circle
        /// and 180 degree angle means that only half of the circle will be formed.</param>
        public CircleFormation(float spacing, float circleAngle = 360f)
        {
            this.spacing = spacing;
            this.circleAngle = Mathf.Clamp(circleAngle, 0f, 360f);
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            // If there aren't enough points to start with the circle,
            // return the list with zero vector so that position is the target.
            if (unitCount <= 1)
            {
                return new List<Vector3>() { Vector3.zero };
            }

            List<Vector3> unitPositions = new List<Vector3>();
            float x, y;
            float angle = 0f;

            var angleIncrement = circleAngle / unitCount;
            var a = angleIncrement / 2;
            var radius = (spacing / 2) / Mathf.Sin(a * Mathf.Deg2Rad);

            for (int i = 0; i < unitCount; i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                unitPositions.Add(new Vector3(x, 0, y));

                angle += angleIncrement;
            }

            return unitPositions;
        }

    }

}