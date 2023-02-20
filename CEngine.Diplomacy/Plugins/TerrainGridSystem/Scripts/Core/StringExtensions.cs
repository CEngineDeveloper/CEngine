using UnityEngine;


namespace TGS {

	public static class StringExtensions {

		public static string Truncate(this string s, int length) {
			if (string.IsNullOrEmpty(s)) return "";
			int len = s.Length < length ? s.Length : length;
			return s.Substring (0, len);
		}

	}
}