using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS
{
	public interface ITerrainWrapper
	{

		GameObject gameObject { get; }
		bool includesGameObject(GameObject gameObject);
		T GetComponent<T> ();
		Transform transform { get; }

        void Dispose();
		bool supportsMultipleObjects { get; }
		bool supportsCustomHeightmap { get; }
        bool supportsPivot { get;  }
		bool enabled { get; set; }
		Bounds bounds { get; }
		TerrainData terrainData { get; }
		void Refresh();
		void SetupTriggers (TerrainGridSystem tgs);

		int heightmapMaximumLOD { get; set; }
		int heightmapWidth { get; }
		int heightmapHeight { get; }
		Vector3 localCenter { get; }
		Vector3 size { get; }
		float[,] GetHeights (int xBase, int yBase, int width, int height);
		void SetHeights (int xBase, int yBase, float[,] heights);
		float SampleInterpolatedHeight(Vector3 worldPosition);
		Vector3 GetInterpolatedNormal (float x, float y);
		bool Contains(GameObject gameObject);
		Vector3 GetLocalPoint(GameObject gameObject, Vector3 wsPosition);
	}
}

