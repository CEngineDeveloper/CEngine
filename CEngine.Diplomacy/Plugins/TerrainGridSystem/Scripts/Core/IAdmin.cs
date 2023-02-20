using UnityEngine;

namespace TGS {
	public interface IAdmin {

		string name { get; set; }

		bool visible { get; set; }

		/// <summary>
		/// Used for incremental updates
		/// </summary>
		bool isDirty { get; set; }

		bool borderVisible { get; set; }

		JSONObject attrib { get; }

		Vector2 GetScaledCenter();

	}
}
