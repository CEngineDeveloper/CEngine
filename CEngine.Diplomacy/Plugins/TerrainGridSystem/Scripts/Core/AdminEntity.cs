using UnityEngine;
using TGS.Geom;

namespace TGS {
	public class AdminEntity : IAdmin {

		/// <summary>
		/// Optional entity name.
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Unscaled center. Ranges from -0.5, -0.5 to 0.5, 0.5.
		/// </summary>
		public Vector2 center;

		/// <summary>
		/// Original entity center with applied offset & scale
		/// </summary>
		public Vector2 scaledCenter;

		public Vector2 GetScaledCenter() { return scaledCenter; }

		public bool borderVisible { get; set; }
	
		/// <summary>
		/// Used internally to control incremental updates
		/// </summary>
		public bool isDirty { get; set; }

		JSONObject _attrib;

		/// <summary>
        /// User defined attributes
        /// </summary>
		public virtual JSONObject attrib {
			get {
				if (_attrib == null) _attrib = new JSONObject();
				return _attrib;
			}
		}

		public virtual bool visible { get; set; }
	}
}
