using UnityEngine;
using System.Collections.Generic;

namespace TGS {

    public partial class TerrainGridSystem : MonoBehaviour {

        /// <summary>
        /// Moves a given game object from current position to the center of a destination cell specified by row and column
        /// </summary>
        /// <param name="o">The game object</param>
        /// <param name="cellIndex">Index of the destination cell</param>
        /// <param name="velocity">Speed in meters per seconds. A value of 0 moves the gameobject immediately to the destination.</param>
        /// <param name="elevation">Optional offset from the grid surface</param>
        public GridMove MoveTo(GameObject o, int row, int column, float velocity = 0, float elevation = 0)
        {
            int destinationCellIndex = CellGetIndex(row, column);
            return MoveTo(o, destinationCellIndex, velocity, elevation);
        }

        /// <summary>
        /// Moves a given game object from current position to the center of a destination cell specified by index
        /// </summary>
        /// <param name="o">The game object</param>
        /// <param name="cellIndex">Index of the destination cell</param>
        /// <param name="velocity">Speed in meters per seconds. A value of 0 moves the gameobject immediately to the destination.</param>
        /// <param name="elevation">Optional offset from the grid surface</param>
        public GridMove MoveTo(GameObject o, int cellIndex, float velocity = 0, float elevation = 0)
        {
            List<int> positions = new List<int>();
            positions.Add(cellIndex);
            return MoveTo(o, positions, velocity, elevation);
        }

        /// <summary>
        /// Moves a given game object from current position to the center of a destination cell specified by row and column
        /// </summary>
        /// <param name="o">The game object</param>
        /// <param name="cellIndex">Index of the destination cell</param>
        /// <param name="velocity">Speed in meters per seconds. A value of 0 moves the gameobject immediately to the destination.</param>
        /// <param name="elevation">Optional offset from the grid surface</param>
        public GridMove MoveTo(GameObject o, List<int> positions, float velocity = 0, float elevation = 0)
        {
            GridMove mv = o.GetComponent<GridMove>();
            if (mv == null)
            {
                mv = o.AddComponent<GridMove>();
            }
            mv.grid = this;
            mv.positions = positions;
            mv.velocity = velocity;
            mv.elevation = elevation;
            mv.Begin();
            return mv;
        }

        /// <summary>
        /// Pauses a moving object
        /// </summary>
        public void MovePause(GameObject o)
        {
            GridMove mv = o.GetComponent<GridMove>();
            if (mv != null)
            {
                mv.enabled = false;
            }
        }


        /// <summary>
        /// Pauses or resumes a moving object
        /// </summary>
        public void MovePauseToggle(GameObject o)
        {
            GridMove mv = o.GetComponent<GridMove>();
            if (mv != null)
            {
                mv.enabled = !mv.enabled;
            }
        }


        /// <summary>
        /// Resumes movement of an object
        /// </summary>
        public void MoveResume(GameObject o)
        {
            GridMove mv = o.GetComponent<GridMove>();
            if (mv != null)
            {
                mv.enabled = true;
            }
        }


        /// <summary>
        /// Cancels movement of an object
        /// </summary>
        public void MoveCancel(GameObject o)
        {
            GridMove mv = o.GetComponent<GridMove>();
            if (mv != null)
            {
                DestroyImmediate(mv);
            }
        }



    }
}