using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS {

	public partial class Territory: AdminEntity {

		/// <summary>
        /// List of other territories sharing some border with this territory
        /// </summary>
		public readonly List<Territory> neighbours = new List<Territory>();

		/// <summary>
        /// List of physical regions. Usually territories have only one region, but they can be split by assigning cells to other territories
        /// </summary>
		public readonly List<Region> regions = new List<Region>();

		public readonly List<Cell> cells = new List<Cell>();
		public Color fillColor = Color.gray;

        /// <summary>
        /// Stores the frontier color for this territory. Do not use this field directly. Use TerritorySetFrontierColor() method to change a territory frontier color instead.
        /// </summary>
        public Color frontierColor = new Color(0, 0, 0, 0);

		public bool neutral { get; set; }

		/// <summary>
        /// The reference to custom territory borders if drawn
        /// </summary>
		public GameObject customFrontiersGameObject;

        /// <summary>
        /// Returns true if thi territory has no regions
        /// </summary>
        public bool isEmpty { get { return regions.Count == 0 || regions[0].points == null || regions[0].points.Count == 0; } }

		public Territory() : this("") {
		}

		/// <summary>
        /// Returns the centroid of the territory. A centroid is a better center for a polygon.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid() {
			return regions.Count > 0 ? regions[0].centroid : Vector2.zero;
        }


        /// <summary>
        /// Returns a better centroid of the territory.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetBetterCentroid() {
            return regions.Count > 0 ? regions[0].betterCentroid : Vector2.zero;
        }

        public Territory(string name) {
			this.name = name;
			visible = true;
			borderVisible = true;
        }

	}

}