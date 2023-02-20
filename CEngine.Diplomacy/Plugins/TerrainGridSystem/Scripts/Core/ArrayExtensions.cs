using UnityEngine;


namespace TGS {

	public static class ArrayExtensions {

		public static bool Contains<T> (this T[] array, T element) {
			for (int k = 0; k < array.Length; k++) {
				if (array [k].Equals(element)) {
					return true;
				}
			}
			return false;
		}

	}
}