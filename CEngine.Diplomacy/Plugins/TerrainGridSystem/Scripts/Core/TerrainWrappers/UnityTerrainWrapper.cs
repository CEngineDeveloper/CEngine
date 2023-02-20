using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;

namespace TGS {
	public class UnityTerrainWrapper : ITerrainWrapper {

		Terrain terrain;

		public bool supportsMultipleObjects {
			get { return false; }
		}

		public bool supportsCustomHeightmap {
			get { return false; }
		}

        public bool supportsPivot {
            get { return false; }
        }

        public GameObject gameObject {
			get { return terrain.gameObject; }
		}

		public Bounds bounds {
			get {
				Bounds bounds = terrain.terrainData.bounds;
				bounds.center += terrain.GetPosition ();
				return bounds;
			}
		}

        public void Dispose() {
        }

        public bool enabled {
			get { return terrain.drawHeightmap; }
			set { 
				terrain.drawHeightmap = value;
				terrain.drawTreesAndFoliage = value;
			}
		}

		public void Refresh () {
		}

		public void SetupTriggers (TerrainGridSystem tgs) {
#if !ENABLE_INPUT_SYSTEM
			TerrainTrigger trigger = terrain.gameObject.GetComponent<TerrainTrigger> ();
			if (trigger == null) {
				trigger = terrain.gameObject.AddComponent<TerrainTrigger> ();
			}
			trigger.Init<TerrainCollider> (tgs);
#endif
		}



		public UnityTerrainWrapper (Terrain terrain) {
			this.terrain = terrain;
		}

		public bool includesGameObject(GameObject go) {
			return terrain.gameObject == go;
        }

		public TerrainData terrainData {
			get {
				return terrain.terrainData;
			}
		}

		public int heightmapMaximumLOD {
			get { return terrain.heightmapMaximumLOD; }
			set {
				terrain.heightmapMaximumLOD = value;
			}
		}

		public int heightmapWidth {
			get { return terrain.terrainData.heightmapResolution; }
		}

		public int heightmapHeight {
			get { return terrain.terrainData.heightmapResolution; }
		}

		public float[,] GetHeights (int xBase, int yBase, int width, int height) {
			return terrain.terrainData.GetHeights (xBase, yBase, width, height);
		}

		public void SetHeights (int xBase, int yBase, float[,] heights) {
			terrain.terrainData.SetHeights (xBase, yBase, heights);
		}

		public T GetComponent<T> () {
			return gameObject.GetComponent<T> ();
		}


		public float SampleInterpolatedHeight (Vector3 worldPosition) {
			return terrain.SampleHeight (worldPosition);
		}

		public Transform transform {
			get {
				return terrain.transform;
			}
		}

		public Vector3 GetInterpolatedNormal (float x, float y) { 
			return terrain.terrainData.GetInterpolatedNormal (x, y);
		}

		public Vector3 size {
			get { if (terrain != null && terrain.terrainData != null) return terrain.terrainData.size; else return Misc.Vector3zero; }
		}

		public Vector3 localCenter {
			get {
				Vector3 size = terrain.terrainData.size;
				return new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);
			}
		}

		public bool Contains (GameObject gameObject) {
			return terrain.gameObject == gameObject;
		}

		public Vector3 GetLocalPoint (GameObject gameObject, Vector3 worldSpacePosition) {
			Vector3 localPoint = gameObject.transform.InverseTransformPoint (worldSpacePosition);
			localPoint.x = localPoint.x / terrain.terrainData.size.x - 0.5f;
			localPoint.y = localPoint.z / terrain.terrainData.size.z - 0.5f;
			return localPoint;
		}

	}
}

