using System;
using System.Collections.Generic;
using System.Text;

namespace TGS.PathFinding {

	public delegate float OnCellCross(TerrainGridSystem tgs, int cellIndex);

	public enum CellType
	{
		Box = 0,
		FlatTopHexagon = 1,
		PointyTopHexagon = 2,
		Irregular = 3
	}

	interface IPathFinder {

		HeuristicFormula Formula {
			get;
			set;
		}

		bool Diagonals {
			get;
			set;
		}

		float HeavyDiagonalsCost {
			get;
			set;
		}

        CellType CellShape
        {
			get;
			set;
		}

		float HeuristicEstimate {
			get;
			set;
		}

		int MaxSteps {
			get;
			set;
		}

		float MaxSearchCost {
			get;
			set;
		}

		int CellGroupMask {
			get;
			set;
		}

		bool IgnoreCanCrossCheck {
			get;
			set;
		}

		bool IgnoreCellCost {
			get;
			set;
		}

		bool IncludeInvisibleCells {
			get;
			set;
		}

        int MinClearance {
            get;
            set;
        }

        List<PathFinderNode> FindPath (TerrainGridSystem tgs, Cell start, Cell end, out float cost, bool evenLayout);

		void SetCalcMatrix (Cell[] grid);

		OnCellCross OnCellCross { get; set; }

	}
}
