using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;
using System.Drawing;

namespace TGS {

	public enum CELL_SIDE {
		TopLeft = 0,
		Top = 1,
		TopRight = 2,
		BottomRight = 3,
		Bottom = 4,
		BottomLeft = 5,
		Left = 6,
		Right = 7
	}

	public enum CELL_DIRECTION {
		Exiting = 0,
		Entering = 1,
		Both = 2
	}

	public partial class Cell: AdminEntity {
      

        /// <summary>
        /// The index of the cell in the cells array
        /// </summary>
        public int index;

		/// <summary>
        /// Physical surface-related data
        /// </summary>
		public Region region { get; set; }

		/// <summary>
        /// Cells adjacent to this cell
        /// </summary>
		public readonly List<Cell> neighbours = new List<Cell>();

		/// <summary>
		/// The territory to which this cell belongs to. You can change it using CellSetTerritory method.
		/// WARNING: do not change this value directly, use CellSetTerritory instead.
		/// </summary>
		public short territoryIndex = -1;

        public override bool visible { get { return visibleSelf && visibleByRules; } set { visibleSelf = value; } }

        public bool visibleSelf { get; private set; }

        public bool visibleByRules = true;

		/// <summary>
        /// Distance to nearest blocking cell
        /// </summary>
		public byte clearance = 1;

		/// <summary>
		/// Optional value that can be set with CellSetTag. You can later get the cell quickly using CellGetWithTag method.
		/// </summary>
		public int tag;

		public ushort row, column;

        public string coordinates { get { return string.Format("row = {0}, column = {1}", column, row); } }

        /// <summary>
        /// If this cell blocks path finding.
        /// </summary>
        public bool canCross = true;

		float[] _crossCost;
		/// <summary>
		/// Used by pathfinding in Cell mode. Cost for crossing a cell for each side. Defaults to 1.
		/// </summary>
		/// <value>The cross cost.</value>
		public float[] crossCost {
			get { return _crossCost; }
			set { _crossCost = value; }
		}

		bool[] _blocksLOS;
		/// <summary>
		/// Used by specify if LOS is blocked across cell sides.
		/// </summary>
		/// <value>The cross cost.</value>
		public bool[] blocksLOS {
			get { return _blocksLOS; }
			set { _blocksLOS = value; }
		}

		
		/// <summary>
		/// Group for this cell. A different group can be assigned to use along with FindPath cellGroupMask argument.
		/// </summary>
		public int group = 1;

		/// <summary>
		/// Used internally to optimize certain algorithms
		/// </summary>
        [NonSerialized]
		public int iteration;


        /// <summary>
        /// Returns the centroid of the territory. The centroid always lays inside the polygon whereas the center is the geometric center of the enclosing rectangle and could fall outside an irregular polygon.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid() {
			return region.centroid;
        }

        public Cell (string name, Vector2 center) {
			this.name = name;
			this.center = center;
			visible = true;
			borderVisible = true;
		}

		public Cell () : this ("", Vector2.zero) {
		}

		public Cell (string name) : this (name, Vector2.zero) {
		}

		public Cell (Vector2 center) : this ("", center) {
		}

		/// <summary>
		/// Returns the highest crossing cost of all side of a cell
		/// </summary>
		public float GetSidesCost()
		{
			if (_crossCost == null) return 0;
            int crossCostLength = _crossCost.Length;
			float maxCost = 0;
            for (int k = 0; k < crossCostLength; k++) { if (_crossCost[k] > maxCost) maxCost = _crossCost[k]; }
			return maxCost;
        }

		/// <summary>
		/// Gets the side cross cost.
		/// </summary>
		/// <returns>The side cross cost.</returns>
		/// <param name="side">Side.</param>
		public float GetSideCrossCost(CELL_SIDE side) {
			if (_crossCost==null) return 0;
			return _crossCost [(int)side];
		}

		/// <summary>
		/// Assigns a crossing cost for a given hexagonal side
		/// </summary>
		/// <param name="side">Side.</param>
		/// <param name="cost">Cost.</param>
		public void SetSideCrossCost(CELL_SIDE side, float cost) {
			if (_crossCost==null) _crossCost = new float[8];
			_crossCost [(int)side] = cost;
		}

		/// <summary>
		/// Sets the same crossing cost for all sides of the hexagon.
		/// </summary>
		public void SetAllSidesCost(float cost) {
			if (_crossCost==null) _crossCost = new float[8];
			int crossCostLength = _crossCost.Length;
			for (int k=0;k<crossCostLength; k++) { _crossCost[k] = cost; }
		}

		/// <summary>
		/// Returns true if side is blocking LOS
		/// </summary>
		public bool GetSideBlocksLOS(CELL_SIDE side) {
			if (_blocksLOS==null) return false;
			return _blocksLOS[(int)side];
		}

		/// <summary>
		/// Assigns a crossing cost for a given hexagonal side
		/// </summary>
		/// <param name="side">Side.</param>
		public void SetSideBlocksLOS(CELL_SIDE side, bool blocks) {
			if (_blocksLOS==null) _blocksLOS = new bool[8];
			_blocksLOS[(int)side] = blocks;
		}


	}
}

