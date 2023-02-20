using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {

	[Serializable]
	public class VoronoiCell {
		public List <Segment> segments;
		public Point center;
		// for cropping
		[NonSerialized]
		public List<Point> top, left, bottom, right;

		public VoronoiCell (Point center) {
			segments = new List<Segment> (16);
			this.center = center;
			left = new List<Point> ();
			top = new List<Point> ();
			bottom = new List<Point> ();
			right = new List<Point> ();
		}

		public Polygon GetPolygon (int edgeSubdivisions, float curvature) {
			Connector connector = new Connector ();
			int segmentsCount = segments.Count;
			for (int k = 0; k < segmentsCount; k++) {
				Segment s = segments [k];
				if (!s.deleted) {
					if (edgeSubdivisions > 1) {
						connector.AddRange (s.Subdivide (edgeSubdivisions, curvature));
					} else {
						connector.Add (s);
					}
				}
			}
			return connector.ToPolygonFromLargestLineStrip ();
		}

	
		public Point centroid {
			get {
				int count = 0;
				int segmentCount = segments.Count;
				double x = 0, y = 0;
				for (int k = 0; k < segmentCount; k++) {
					Segment s = segments [k];
					if (!s.deleted) {
						x += s.start.x;
						y += s.start.y;
						x += s.end.x;
						y += s.end.y;
						count += 2;
					}
				}
				if (count == 0)
					count = 1;
				Point point = new Point (x / count, y / count);
				return point;
			}
		}

	}

}
