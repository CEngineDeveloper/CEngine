using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {

	class SweepEvent {

		public Point p;
		public bool isLeft; 		// Is the point the left endpoint of the segment (p, other->p)?
		public PolygonType polygonType; 	// PolygonType to which this event belongs to: either PolygonClipper.SUBJECT, or PolygonClipper.CLIPPING
		public SweepEvent otherSE; 	// Event associated to the other endpoint of the segment
		public Segment _segment;

		/* Does the segment (p, other->p) represent an inside-outside transition
	 * in the polygon for a vertical ray from (p.x, -infinite) that crosses the segment? 
	 */
		public bool inOut;
		public EdgeType edgeType; 		// The EdgeType. @see EdgeType.as
		
		public bool inside; 		// Only used in "left" events. Is the segment (p, other->p) inside the other polygon?


		public SweepEvent(Point p, bool isLeft, PolygonType polygonType): this(p,isLeft,polygonType,null, EdgeType.NORMAL) {
		}

		public SweepEvent(Point p, bool isLeft, PolygonType polygonType, SweepEvent otherSweepEvent): this(p,isLeft,polygonType,otherSweepEvent, EdgeType.NORMAL) {
		}

		public SweepEvent(Point p, bool isLeft, PolygonType polygonType, SweepEvent otherSweepEvent, EdgeType edgeType) {
			this.p = p;
			this.isLeft = isLeft;
			this.polygonType = polygonType;
			this.otherSE = otherSweepEvent;
			this.edgeType = edgeType;
		}
		
		public Segment segment {
			get {
				if (_segment==null) {
					_segment = new Segment(p, otherSE.p);
				}
				return _segment;
			}
		}

		double signedArea(Point p0, Point p1, Point p2) {
			return (p0.x - p2.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p0.y - p2.y);
		}


		// Checks if this sweep event is below point p.
		public bool isBelow(Point x) {
			return (isLeft) ? (signedArea(p, otherSE.p, x) > 0) : (signedArea(otherSE.p, p, x) > 0);		
		}
		
		public bool isAbove(Point x) {
			return !isBelow(x);
		}

		public bool Equals(SweepEvent e2) {
			bool equal = isLeft == e2.isLeft && polygonType == e2.polygonType && inOut == e2.inOut && edgeType == e2.edgeType && inside == e2.inside && Point.EqualsBoth(p, e2.p);
			if (!equal) return false;

			return otherSE.isLeft == e2.otherSE.isLeft && otherSE.polygonType == e2.otherSE.polygonType &&
				otherSE.inOut == e2.otherSE.inOut && otherSE.edgeType == e2.otherSE.edgeType &&	otherSE.inside == e2.otherSE.inside && Point.EqualsBoth(otherSE.p, e2.otherSE.p);
		}
		
	}

}