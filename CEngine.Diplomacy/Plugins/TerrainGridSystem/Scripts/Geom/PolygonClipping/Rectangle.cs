using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom {

	public class Rectangle {
		public double minX, minY, width, height;

		public Rectangle(double minX, double minY, double width, double height) {
			this.minX = minX;
			this.minY = minY;
			this.width = width;
			this.height = height;
		}

		public double right {
			get {
				return minX + width;
			}
		}

		public double top {
			get {
				return minY + height;
			}
		}

		public Rectangle Union(Rectangle o) {
			double minX = this.minX < o.minX + Point.PRECISION ? this.minX: o.minX;
			double maxX = this.right > o.right - Point.PRECISION ? this.right: o.right;
			double minY = this.minY < o.minY+ Point.PRECISION ? this.minY: o.minY;
			double maxY = this.top > o.top - Point.PRECISION ? this.top: o.top;
			return new Rectangle(minX, minY, maxX-minX, maxY-minY);
		}

		public bool Intersects(Rectangle o) {
			if (o.minX>right + Point.PRECISION) return false;
			if (o.right<minX - Point.PRECISION) return false;
			if (o.minY>top + Point.PRECISION) return false;
			if (o.top<minY - Point.PRECISION) return false;
			return true;
		}

		public override string ToString () {
			return string.Format ("minX:" + minX.ToString("F5") + " minY:" + minY.ToString("F5") + " width:" + width.ToString("F5") + " height:" + height.ToString ("F5"));
		} 
	}

}