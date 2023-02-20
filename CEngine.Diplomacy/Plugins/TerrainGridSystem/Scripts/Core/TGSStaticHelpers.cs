using System.Collections.Generic;
using UnityEngine;

namespace TGS {

    public partial class TerrainGridSystem : MonoBehaviour {

        /// <summary>
        /// Global list of grids in the scene
        /// </summary>
        public static List<TerrainGridSystem> grids = new List<TerrainGridSystem>();

        /// <summary>
        /// Returns the reference to the grid which contains a position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static TerrainGridSystem GetGridAt(Vector3 position) {
            int count = grids.Count;
            for (int k = 0; k < count; k++) {
                TerrainGridSystem tgs = grids[k];
                if (tgs.Contains(position)) return tgs;
            }
            return null;
        }


    }
}