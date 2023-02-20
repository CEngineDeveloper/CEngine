using UnityEngine;

namespace TGS {

    public partial class TerrainGridSystem : MonoBehaviour {

		[SerializeField]
		bool _enableRectangleSelection;

		public bool enableRectangleSelection {
			get { return _enableRectangleSelection; }
			set {
				if (_enableRectangleSelection != value) {
					_enableRectangleSelection = value; 
					CheckRectangleSelectionObject ();
				}
			}
		}

	}
}

