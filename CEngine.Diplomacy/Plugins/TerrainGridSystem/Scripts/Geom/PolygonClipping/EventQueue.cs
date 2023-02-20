using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {
	class EventQueue {
		List<SweepEvent>elements;
		bool sorted;
		int pointer;
		
		public EventQueue () {
			elements = new List<SweepEvent> (1000); 
			sorted = false;
			pointer = -1;
		}
		
		public void Enqueue (SweepEvent obj) {
			if (!sorted) {
				elements.Add (obj);
				pointer = elements.Count-1;
				return;
			}
			// If already sorted use insertionSort on the inserted item.
//			int count = pointer = elements.Count;
			if (pointer < 0) {
				elements.Add (obj);
				pointer = 0;
				return;
			}
			elements.Add (null); // Expand the Vector by one.
			int i = pointer;
			pointer++;
			while (i >= 0 && CompareSweepEvent(obj, elements[i]) == -1) {
				elements [i + 1] = elements [i];
				i--;
			}
			elements [i + 1] = obj;
		}
		
		// The ordering is reversed because push and pop are faster.
		int CompareSweepEvent (SweepEvent e1, SweepEvent e2) {
			if (e1.Equals(e2)) return 0; 

			if (e1.p.x - e2.p.x > Point.PRECISION) // Different x coordinate
				return -1;
			
			if (e1.p.x - e2.p.x <  -Point.PRECISION) // Different x coordinate
				return 1;

			if ( e1.p.y - e2.p.y > Point.PRECISION) // Different points, but same x coordinate. The event with lower y coordinate is processed first
				return -1;

			if ( e1.p.y - e2.p.y < -Point.PRECISION ) // Different points, but same x coordinate. The event with lower y coordinate is processed first
				return 1;

			if (e1.isLeft != e2.isLeft) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first
				return (e1.isLeft) ? -1 : 1;

			// Same point, both events are left endpoints or both are right endpoints. The event associate to the bottom segment is processed first
			bool isAbove = e1.isAbove (e2.otherSE.p);
			return isAbove ? -1 : 1;
		}

		public SweepEvent Dequeue () {
			if (!sorted) {
				elements.Sort (CompareSweepEvent);
				sorted = true;
			}

//			SweepEvent e = elements [elements.Count - 1];
//			elements.RemoveAt (elements.Count - 1);
//			return e;

			return elements [pointer--];
		}
		
		public bool isEmpty { get {
//				return elements.Count == 0;
				return pointer<0;
			} }

	}

}