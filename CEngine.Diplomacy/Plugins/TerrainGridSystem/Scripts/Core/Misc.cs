using UnityEngine;


namespace TGS
{

	public static class Misc
	{

		public static Vector4 Vector4zero = Vector4.zero;

		public static Vector3 Vector3zero = Vector3.zero;
		public static Vector3 Vector3one = Vector3.one;
		public static Vector3 Vector3up = Vector3.up;

		public static Vector2 Vector2left = Vector2.left;
		public static Vector2 Vector2right = Vector2.right;
		public static Vector2 Vector2one = Vector2.one;
		public static Vector2 Vector2zero = Vector2.zero;
		public static Vector2 Vector2down = Vector2.down;
		public static Vector2 Vector2half = Vector2.one * 0.5f;

		public static Color ColorNull = new Color (0, 0, 0, 0);

		public static int FastConvertToInt (string s)
		{
			int value = 0;
			int start, sign;
			if (s [0] == '-') {
				start = 1;
				sign = -1;
			} else {
				start = 0;
				sign = 1;
			}
			for (int i = start; i < s.Length; i++) {
				value = value * 10 + (s [i] - '0');
			}
			return value * sign;
		}
	}

}