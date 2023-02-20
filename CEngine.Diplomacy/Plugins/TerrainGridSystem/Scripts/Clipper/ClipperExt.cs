using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGS.ClipperLib {
	public partial class Clipper {

		const float MULTIPLIER = 5000000;

		Region subject;
        List<Region> subjects;
		List<List<IntPoint>> solution;

		public override void Clear () {
			subject = null;
			base.Clear ();
		}

		public void AddPaths (List<Region> regions, PolyType polyType) {
			int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				AddPath (regions [k], polyType);
			}
		}

		public void AddPath (Region region, PolyType polyType) {
			if (region == null || region.points == null)
				return;
			int count = region.points.Count;
			List<IntPoint> points = new List<IntPoint> (count);
			for (int k = 0; k < count; k++) {
				double ix = region.points [k].x * MULTIPLIER;
				double iy = region.points [k].y * MULTIPLIER;
				IntPoint p = new IntPoint (ix, iy);
				points.Add (p);
			}
			AddPath (points, polyType, true);

			if (polyType == PolyType.ptSubject) {
				subject = region;
                if (subjects == null) {
                    subjects = new List<Region>();
                }
                subjects.Add(region);
			}
		}

		public void AddPath (Vector2[] points, PolyType polyType) {
			if (points == null)
				return;
			if (polyType == PolyType.ptSubject) {
				Debug.LogError ("Subject polytype needs a Region object.");
				return;
			}
			int count = points.Length;
			List<IntPoint> newPoints = new List<IntPoint> (count);
			for (int k = 0; k < count; k++) {
				double ix = points [k].x * MULTIPLIER;
				double iy = points [k].y * MULTIPLIER;
				IntPoint p = new IntPoint (ix, iy);
				newPoints.Add (p);
			}
			AddPath (newPoints, polyType, true);
		}

		public void Execute (ClipType clipType, Territory entity) {
			if (solution == null) {
				solution = new List<List<IntPoint>> ();
			}
			Execute (clipType, solution);
			int contourCount = solution.Count;
            // Remove subject from entity
            if (subjects != null) {
                for (int k = 0; k < subjects.Count; k++) {
                    Region sub = subjects[k];
                    if (entity.regions.Contains(sub)) {
                        entity.regions.Remove(sub);
                    }
                }
            }
			// Add resulting regions
			for (int c = 0; c < contourCount; c++) {
                // In the case of difference operations, the resulting polytongs could be artifacts if the frontiers do not match perfectly. In that case, we ignore small isolated triangles.
                if (clipType == ClipType.ctUnion || solution[c].Count >= 5) {
					Region region = new Region(entity, false);
					region.SetPoints(BuildPointList(solution[c]));
                    entity.regions.Add(region);
                }
			}
		}

		Vector2[] BuildPointArray (List<IntPoint> points) {
			int count = points.Count;
			Vector2[] newPoints = new Vector2[count];
			for (int k = 0; k < count; k++) {
				newPoints [k].x = (float)points [k].X / MULTIPLIER;
				newPoints [k].y = (float)points [k].Y / MULTIPLIER;
			}
			return newPoints;
		}

		List<Vector2> BuildPointList(List<IntPoint> points) {
			int count = points.Count;
			List<Vector2> newPoints = new List<Vector2>(count);
			for (int k = 0; k < count; k++) {
				Vector2 point = new Vector2((float)points[k].X / MULTIPLIER, (float)points[k].Y / MULTIPLIER);
				newPoints.Add(point);
			}
			return newPoints;
		}

		public void Execute (ClipType clipType, Region output) {
			if (solution == null) {
				solution = new List<List<IntPoint>> ();
			}
			Execute (clipType, solution);
			int contourCount = solution.Count;
			if (contourCount == 0) {
				output.Clear ();
			} else {
				// Use the largest contour
				int best = 0;
				int pointCount = solution [0].Count;
				for (int k = 1; k < contourCount; k++) {
					int candidatePointCount = solution [k].Count;
					if (candidatePointCount > pointCount) {
						pointCount = candidatePointCount;
						best = k;
					}
				}
				output.SetPoints(BuildPointList(solution[best]));
			}
		}

		public void Execute (ClipType clipType) {
			if (subject == null) {
				Debug.LogError ("Clipper.Execute called without defined subject");
				return;
			}
			Execute (clipType, subject);
		}
	

	}

}