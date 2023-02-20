using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace TGS {

	public class DisposalManager {

		List<Object> disposeObjects;

		public DisposalManager() {
			disposeObjects = new List<Object>();
		}

		public void DisposeAll() {
			if (disposeObjects == null) return;
			int c = disposeObjects.Count;
			for (int k = 0; k < c; k++) {
				Object o = disposeObjects[k];
				if (o != null) {
					Object.DestroyImmediate(o);
				}
			}
			disposeObjects.Clear();
		}

		public void MarkForDisposal(Object o) {
			if (o != null) {
				o.hideFlags = HideFlags.DontSave;
				disposeObjects.Add (o);
			}
		}


	}

}