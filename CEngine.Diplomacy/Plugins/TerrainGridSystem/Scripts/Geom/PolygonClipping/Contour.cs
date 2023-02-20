using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {
    public class Contour {
        public List<Point> points;
        public Rectangle bounds;

        public Contour() {
            points = new List<Point>(6);
        }

        public Contour(Point[] points) {
            this.points = new List<Point>(points);
        }

        public void Clear() {
            points.Clear();
            bounds = null;
        }

        public Contour Clone() {
            Contour u = new Contour();
            u.points = new List<Point>(this.points);
            u.bounds = this.bounds;
            return u;
        }


        public void Add(Point p) {
            points.Add(p);
            bounds = null;
        }


        public void SetPoints(List<Point> points) {
            this.points = points;
            bounds = null;
        }

        public void AddRange(List<Point> points) {
            this.points.AddRange(points);
            bounds = null;
        }

        public void GetVector2Points(Vector2 offset, Vector2 scale, ref List<Vector2> vector2points) {
            int pointCount = points.Count;
            if (vector2points == null) {
                vector2points = new List<Vector2>(pointCount);
            } else {
                vector2points.Clear();
            }
            Vector2 v;
            for (int k = 0; k < pointCount; k++) {
                Point point = points[k];
                FastMath.Round(ref point.x, ref point.y, 7);
                v.x = (float)point.x * scale.x + offset.x;
                v.y = (float)point.y * scale.y + offset.y;
                vector2points.Add(v);
            }
        }


        public Rectangle boundingBox {
            get {
                if (bounds != null)
                    return bounds;

                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                int pointCount = points.Count;
                for (int k = 0; k < pointCount; k++) {
                    Point p = points[k];
                    if (p.x > maxX)
                        maxX = p.x;
                    if (p.x < minX)
                        minX = p.x;
                    if (p.y > maxY)
                        maxY = p.y;
                    if (p.y < minY)
                        minY = p.y;
                }
                bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                return bounds;
            }
        }

        public Segment GetSegment(int index) {
            int lastPointIndex = points.Count - 1;
            if (index == lastPointIndex)
                return new Segment(points[lastPointIndex], points[0]);

            return new Segment(points[index], points[index + 1]);
        }

        /**
	 * Checks if a point is inside a contour using the point in polygon raycast method.
	 * This works for all polygons, whether they are clockwise or counter clockwise,
	 * convex or concave.
	 * @see 	http://en.wikipedia.org/wiki/Point_in_polygon#Ray_casting_algorithm
	 * @param	p
	 * @param	contour
	 * @return	True if p is inside the polygon defined by contour
	 */
        public bool ContainsPoint(Point p) {
            // Cast ray from p.x towards the right
            int intersections = 0;
            int pointCount = points.Count;
            int pointCountMinusOne = pointCount - 1;
            for (int i = 0; i < pointCount; i++) {
                Point curr = points[i];
                Point next = (i == pointCountMinusOne) ? points[0] : points[i + 1];

                if ((p.y >= next.y || p.y < curr.y) && (p.y >= curr.y || p.y < next.y)) { // was all equal (>= & <=)
                    continue;
                }

                // Edge is from curr to next.
                if (p.x < Math.Max(curr.x, next.x) && next.y != curr.y) {
                    // Find where the line intersects...
                    double xInt = (p.y - curr.y) * (next.x - curr.x) / (next.y - curr.y) + curr.x;
                    if (curr.x == next.x || p.x <= xInt)
                        intersections++;
                }
            }

            if (intersections % 2 == 0)
                return false;
            else
                return true;
        }

    }

}