using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {

	class Arc {
		public Point p;
		public Arc prev, next;
		public Event e;
		public Segment s0, s1;
		public VoronoiCell cell;

		public Arc (VoronoiCell cell, Point p) {
			this.p = p;
			this.cell = cell;
		}

		public Arc (VoronoiCell cell, Point p, Arc prev) : this (cell, p) {
			this.prev = prev;
		}

		public Arc (VoronoiCell cell, Point p, Arc prev, Arc next) : this (cell, p, prev) {
			this.next = next;
		}

	}

}
