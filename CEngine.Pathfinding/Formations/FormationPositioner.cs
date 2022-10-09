using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding.Formations
{

    /// <summary>
    /// Class responsible for providing unit positions in formation on
    /// a target position facing the respective angle.
    /// </summary>
    public class FormationPositioner
    {

        /// <summary>
        /// Returns aligned units formation positions that are facing the
        /// passed angle.
        /// </summary>
        /// <param name="unitCount">Amount of units in formation.</param>
        /// <param name="formation">Formation that units will position in.</param>
        /// <param name="targetPosition">Position of the formation.</param>
        /// <param name="targetAngle">Facing angle for the formation.</param>
        /// <returns>Returns aligned positions of the units in formation.</returns>
        public static List<Vector3> GetAlignedPositions(
            int unitCount,
            IFormation formation,
            Vector3 targetPosition,
            float targetAngle)
        {
            var positions = formation.GetPositions(unitCount);
            var pivot = targetPosition;
            var angleVector = new Vector3(0f, targetAngle, 0f);

            return positions.ConvertAll(pos =>
            {
                return RotatePointAroundPivot(
                    targetPosition + pos, pivot, angleVector);
            });
        }

        /// <summary>
        /// Finds new positions for the passed positions and the formation.
        /// If distance from current positions center is less than rotation
        /// threshold, units formation will not be rotated around the target.
        /// New rotation angle is calculated from center position of all current
        /// positions and the target positions.
        /// </summary>
        /// <param name="currentPositions">Current unit positions.</param>
        /// <param name="formation">Formation used on units</param>
        /// <param name="targetPosition">Position to where the units will be
        /// moved.</param>
        /// <param name="rotationThreshold">Threshold used to specify when the
        /// unit formation should be rotated around target position (pivot).</param>
        /// <returns>Returns list of the new unit positions and their new facing
        /// angle</returns>
        public static UnitsFormationPositions GetPositions(
            List<Vector3> currentPositions,
            IFormation formation,
            Vector3 targetPosition,
            float rotationThreshold = 4.0f)
        {

            if (currentPositions.Count == 0)
            {
                Debug.LogWarning("Cannot generate formation for an empty game object list.");
                return new UnitsFormationPositions(new List<Vector3>(), 0f);
            }

            // Get sum of all positions in order to get center of the objects.
            Vector3 sum = new Vector3();
            foreach (Vector3 pos in currentPositions)
                sum += pos;
            
            var centerPos = sum / currentPositions.Count;
            var direction = targetPosition - centerPos;
            float angle = 0;

            // Only if direction change is significant, it should rotate units
            // formation as well.
            if (direction.magnitude > rotationThreshold)
            {
                angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            }

            var formationPositions = GetAlignedPositions(
                currentPositions.Count, formation, targetPosition, angle);
            return new UnitsFormationPositions(formationPositions, angle);
        }

        #region Convenience

        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
            => Quaternion.Euler(angles) * (point - pivot) + pivot;

        #endregion

    }

    #region UnitsFormationPositions

    /// <summary>
    /// Data structure that represents the units new formation positions and angles.
    /// </summary>
    public struct UnitsFormationPositions
    {
        /// <summary>
        /// Specifies the new positions that units can move to new formation.
        /// </summary>
        public List<Vector3> UnitPositions;

        /// <summary>
        /// Specifies the units facing angle (loot at direction) for the new position.
        /// </summary>
        public float FacingAngle;

        public UnitsFormationPositions(
            List<Vector3> unitPositions,
            float finalRotation)
        {
            UnitPositions = unitPositions;
            FacingAngle = finalRotation;
        }
    }

    #endregion

}
