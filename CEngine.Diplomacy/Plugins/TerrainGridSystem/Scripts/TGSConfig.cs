using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS {

	[ExecuteInEditMode]
	public class TGSConfig : MonoBehaviour {

		[Tooltip ("User-defined name for this configuration")]
		[TextArea]
		public string title = "Optionally name this configuration editing this text.";

		[Tooltip ("Enter a comma separated list of territory indices to use from this configuration. Leave this field in black to restore all territories.")]
		public string filterTerritories;

		public TGSConfigEntry[] cellSettings;

		[HideInInspector]
		public string config;

		[HideInInspector]
		public Texture2D[] textures;

		void Awake() {
			// Migrate old format
			if (string.IsNullOrEmpty (config))
				return;
			string[] cellsInfo = config.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			char[] separators = new char[] { ',' };
			cellSettings = new TGSConfigEntry[cellsInfo.Length];
			for (int k = 0; k < cellsInfo.Length; k++) {
				TGSConfigEntry entry = new TGSConfigEntry ();
				string[] cellInfo = cellsInfo [k].Split (separators, StringSplitOptions.RemoveEmptyEntries);
				int length = cellInfo.Length;
				if (length > 1) {
					int territoryIndex = Misc.FastConvertToInt (cellInfo [1]);
					entry.territoryIndex = territoryIndex;
				}
				if (length > 0) {
					if (cellInfo [0].Length > 0) {
						entry.visible = cellInfo [0] [0] != '0';
					}
				}
				Color color = new Color (0, 0, 0, 0);
				if (length > 5) {
					Single.TryParse (cellInfo [2], out color.a);
					if (color.a > 0) {
						Single.TryParse (cellInfo [3], out color.r);
						Single.TryParse (cellInfo [4], out color.g);
						Single.TryParse (cellInfo [5], out color.b);
					}
				} 
				int textureIndex = -1;
				if (length > 6) {
					textureIndex = Misc.FastConvertToInt (cellInfo [6]);
				}
				if (color.a > 0 || textureIndex >= 1) {
					entry.color = color;
					entry.textureIndex = textureIndex;
				}
				if (length > 7) {
					entry.tag = Misc.FastConvertToInt (cellInfo [7]);
				}
                cellSettings[k] = entry;
			}
			config = null;
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("TGS config migrated. You can save the scene now.");
			#endif
		}

		void OnEnable () {
			if (!Application.isPlaying)
				LoadConfiguration ();
		}

		void Start () {
            if (Application.isPlaying) {
                LoadConfiguration();
            }
		}


		public void Clear () {
			TerrainGridSystem tgs = GetTGS ();
			if (tgs != null)
				tgs.ClearAll ();
		}

		/// <summary>
		/// Stores current grid configuration in this component
		/// </summary>
		public void SaveConfiguration (TerrainGridSystem tgs) {
			if (tgs == null)
				return;
			if (tgs.textures != null) {
				textures = new Texture2D[tgs.textures.Length];
				for (int k = 0; k < tgs.textures.Length; k++) {
					textures [k] = tgs.textures [k];
				}
			}
			cellSettings = tgs.CellGetSettings ();
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
			#endif
		}

		/// <summary>
		/// Call this method to force a configuration load.
		/// </summary>
		public void LoadConfiguration () {
			if (cellSettings == null)
				return;
			
			TerrainGridSystem tgs = GetTGS ();
			if (tgs == null) {
				return;
			}
			if (textures != null) {
				tgs.textures = textures;
			}
			int[] territories = null;
			if (!string.IsNullOrEmpty (filterTerritories)) {
				string[] ss = filterTerritories.Split (new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
				List<int> tt = new List<int> ();
				for (int k = 0; k < ss.Length; k++) {
					int v = 0;
					if (int.TryParse (ss [k], out v)) {
						tt.Add (v);
					}
				}
				territories = tt.ToArray ();
			}
			tgs.CellSetSettings (cellSettings, territories);
		}

		TerrainGridSystem GetTGS () {
			TerrainGridSystem tgs = GetComponent<TerrainGridSystem> ();
			if (tgs == null) {
				tgs = TerrainGridSystem.instance;
			}
			if (tgs == null) {
				Debug.Log ("Terrain Grid System not found!");
				return null;
			}
			return tgs;
		}

	}

}