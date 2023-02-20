using UnityEngine;
using System;
using System.Collections.Generic;

namespace TGS.Geom {

	[Serializable]
	public struct Point: IEquatable<Point>, IEqualityComparer<Point> {

		public const double PRECISION = 1e-8;
		public static readonly Point zero = new Point (0, 0);

		public double x;
		public double y;


		public Point (double x, double y) {
			this.x = x;
			this.y = y;
		}

		public Vector3 vector3 {
			get {
				return new Vector3((float)x, (float)y);
			}
		}


		public double magnitude {
			get {
				return Math.Sqrt (x * x + y * y);
			}
		}

		public static double Distance (Point p1, Point p2) {
			return Math.Sqrt ((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
		}

		public override string ToString () {
			return "x:" + x.ToString ("F5") + " y:" + y.ToString ("F5");
		}


		public bool Equals (Point p0, Point p1) {
			return  (p0.x - p1.x) < PRECISION && (p0.x - p1.x) > -PRECISION &&
			(p0.y - p1.y) < PRECISION && (p0.y - p1.y) > -PRECISION;
		}

		public static bool EqualsBoth (Point p0, Point p1) {
			return  (p0.x - p1.x) < PRECISION && (p0.x - p1.x) > -PRECISION &&
			(p0.y - p1.y) < PRECISION && (p0.y - p1.y) > -PRECISION;
		}

		public bool Equals(Point o) {
			return o.x == x && o.y == y;
		}

		public override bool Equals (object o) {
			if (o is Point) {
				return ((Point)o).x == x && ((Point)o).y == y;
			}
			return false;
		}

		public override int GetHashCode () {
			return (int)(10e7 * x + 10e9 * y);
		}

		public int GetHashCode (Point p) {
			//return (int)(10e7 * Math.Round (p._x, 7) + 10e9 * Math.Round (p._y, 7));
			return (int)(10e7 * x + 10e9 * y);
		}

		public static bool operator == (Point p1, Point p2) {
			return p1.x == p2.x && p1.y == p2.y;
		}

		public static bool operator != (Point p1, Point p2) {
			return p1.x != p2.x || p1.y != p2.y;
		}

		public static Point operator - (Point p1, Point p2) {
			return new Point (p1.x - p2.x, p1.y - p2.y);
		}

		public static Point operator - (Point p) {
			return new Point (-p.x, -p.y);
		}

		public static Point operator + (Point p1, Point p2) {
			return new Point (p1.x + p2.x, p1.y + p2.y);
		}

		public static Point operator * (Point p, double scalar) {
			return new Point (p.x * scalar, p.y * scalar);
		}

		public static Point operator / (Point p, double scalar) {
			return new Point (p.x / scalar, p.y / scalar);
		}

		public double sqrMagnitude {
			get {
				return x * x + y * y;
			}
		}

		public static Point operator - (Vector2 p1, Point p2) {
			return new Point (p1.x - p2.x, p1.y - p2.y);
		}


		public static Point Lerp (Point start, Point end, double t) {
			return start + (end - start) * t;

		}

		public void Normalize () {
			double d = Math.Sqrt (x * x + y * y);
			x /= d;
			y /= d;
		}

		public Point normalized {
			get {
				double d = Math.Sqrt (x * x + y * y);
				return new Point (x / d, y / d);
			}
		}

		public Point Offset (double deltax, double deltay) {
			return new Point (x + deltax, y + deltay);
		}

		public void CropBottom () {
			if (y < -0.5)
				y = -0.5;
		}

		public void CropRight () {
			if (x > 0.5)
				x = 0.5;
		}


    }

}
