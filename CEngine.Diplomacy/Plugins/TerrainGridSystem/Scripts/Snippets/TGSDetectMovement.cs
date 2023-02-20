using UnityEngine;

namespace TGS {
	[AddComponentMenu ("Kronnect/Terrain Grid System/TGS Detect Movement")]
	public class TGSDetectMovement : TGSSnippetBase {

		Vector3 oldPosition;

		protected override void Configure() {
			instructions = "Awakes other snippets when this gameobject moves.";
			hideOptions = true;
			oldPosition = transform.position;
		}

		void LateUpdate() {
			execute = ExecutionEvent.Immediate;
			if (transform.position != oldPosition) {
				oldPosition = transform.position;
				PingSnippets ();
			}

		}


	}

}