using System;

namespace TGS.PathFinding {

	public class PathFindingPoint {
		private int _x;
		private int _y;

		public PathFindingPoint (int x, int y)
		{
			this._x = x;
			this._y = y;
		}

		public int x { get { return this._x; } set { this._x = value; } }

		public int y { get { return this._y; } set { this._y = value; } }

		// For debugging
		public override string ToString () {
			return string.Format ("{0}, {1}", this.x, this.y);
		}
	}
}
