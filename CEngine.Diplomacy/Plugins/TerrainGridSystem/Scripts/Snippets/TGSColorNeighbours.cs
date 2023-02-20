using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS {
	[AddComponentMenu ("Kronnect/Terrain Grid System/TGS Color Neighbours")]
	public class TGSColorNeighbours : TGSSnippetBase {

		public int radius;
		public Color color = Color.white;

		protected override void Configure () {
			instructions = "Color cells within n-cells distance.";
			supportsTweening = false;
		}


		protected override void Execute (float t) {
			Cell cell = tgs.CellGetAtPosition (transform.position, true);
			if (cell == null) {
				return;
			}
			List<int> indices = tgs.CellGetNeighbours (cell, radius);
			indices.Add (cell.index);
			tgs.CellSetColor (indices, color);
		}



	}

}