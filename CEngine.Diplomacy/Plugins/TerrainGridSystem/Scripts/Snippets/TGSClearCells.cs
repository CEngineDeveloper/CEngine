using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS {
	[AddComponentMenu ("Kronnect/Terrain Grid System/TGS Clear Cells")]
	public class TGSClearCells : TGSSnippetBase {

		public bool clearAll;
		public int radius;

		protected override void Configure () {
			instructions = "Remove colors on cells.";
			supportsTweening = false;
		}


		protected override void Execute (float t) {
			if (clearAll) {
				tgs.ClearAll ();
			} else {
				Cell cell = tgs.CellGetAtPosition (transform.position, true);
				if (cell == null) {
					return;
				}
				List<int> indices = tgs.CellGetNeighbours (cell, radius);
				indices.Add (cell.index);
				tgs.CellClear (indices);
			}
		}



	}

}