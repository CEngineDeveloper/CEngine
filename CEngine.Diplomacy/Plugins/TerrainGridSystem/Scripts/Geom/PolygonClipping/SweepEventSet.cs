using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace TGS.Geom {
	class SweepEventSet {
		public List<SweepEvent>eventSet;
		
		public SweepEventSet () {
			eventSet = new List<SweepEvent> (1000);
		}
		
		public void Remove (SweepEvent key) {
			int keyIndex = eventSet.IndexOf (key);
			if (keyIndex == -1)
				return;
			
			eventSet.RemoveAt (keyIndex);
		}
		
		public int Insert (SweepEvent item) {
			int count = eventSet.Count;
			if (count == 0) {
				eventSet.Add (item);
				return 0;
			}
			
			eventSet.Add (null); // Expand the Vector by one.
			
			int i = count - 1;
			while (i >= 0 && SegmentCompare(item, eventSet[i])) {
				eventSet [i + 1] = eventSet [i];
				i--;
			}
			eventSet [i + 1] = item;
			return i + 1;
		}
		
		double signedArea (Point p0, Point p1, Point p2) {
			return (p0.x - p2.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p0.y - p2.y);
		}

		private bool SegmentCompare (SweepEvent e1, SweepEvent e2) {

			if (e1.Equals(e2))
				return false;
			
			if (signedArea (e1.p, e1.otherSE.p, e2.p) != 0 || signedArea (e1.p, e1.otherSE.p, e2.otherSE.p) != 0) {
				// Segments are not collinear
				// If they share their left endpoint use the right endpoint to sort
				if (Point.EqualsBoth(e1.p, e2.p))
					return e1.isBelow (e2.otherSE.p);
				
				if (CompareSweepEvent (e1, e2))
					return e2.isAbove (e1.p);
				
				return e1.isBelow (e2.p);
			}
			
			if (Point.EqualsBoth(e1.p, e2.p)) // Segments colinear
				return false;
			
			return CompareSweepEvent (e1, e2);		
		}
		
		// Should only be called by segmentCompare
		private bool CompareSweepEvent (SweepEvent e1, SweepEvent e2) {
			if (e1.p.x > e2.p.x) // Different x coordinate
				return true;
			
			if (e2.p.x > e1.p.x) // Different x coordinate
				return false;
			
			if (e1.p != e2.p) // Different points, but same x coordinate. The event with lower y coordinate is processed first
				return e1.p.y > e2.p.y;
			
			if (e1.isLeft != e2.isLeft) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first
				return e1.isLeft;
			
			// Same point, both events are left endpoints or both are right endpoints. The event associate to the bottom segment is processed first
			return e1.isAbove (e2.otherSE.p);
		}
		
	}

}