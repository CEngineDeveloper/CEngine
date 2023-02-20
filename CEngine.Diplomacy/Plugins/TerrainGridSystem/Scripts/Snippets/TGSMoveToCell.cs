using UnityEngine;

namespace TGS {
	[AddComponentMenu ("Kronnect/Terrain Grid System/TGS Move To Cell")]
	public class TGSMoveToCell : TGSSnippetBase {

		public int row;
		public int column;

		Vector3 startPosition, endPosition;

		protected override void Configure() {
			instructions = "Move this gameobject to specified cell with options.";
			supportsTweening = true;
		}


		protected override bool Prepare() {
			int cellIndex = tgs.CellGetIndex (row, column);
			startPosition = transform.position;
			if (cellIndex >= 0) {
				endPosition = tgs.CellGetPosition (cellIndex);
				return true;
			} else {
				Debug.Log ("No cell found at row " + row + ", column " + column);
				return false;
			}
		}

		protected override void Execute (float t) {
			transform.position = Vector3.Lerp (startPosition, endPosition, Tween (t));
		}

	}

}