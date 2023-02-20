using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {

	[Serializable]
	public class Segment : IDeserializationCallback  {
		public Point start, end;
		public bool border;
		public bool deleted;

		[NonSerialized]
		public bool done;

		[NonSerialized]
		public List<Segment> subdivisions;

		[NonSerialized]
		public int territoryIndex = -1;

		[NonSerialized]
		public int disputingTerritory1Index = -1;

		[NonSerialized]
		public int disputingTerritory2Index = -1;

		[NonSerialized]
		public int cellIndex = -1;

		public Vector3 startToVector3 {
			get {
				return new Vector3 ((float)start.x, (float)start.y, 0);
			}
		}

		public Vector3 endToVector3 {
			get {
				return new Vector3 ((float)end.x, (float)end.y, 0);
			}
		}

		void IDeserializationCallback.OnDeserialization(object tgs) {
			// deserializer calls this and ensures default values are correct because these fields are not serialized
			disputingTerritory1Index = disputingTerritory2Index = territoryIndex = cellIndex = -1;
		}


		public Segment (Point start, Point end) : this (start, end, false) {
		}

		public Segment (Point start, Point end, bool border) {
			this.start = start;
			this.end = end;
			this.border = border;
			done = true;
		}

		public void Init(Point start, Point end) {
			this.start = start;
			this.end = end;
			done = true;
		}


		public Segment (Point p) {
			start = p;
		}

		public void Finish (Point p) {
			if (done)
				return;
			end = p;
			done = true;
		}

		public double sqrMagnitude {
			get {
				double dx = end.x - start.x;
				double dy = end.y - start.y;
				return dx * dx + dy * dy;
			}
		}

		public double magnitude {
			get {
				double dx = end.x - start.x;
				double dy = end.y - start.y;
				return Math.Sqrt (dx * dx + dy * dy);
			}
		}


		public List<Segment> Subdivide (int divisions, double waveAmount) {
			if (subdivisions != null)
				return subdivisions;
			
			// Divide and add random displacement
			if (subdivisions != null) {
				subdivisions.Clear ();
			} else {
				subdivisions = new List<Segment> (divisions);
			}
			Point normal = Point.zero;
			double l = 0;
			if (!border && waveAmount > 0 && divisions > 1) {
				// safety check - length must be > 0.01f;
				l = waveAmount * Math.Sqrt (sqrMagnitude);
				normal = new Point (-(end.y - start.y), end.x - start.x);
				normal = normal.normalized * l;
				if (UnityEngine.Random.value > 0.5f)
					normal *= -1;
			}
			Point d0 = start;
			for (int d = 1; d < divisions; d++) {
				Point d1 = Point.Lerp (start, end, (double)d / divisions);
				if (!border && waveAmount > 0) {
					double s = 1 - Math.Abs (d - (double)divisions / 2) / ((double)divisions / 2);
					d1 += normal * Math.Sin (d * Math.PI / divisions) * s;
				}
				subdivisions.Add (new Segment (d0, d1, border));
				d0 = d1;
			}
			subdivisions.Add (new Segment (d0, end, border));
			return subdivisions;
		}


		public override string ToString () {
			return string.Format ("start:" + start.ToString () + ", end:" + end.ToString ());
		}

		public void CropBottom () {
			start.CropBottom ();
			end.CropBottom ();

			if (Point.EqualsBoth (start, end))
				deleted = true;
		}

		public void CropRight () {
			start.CropRight ();
			end.CropRight ();

			if (Point.EqualsBoth (start, end))
				deleted = true;
		}

	}

}
	