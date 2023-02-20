using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TGS.Geom;

namespace TGS {

	public partial class TerrainGridSystem : MonoBehaviour {

		Dictionary<Segment, int> cachedSegments;
		List<Segment> indexedSegments;
		int totalSegCount;

		public void VoronoiDeserializeData (ref VoronoiCell[] cells) {
			indexedSegments = new List<Segment> (8000);
			using (MemoryStream stream = new MemoryStream (_voronoiSerializationData)) {
				BinaryReader br = new BinaryReader (stream, Encoding.UTF8);
				int cellsCount = br.ReadInt32 ();
				cells = new VoronoiCell[cellsCount];
				for (int k = 0; k < cellsCount; k++) {
					ReadCell (br, cells, k);
				}

			}
			indexedSegments = null;
		}

		void ReadCell (BinaryReader br, VoronoiCell[] cells, int k) {
			double x = br.ReadDouble ();
			double y = br.ReadDouble ();
			VoronoiCell cell = cells [k] = new VoronoiCell (new Point (x, y));
			int segCount = br.ReadInt16 ();
			cell.segments = new List<Segment> (segCount);
			for (int s = 0; s < segCount; s++) {
				int segIndex = br.ReadInt32 ();
				Segment seg;
				if (segIndex >= 0) {
					seg = indexedSegments[segIndex];
				} else {
					double startX = br.ReadDouble ();
					double startY = br.ReadDouble ();
					double endX = br.ReadDouble ();
					double endY = br.ReadDouble ();
					byte border = br.ReadByte ();
					byte deleted = br.ReadByte ();
					seg = new Segment (new Point(startX, startY), new Point(endX, endY), border == 1);
					seg.deleted = (deleted == 1);
				}
				cell.segments.Add (seg);
				indexedSegments.Add (seg);
			}
		}



		public void VoronoiSerializeData () {
			cachedSegments = new Dictionary<Segment, int> ();
			totalSegCount = 0;
			using (MemoryStream stream = new MemoryStream ()) {
				BinaryWriter bw = new BinaryWriter (stream, Encoding.UTF8);
				bw.Write (lastComputedVoronoiCells.Length);
				for (int k = 0; k < lastComputedVoronoiCells.Length; k++) {
					WriteCell (bw, lastComputedVoronoiCells [k]);
				}
				_voronoiSerializationData = stream.ToArray ();
			}
			cachedSegments = null;
		}

		void WriteCell (BinaryWriter bw, VoronoiCell cell) {
			bw.Write (cell.center.x);
			bw.Write (cell.center.y);
			int segCount = cell.segments.Count;
			bw.Write ((short)segCount);
			for (int k = 0; k < segCount; k++) {
				Segment seg = cell.segments [k];
				int segIndex;
				if (cachedSegments.TryGetValue (seg, out segIndex)) {
					bw.Write (segIndex);
				} else {
					cachedSegments [seg] = totalSegCount;
					bw.Write ((int)-1);
					bw.Write (seg.start.x);
					bw.Write (seg.start.y);
					bw.Write (seg.end.x);
					bw.Write (seg.end.y);
					if (seg.border) {
						bw.Write ((byte)1);
					} else {
						bw.Write ((byte)0);
					}
					if (seg.deleted) {
						bw.Write ((byte)1);
					} else {
						bw.Write ((byte)0);
					}
				}
				totalSegCount++;
			}
		}

	}

}