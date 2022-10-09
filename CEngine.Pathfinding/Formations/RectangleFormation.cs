using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Pathfinding.Formations
{

    /// <summary>
    /// Formation that positions units in a rectangle
    /// with specified spacing and maximal column count.
    /// </summary>
    public struct RectangleFormation : IFormation
    {

        /// <summary>
        /// Returns the column count which represents the max
        /// unit number in a single row.
        /// </summary>
        public int ColumnCount { get; private set; }

        private float spacing;
        private bool centerUnits;

        /// <summary>
        /// Instantiates rectangle formation.
        /// </summary>
        /// <param name="columnCount">Maximal number of columns per row (there
        /// are less rows if number of units is smaller than this number).</param>
        /// <param name="spacing">Specifies spacing between units.</param>
        /// <param name="centerUnits">Specifies if units should be centered if
        /// they do not fill the full space of the row.</param>
        public RectangleFormation(
            int columnCount,
            float spacing,
            bool centerUnits = true)
        {
            this.ColumnCount = columnCount;
            this.spacing = spacing;
            this.centerUnits = centerUnits;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();
            var unitsPerRow = Mathf.Min(ColumnCount, unitCount);
            float offset = (unitsPerRow - 1) * spacing / 2f;
            float x, y, column;

            for (int row = 0; unitPositions.Count < unitCount; row++)
            {
                // Check if centering is enabled and if row has less than maximum
                // allowed units within the row.
                var firstIndexInRow = row * ColumnCount;
                if (centerUnits &&
                    row != 0 &&
                    firstIndexInRow + ColumnCount > unitCount)
                {
                    // Alter the offset to center the units that do not fill the row
                    var emptySlots = firstIndexInRow + ColumnCount - unitCount;
                    offset -= emptySlots / 2f * spacing;
                }

                for (column = 0; column < ColumnCount; column++)
                {
                    if (firstIndexInRow + column < unitCount)
                    {
                        x = column * spacing - offset;
                        y = row * spacing;
                        unitPositions.Add(new Vector3(x, 0, -y));
                    }
                    else
                    {
                        return unitPositions;
                    }
                }
            }

            return unitPositions;
        }

    }

}