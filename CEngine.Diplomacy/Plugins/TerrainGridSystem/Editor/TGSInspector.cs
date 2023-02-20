using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using TGS;

namespace TGS_Editor {
    [CustomEditor(typeof(TerrainGridSystem))]
    public class TGSInspector : Editor {

        TerrainGridSystem tgs;
        Texture2D _headerTexture;
        string[] selectionModeOptions, topologyOptions, overlayModeOptions;
        int[] topologyOptionsValues;
        GUIStyle infoLabelStyle;
        int cellHighlightedIndex = -1, cellTerritoryIndex, cellTextureIndex;
        List<int> cellSelectedIndices;

        Color colorSelection, cellColor;
        int textureMode, cellTag;
        float cellCrossCost;
        static GUIStyle toggleButtonStyleNormal;
        static GUIStyle toggleButtonStyleToggled;
        static GUIStyle blackBack;

        SerializedProperty isDirty;
        StringBuilder sb;
        Vector2 cellSize;

        void OnEnable() {

            _headerTexture = Resources.Load<Texture2D>("EditorHeader");
            blackBack = new GUIStyle();
            blackBack.normal.background = MakeTex(4, 4, Color.black);

            selectionModeOptions = new string[] {
                "None",
                "Territories",
                "Cells"
            };
            overlayModeOptions = new string[] { "Overlay", "Ground" };
            topologyOptions = new string[] { "Irregular", "Box", "Hexagonal" };
            topologyOptionsValues = new int[] {
                (int)GridTopology.Irregular,
                (int)GridTopology.Box,
                (int)GridTopology.Hexagonal
            };

            tgs = (TerrainGridSystem)target;
            if (tgs.cells == null || tgs.textures == null) {
                tgs.Init();
            }
            sb = new StringBuilder();
            cellSelectedIndices = new List<int>();
            colorSelection = new Color(1, 1, 0.5f, 0.85f);
            cellColor = Color.white;
            isDirty = serializedObject.FindProperty("_isDirty");
            cellSize = tgs.cellSize;
            HideEditorMesh();
        }

        public override void OnInspectorGUI() {

            float labelWidth = EditorGUIUtility.labelWidth;

            GUILayout.BeginHorizontal(blackBack);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(_headerTexture, GUILayout.ExpandWidth(true), GUILayout.Height(32));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            DrawTitleLabel("Grid Configuration");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Help")) {
                EditorUtility.DisplayDialog("Terrain Grid System", "TGS is an advanced grid generator for Unity terrain. It can also work as a standalone 2D grid.\n\nFor a complete description of the options, please refer to the documentation guide (PDF) included in the asset.\nWe also invite you to visit and sign up on our support forum on kronnect.com where you can post your questions/requests.\n\nThanks for purchasing! Please rate Terrain Grid System on the Asset Store! Thanks.", "Close");
            }
            if (GUILayout.Button("Redraw")) {
                tgs.Redraw(false);
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("Clear")) {
                if (EditorUtility.DisplayDialog("Clear All", "Remove any color/texture from cells and territories?", "Ok", "Cancel")) {
                    tgs.ClearAll();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            tgs.terrainObject = (GameObject)EditorGUILayout.ObjectField("Terrain", tgs.terrainObject, typeof(GameObject), true);
            if (tgs.terrainObject != null) {
                EditorGUI.indentLevel++;
                tgs.multiTerrain = EditorGUILayout.Toggle(new GUIContent("Multi Terrain", "Spans the grid across multiple terrain tiles. To use this feature, add Terrain Grid System to any terrain tile and toggle this checkbox. Terrain Grid System will automatically locate all neighbours of terrain and adjust the grid size to include them."), tgs.multiTerrain);
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck()) {
                GUIUtility.ExitGUI();
                return;
            }

            if (tgs.terrain != null) {
                if (tgs.terrain.supportsMultipleObjects) {
                    EditorGUI.indentLevel++;
                    tgs.terrainObjectsPrefix = EditorGUILayout.TextField(new GUIContent("Objects Name Prefix", "Use terrain gameobjects which has this prefix in their names."), tgs.terrainObjectsPrefix);
                    tgs.terrainObjectsLayerMask = LayerMaskField(new GUIContent("Objects Layer Mask", "Use terrain gameobjects which match this layer mask."), (int)tgs.terrainObjectsLayerMask);
                    tgs.terrainObjectsSearchGlobal = EditorGUILayout.Toggle(new GUIContent("Search Global", "Include potential objects in the entire scene."), tgs.terrainObjectsSearchGlobal);
                    EditorGUI.indentLevel--;
                }
                if (tgs.terrain.supportsCustomHeightmap) {
                    tgs.heightmapWidth = EditorGUILayout.IntField(new GUIContent("Heightmap Width"), tgs.heightmapWidth);
                    tgs.heightmapHeight = EditorGUILayout.IntField(new GUIContent("Heightmap Height"), tgs.heightmapHeight);
                }
            }

            tgs.gridTopology = (GridTopology)EditorGUILayout.IntPopup("Topology", (int)tgs.gridTopology, topologyOptions, topologyOptionsValues);
            bool bakedVoronoi = false;
            if (tgs.gridTopology == GridTopology.Irregular) {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !tgs.hasBakedVoronoi && !Application.isPlaying;
                if (GUILayout.Button("Bake Voronoi")) {
                    tgs.VoronoiSerializeData();
                }
                bakedVoronoi = tgs.hasBakedVoronoi;
                GUI.enabled = bakedVoronoi && !Application.isPlaying;
                if (GUILayout.Button("Clear Baked Data")) {
                    tgs.voronoiSerializationData = null;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            if (bakedVoronoi) {
                EditorGUILayout.LabelField("Cells (aprox.)", tgs.numCells.ToString());
            } else {
                if (tgs.gridTopology == GridTopology.Irregular) {
                    tgs.numCells = EditorGUILayout.IntField("Cells (aprox.)", tgs.numCells);
                } else {
                    tgs.columnCount = EditorGUILayout.IntField("Columns", tgs.columnCount);
                    tgs.rowCount = EditorGUILayout.IntField("Rows", tgs.rowCount);
                }
            }
            tgs.numTerritories = EditorGUILayout.IntSlider("Territories", tgs.numTerritories, 0, Mathf.Min(tgs.numCells, TerrainGridSystem.MAX_TERRITORIES));
            if (!bakedVoronoi) {
                if (tgs.numTerritories > 0) {
                    tgs.territoriesTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("   Territories Texture", "Quickly create territories assigning a color texture in which each territory corresponds to a color."), tgs.territoriesTexture, typeof(Texture2D), true);
                    if (tgs.territoriesTexture != null) {
                        CheckTextureImportSettings(tgs.territoriesTexture);
                        tgs.territoriesTextureNeutralColor = EditorGUILayout.ColorField(new GUIContent("   Neutral Color", "Color to be ignored."), tgs.territoriesTextureNeutralColor, true, true, true);
                        EditorGUILayout.BeginHorizontal();
                        tgs.territoriesHideNeutralCells = EditorGUILayout.Toggle(new GUIContent("   Hide Neutral Cells", "Cells belonging to neutral territories will be invisible."), tgs.territoriesHideNeutralCells);
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Generate Territories", GUILayout.Width(140))) {
                            tgs.CreateTerritories(tgs.territoriesTexture, tgs.territoriesTextureNeutralColor, tgs.territoriesHideNeutralCells);
                        }
                        EditorGUILayout.EndHorizontal();
                    } else {
                        if (tgs.gridTopology != GridTopology.Irregular) {
                            tgs.territoriesAsymmetry = EditorGUILayout.Slider(new GUIContent("   Asymmetry", "Determines the distribution of cells among territories. Increase this value to produce territories with different sizes."), tgs.territoriesAsymmetry, 0, 1);
                            tgs.territoriesOrganic = EditorGUILayout.Slider(new GUIContent("   Organic", "Produces wacky borders."), tgs.territoriesOrganic, 0, 1);
                            tgs.territoriesMaxIterations = EditorGUILayout.IntField(new GUIContent("   Max Iterations", "Maximum number of iterations used in the process that assigns cells to territories. 0 = no limit."), tgs.territoriesMaxIterations);
                        }
                    }
                }
                if (tgs.gridTopology == GridTopology.Hexagonal) {
                    tgs.regularHexagons = EditorGUILayout.Toggle("Regular Hexes", tgs.regularHexagons);
                    if (tgs.regularHexagons) {
                        tgs.regularHexagonsWidth = EditorGUILayout.FloatField("   Hex Width", tgs.regularHexagonsWidth);
                    }
                    tgs.pointyTopHexagons = EditorGUILayout.Toggle("Pointy Top", tgs.pointyTopHexagons);
                    tgs.evenLayout = EditorGUILayout.Toggle("Even Layout", tgs.evenLayout);
                }
            }

            if (tgs.gridTopology == GridTopology.Irregular) {
                if (tgs.numCells > 10000) {
                    EditorGUILayout.HelpBox("Total cell count exceeds recommended maximum of 10.000!", MessageType.Warning);
                }
            } else if (tgs.rowCount > TerrainGridSystem.MAX_ROWS_OR_COLUMNS || tgs.columnCount > TerrainGridSystem.MAX_ROWS_OR_COLUMNS) {
                EditorGUILayout.HelpBox("Total row or column count exceeds recommended maximum of " + TerrainGridSystem.MAX_ROWS_OR_COLUMNS + "!", MessageType.Warning);
            }

            if (!bakedVoronoi) {
                if (tgs.numCells > TerrainGridSystem.MAX_CELLS_FOR_CURVATURE) {
                    EditorGUILayout.LabelField("Curvature", "Not available with >" + TerrainGridSystem.MAX_CELLS_FOR_CURVATURE + " cells");
                } else {
                    tgs.gridCurvature = EditorGUILayout.Slider("Curvature", tgs.gridCurvature, 0, 0.1f);
                }
                if (tgs.gridTopology != GridTopology.Irregular) {
                    EditorGUILayout.LabelField("Relaxation", "Only available with irregular topology");
                } else if (tgs.numCells > TerrainGridSystem.MAX_CELLS_FOR_RELAXATION) {
                    EditorGUILayout.LabelField("Relaxation", "Not available with >" + TerrainGridSystem.MAX_CELLS_FOR_RELAXATION + " cells");
                } else {
                    tgs.gridRelaxation = EditorGUILayout.IntSlider("Relaxation", tgs.gridRelaxation, 1, 32);
                }
                if (tgs.territoriesTexture != null) {
                    GUI.enabled = false;
                    EditorGUILayout.HelpBox("Territories driven by territory texture. Random seed not used.", MessageType.Info);
                }
                tgs.seed = EditorGUILayout.IntSlider("Seed", tgs.seed, 1, 10000);
                GUI.enabled = true;
            }

            if (tgs.terrain != null) {
                tgs.gridRoughness = EditorGUILayout.Slider("Roughness", tgs.gridRoughness, 0f, 0.2f);
                tgs.cellsFlat = EditorGUILayout.Toggle(new GUIContent("Flat Cells", "If enabled, cells will be rendered as horizontal/flat surfaces ignoring the terrain or mesh slope."), tgs.cellsFlat);
                if (tgs.cellsFlat) {
                    tgs.gridFlatMask = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("   Mask", "Alpha channel is used to determine if a cell is rendered flat (0 = cell adapts to terrain as usual)"), tgs.gridFlatMask, typeof(Texture2D), true);
                    if (CheckTextureImportSettings(tgs.gridFlatMask)) {
                        tgs.ReloadFlatMask();
                    }
                } else {
                    tgs.cellsMaxSlope = EditorGUILayout.Slider("Max Slope", tgs.cellsMaxSlope, 0, 1f);
                    EditorGUILayout.BeginHorizontal();
                    tgs.cellsMaxHeightDifference = EditorGUILayout.FloatField("Max Height Difference", tgs.cellsMaxHeightDifference);
                    DrawInfoLabel("(0 = not used)");
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                tgs.cellsMinimumAltitude = EditorGUILayout.FloatField("Minimum Altitude", tgs.cellsMinimumAltitude);
                DrawInfoLabel("(0 = not used)");
                EditorGUILayout.EndHorizontal();
                if (tgs.cellsMinimumAltitude != 0) {
                    tgs.cellsMinimumAltitudeClampVertices = EditorGUILayout.Toggle(new GUIContent("   Clamp Vertices", "Clamp vertices altitude to the minimum altitude."), tgs.cellsMinimumAltitudeClampVertices);
                }

                EditorGUILayout.BeginHorizontal();
                tgs.cellsMaximumAltitude = EditorGUILayout.FloatField("Maximum Altitude", tgs.cellsMaximumAltitude);
                DrawInfoLabel("(0 = not used)");
                EditorGUILayout.EndHorizontal();
                if (tgs.cellsMaximumAltitude != 0) {
                    tgs.cellsMaximumAltitudeClampVertices = EditorGUILayout.Toggle(new GUIContent("   Clamp Vertices", "Clamp vertices altitude to the maximum altitude."), tgs.cellsMaximumAltitudeClampVertices);
                }
            }

            tgs.gridMask = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Mask", "Alpha channel is used to determine cell visibility (0 = cell is not visible)"), tgs.gridMask, typeof(Texture2D), true);
            if (CheckTextureImportSettings(tgs.gridMask)) {
                tgs.ReloadGridMask();
            }
            if (tgs.gridMask != null) {
                tgs.gridMaskUseScale = EditorGUILayout.Toggle(new GUIContent("   Use Scale", "Respects offset and scale parameters when applying mask."), tgs.gridMaskUseScale);
                tgs.gridMaskInsideCount = EditorGUILayout.IntField(new GUIContent("   Inside Count", "Minimum number of vertices that must be inside the grid mask to consider the entire cell is inside the mask."), tgs.gridMaskInsideCount);
            }

            tgs.useGeometryShaders = EditorGUILayout.Toggle(new GUIContent("Use Geometry Shaders", "Use geometry shaders if platform supports them."), tgs.useGeometryShaders);
            tgs.transparentBackground = EditorGUILayout.Toggle("Transparent Background", tgs.transparentBackground);
            tgs.useStencilBuffer = EditorGUILayout.Toggle(new GUIContent("Use Stencil Buffer", "When enabled, stencil buffer will be used to avoid overdraw and ensure correct rendering order of grid features. You can disable this option if it creates conflicts with other stencil-based renderers."), tgs.useStencilBuffer);

            GUI.enabled = tgs.transparentBackground;
            tgs.sortingOrder = EditorGUILayout.IntField("Transparent Sorting Order", tgs.sortingOrder);
            GUI.enabled = true;

            tgs.canvasTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Canvas Texture", "Optional texture for background that's revealed when using the texture methods of cells or territories."), tgs.canvasTexture, typeof(Texture2D), true);

            int cellsCreated = tgs.cells == null ? 0 : tgs.cells.Count;
            int territoriesCreated = tgs.territories.Count;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawInfoLabel("Cells Created: " + cellsCreated + " / Territories Created: " + territoriesCreated + " / Vertex Count: " + tgs.lastVertexCount);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            DrawTitleLabel("Grid Positioning");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hide Objects", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (tgs.terrain != null && GUILayout.Button("Toggle Terrain")) {
                tgs.terrain.enabled = !tgs.terrain.enabled;
                tgs.transparentBackground = !tgs.terrain.enabled;
                if (tgs.transparentBackground && tgs.gridSurfaceDepthOffsetTerritory < 20) {
                    tgs.gridSurfaceDepthOffsetTerritory = 20;
                } else if (!tgs.transparentBackground && tgs.gridSurfaceDepthOffsetTerritory > 0) {
                    tgs.gridSurfaceDepthOffsetTerritory = -1;
                }
            }
            if (GUILayout.Button("Toggle Grid")) {
                tgs.gameObject.SetActive(!tgs.gameObject.activeSelf);
            }
            EditorGUILayout.EndHorizontal();

            if (tgs.terrain != null && tgs.terrain.supportsPivot) {
                tgs.terrainMeshPivot = EditorGUILayout.Vector2Field(new GUIContent("Mesh Pivot", "Specify a center correction if mesh center is not at 0,0,0"), tgs.terrainMeshPivot);
            }

            EditorGUILayout.BeginHorizontal();
            tgs.gridCenter = EditorGUILayout.Vector2Field(new GUIContent("Center", "The position of the grid center."), tgs.gridCenter);
            if (GUILayout.Button(new GUIContent("X", "Reset value"), GUILayout.Width(20))) {
                tgs.gridCenter = Vector2.zero;
            }
            EditorGUILayout.EndHorizontal();

            if (tgs.gridTopology == GridTopology.Hexagonal && tgs.regularHexagons) {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            tgs.gridScale = EditorGUILayout.Vector2Field("Scale", tgs.gridScale);
            if (GUILayout.Button(new GUIContent("X", "Reset value"), GUILayout.Width(20))) {
                tgs.gridScale = Vector2.one;
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            if (tgs.gridTopology == GridTopology.Hexagonal && tgs.regularHexagons) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Scale is driven by regular hexagons option.", MessageType.Info);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            } else if (tgs.gridTopology != GridTopology.Irregular) {
                cellSize = EditorGUILayout.Vector2Field("Match Cell Size", cellSize);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button("Update Cell Size")) {
                    tgs.cellSize = cellSize;
                }
                EditorGUILayout.EndHorizontal();
            }

            tgs.gridMeshDepthOffset = EditorGUILayout.IntField("Mesh Depth Offset", tgs.gridMeshDepthOffset);
            tgs.gridSurfaceDepthOffset = EditorGUILayout.IntField("Cells Depth Offset", tgs.gridSurfaceDepthOffset);
            tgs.gridSurfaceDepthOffsetTerritory = EditorGUILayout.IntField("Territories Depth Offset", tgs.gridSurfaceDepthOffsetTerritory);

            if (tgs.terrain != null) {
                tgs.gridElevation = EditorGUILayout.Slider("Elevation", tgs.gridElevation, 0f, 5f);
                tgs.gridElevationBase = EditorGUILayout.FloatField("Elevation Base", tgs.gridElevationBase);
                tgs.gridMinElevationMultiplier = EditorGUILayout.FloatField(new GUIContent("Min Elevation Multiplier", "Grid, cells and territories meshes are rendered with a minimum gap to preserve correct order. This value is the scale for that gap."), tgs.gridMinElevationMultiplier);
                tgs.gridCameraOffset = EditorGUILayout.Slider("Camera Offset", tgs.gridCameraOffset, 0, 0.1f);
                tgs.gridNormalOffset = EditorGUILayout.Slider("Normal Offset", tgs.gridNormalOffset, 0.00f, 5f);
            }

            tgs.cameraMain = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "The camera used for some calculations. Main camera is picked by default."), tgs.cameraMain, typeof(Camera), true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            if (tgs.numTerritories > 0) {
                DrawTitleLabel("Territories Appearance");
                tgs.showTerritories = EditorGUILayout.Toggle("Show Frontiers", tgs.showTerritories);
                if (tgs.showTerritories) {
                    EditorGUI.indentLevel++;
                    tgs.territoryFrontiersColor = EditorGUILayout.ColorField(new GUIContent("Frontier Color"), tgs.territoryFrontiersColor, true, true, true);
                    tgs.territoryDisputedFrontierColor = EditorGUILayout.ColorField(new GUIContent("Disputed Frontier", "Color for common frontiers between two territories."), tgs.territoryDisputedFrontierColor, true, true, true);
                    tgs.territoryFrontiersThickness = EditorGUILayout.FloatField("Thickness", tgs.territoryFrontiersThickness);
                    tgs.showTerritoriesOuterBorders = EditorGUILayout.Toggle("Outer Borders", tgs.showTerritoriesOuterBorders);
                    EditorGUI.indentLevel--;
                }
                tgs.showTerritoriesInteriorBorders = EditorGUILayout.Toggle("Show Interior Borders", tgs.showTerritoriesInteriorBorders);
                if (tgs.showTerritoriesInteriorBorders) {
                    EditorGUI.indentLevel++;
                    tgs.territoryInteriorBorderThickness = EditorGUILayout.FloatField("Thickness", tgs.territoryInteriorBorderThickness);
                    tgs.territoryInteriorBorderPadding = EditorGUILayout.FloatField("Padding", tgs.territoryInteriorBorderPadding);
                    EditorGUI.indentLevel--;
                }
                tgs.colorizeTerritories = EditorGUILayout.Toggle("Colorize Territories", tgs.colorizeTerritories);
                if (tgs.colorizeTerritories) {
                    EditorGUI.indentLevel++;
                    tgs.colorizedTerritoriesAlpha = EditorGUILayout.Slider("Alpha", tgs.colorizedTerritoriesAlpha, 0.0f, 1.0f);
                    EditorGUI.indentLevel--;
                }
                tgs.allowTerritoriesInsideTerritories = EditorGUILayout.Toggle(new GUIContent("Internal Territories", "Allows territories to be contained by other territories."), tgs.allowTerritoriesInsideTerritories);
                tgs.territoryHighlightColor = EditorGUILayout.ColorField(new GUIContent("Highlight Color"), tgs.territoryHighlightColor, true, true, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            DrawTitleLabel("Cells Appearance");

            tgs.showCells = EditorGUILayout.Toggle("Show Cells", tgs.showCells);
            if (tgs.showCells) {
                tgs.cellBorderColor = EditorGUILayout.ColorField(new GUIContent("Border Color"), tgs.cellBorderColor, true, true, true);
                tgs.cellCustomBorderThickness = EditorGUILayout.Toggle(new GUIContent("Custom Thickness"), tgs.cellCustomBorderThickness);
                if (tgs.cellCustomBorderThickness) {
                    tgs.cellBorderThickness = EditorGUILayout.FloatField("Thickness", tgs.cellBorderThickness);
                }
            }
            tgs.cellHighlightColor = EditorGUILayout.ColorField(new GUIContent("Highlight Color"), tgs.cellHighlightColor, true, true, true);
            EditorGUI.indentLevel++;
            tgs.cellHighlightUseCellColor = EditorGUILayout.Toggle("Use Cell Color", tgs.cellHighlightUseCellColor);
            if (tgs.gridTopology == GridTopology.Box) {
                tgs.cellHighlightBorderSize = EditorGUILayout.Slider("Border Size", tgs.cellHighlightBorderSize, 0, 0.5f);
                tgs.cellHighlightBorderColor = EditorGUILayout.ColorField("Border Color", tgs.cellHighlightBorderColor);
            }
            EditorGUI.indentLevel--;

            float highlightFadeMin = tgs.highlightFadeMin;
            float highlightFadeAmount = tgs.highlightFadeAmount;
            EditorGUILayout.MinMaxSlider("Highlight Fade", ref highlightFadeMin, ref highlightFadeAmount, 0.0f, 1.0f);

            tgs.highlightFadeMin = highlightFadeMin;
            tgs.highlightFadeAmount = highlightFadeAmount;

            tgs.highlightFadeSpeed = EditorGUILayout.Slider("Highlight Speed", tgs.highlightFadeSpeed, 0.1f, 5.0f);
            tgs.highlightEffect = (HighlightEffect)EditorGUILayout.EnumPopup("Highlight Effect", tgs.highlightEffect);

            if (tgs.highlightEffect == HighlightEffect.TextureScale) {
                EditorGUILayout.BeginHorizontal();
                float highlightScaleMin = tgs.highlightScaleMin;
                float highlightScaleMax = tgs.highlightScaleMax;
                EditorGUILayout.MinMaxSlider("      Scale Range", ref highlightScaleMin, ref highlightScaleMax, 0.0f, 2.0f);
                if (GUILayout.Button("Default", GUILayout.Width(60))) {
                    highlightScaleMin = 0.75f;
                    highlightScaleMax = 1.1f;
                }
                tgs.highlightScaleMin = highlightScaleMin;
                tgs.highlightScaleMax = highlightScaleMax;
                EditorGUILayout.EndHorizontal();
            } else if (tgs.highlightEffect == HighlightEffect.DualColors) {
                tgs.cellHighlightColor2 = EditorGUILayout.ColorField(new GUIContent("Cell Alternate Color"), tgs.cellHighlightColor2, true, true, true);
                tgs.territoryHighlightColor2 = EditorGUILayout.ColorField(new GUIContent("Territory Alternate Color"), tgs.territoryHighlightColor2, true, true, true);
            }

            tgs.cellFillPadding = EditorGUILayout.FloatField(new GUIContent("Fill Padding", "Padding or separation applied to cells when they're filled with color/texture."), tgs.cellFillPadding);

            if (tgs.terrain != null) {
                tgs.nearClipFadeEnabled = EditorGUILayout.Toggle(new GUIContent("Near Clip Fade", "Fades out the cell and territories lines near to the camera."), tgs.nearClipFadeEnabled);
                if (tgs.nearClipFadeEnabled) {
                    tgs.nearClipFade = EditorGUILayout.FloatField("   Distance", tgs.nearClipFade);
                    tgs.nearClipFadeFallOff = EditorGUILayout.FloatField("   FallOff", tgs.nearClipFadeFallOff);
                }
                tgs.farFadeEnabled = EditorGUILayout.Toggle(new GUIContent("Far Distance Fade", "Fades out the cell and territories lines far from the camera."), tgs.farFadeEnabled);
                if (tgs.farFadeEnabled) {
                    tgs.farFadeDistance = EditorGUILayout.FloatField("   Distance", tgs.farFadeDistance);
                    tgs.farFadeFallOff = EditorGUILayout.FloatField("   FallOff", tgs.farFadeFallOff);
                }
            }
            tgs.circularFadeEnabled = EditorGUILayout.Toggle(new GUIContent("Circular Fade", "Fades out the cell and territories lines with respect to a gameobject position."), tgs.circularFadeEnabled);
            if (tgs.circularFadeEnabled) {
                tgs.circularFadeTarget = (Transform)EditorGUILayout.ObjectField("   Target", tgs.circularFadeTarget, typeof(Transform), true);
                tgs.circularFadeDistance = EditorGUILayout.FloatField("   Distance", tgs.circularFadeDistance);
                tgs.circularFadeFallOff = EditorGUILayout.FloatField("   FallOff", tgs.circularFadeFallOff);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            DrawTitleLabel("Grid Interaction");

            tgs.highlightMode = (HighlightMode)EditorGUILayout.Popup("Selection Mode", (int)tgs.highlightMode, selectionModeOptions);
            EditorGUI.indentLevel++;
            tgs.cellHighlightNonVisible = EditorGUILayout.Toggle("Include Invisible Cells", tgs.cellHighlightNonVisible);
            tgs.highlightMinimumTerrainDistance = EditorGUILayout.FloatField(new GUIContent("Minimum Distance", "Minimum distance of cell/territory to camera to be selectable. Useful in first person view to prevent selecting cells already under character."), tgs.highlightMinimumTerrainDistance);
            tgs.allowHighlightWhileDragging = EditorGUILayout.Toggle(new GUIContent("Highlight While Drag", "Allows highlight while moving the mouse over the grid during a drag operation not started on the grid."), tgs.allowHighlightWhileDragging);
            EditorGUI.indentLevel--;

            tgs.overlayMode = (OverlayMode)EditorGUILayout.Popup("Overlay Mode", (int)tgs.overlayMode, overlayModeOptions);
            tgs.respectOtherUI = EditorGUILayout.Toggle("Respect Other UI", tgs.respectOtherUI);
            tgs.blockingMask = LayerMaskField(new GUIContent("Blocking Mask", "Used to block interaction when other objects block raycast."), tgs.blockingMask);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            DrawTitleLabel("Path Finding");

            tgs.pathFindingHeuristicFormula = (TGS.PathFinding.HeuristicFormula)EditorGUILayout.EnumPopup("Algorithm", tgs.pathFindingHeuristicFormula);
            tgs.pathFindingMaxCost = EditorGUILayout.FloatField("Max Search Cost", tgs.pathFindingMaxCost);
            tgs.pathFindingMaxSteps = EditorGUILayout.IntField("Max Steps", tgs.pathFindingMaxSteps);
            tgs.pathFindingIncludeInvisibleCells = EditorGUILayout.Toggle("Include Invisible Cells", tgs.pathFindingIncludeInvisibleCells);

            if (tgs.gridTopology == GridTopology.Box) {
                tgs.pathFindingUseDiagonals = EditorGUILayout.Toggle("Use Diagonals", tgs.pathFindingUseDiagonals);
                EditorGUI.indentLevel++;
                tgs.pathFindingHeavyDiagonalsCost = EditorGUILayout.FloatField("Diagonals Cost", tgs.pathFindingHeavyDiagonalsCost);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            if (!Application.isPlaying) {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                DrawTitleLabel("Grid Editor");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Export Grid Config")) {
                    if (EditorUtility.DisplayDialog("Export Grid Config", "A TGS Config component will be atteched to this game object with current cell settings. You can restore this configuration just enabling this new component.\nContinue?", "Ok", "Cancel")) {
                        ExportGridConfig();
                    }
                }
                if (GUILayout.Button("Export Grid Mesh")) {
                    if (EditorUtility.DisplayDialog("Export Grid Mesh", "A copy of each territory grid mesh will be created and assigned to a new gameobject. This operation does not modify current grid.\nContinue?", "Ok", "Cancel")) {
                        tgs.ExportTerritoriesMesh();
                    }
                }
                if (GUILayout.Button("Reset")) {
                    if (EditorUtility.DisplayDialog("Reset Grid", "Reset cells to their default values?", "Ok", "Cancel")) {
                        ResetCells();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();

                tgs.enableGridEditor = EditorGUILayout.Toggle(new GUIContent("Enable Editor", "Enables grid editing options in Scene View"), tgs.enableGridEditor);

                if (tgs.enableGridEditor) {
                    int selectedCount = cellSelectedIndices.Count;
                    if (selectedCount == 0) {
                        GUILayout.Label("Click on a cell in Scene View to edit its properties\n(use Control or Shift to select multiple cells)");
                    } else {
                        // Check that all selected cells are within range
                        for (int k = 0; k < selectedCount; k++) {
                            if (cellSelectedIndices[k] < 0 || cellSelectedIndices[k] >= tgs.cellCount) {
                                cellSelectedIndices.Clear();
                                GUIUtility.ExitGUI();
                                return;
                            }
                        }

                        int cellSelectedIndex = cellSelectedIndices[0];
                        EditorGUILayout.BeginHorizontal();
                        if (selectedCount == 1) {
                            EditorGUILayout.LabelField("Selected Cell", cellSelectedIndex.ToString());
                        } else {
                            sb.Length = 0;
                            for (int k = 0; k < selectedCount; k++) {
                                if (k > 0) {
                                    sb.Append(", ");
                                }
                                sb.Append(cellSelectedIndices[k].ToString());
                            }
                            if (selectedCount > 5) {
                                EditorGUILayout.LabelField("Selected Cells");
                                GUILayout.TextArea(sb.ToString(), GUILayout.ExpandHeight(true));
                            } else {
                                EditorGUILayout.LabelField("Selected Cells", sb.ToString());
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        bool needsRedraw = false;

                        Cell selectedCell = tgs.cells[cellSelectedIndex];
                        bool cellVisible = selectedCell.visible;
                        selectedCell.visible = EditorGUILayout.Toggle("   Visible", cellVisible);
                        if (selectedCell.visible != cellVisible) {
                            for (int k = 0; k < selectedCount; k++) {
                                tgs.cells[cellSelectedIndices[k]].visible = selectedCell.visible;
                            }
                            needsRedraw = true;
                        }

                        bool canCross = selectedCell.canCross;
                        selectedCell.canCross = EditorGUILayout.Toggle(new GUIContent("   Can Cross", "This cell can be crossed when calculating a route using path finding."), canCross);
                        if (selectedCell.canCross != canCross) {
                            for (int k = 0; k < selectedCount; k++) {
                                tgs.cells[cellSelectedIndices[k]].canCross = selectedCell.canCross;
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        cellCrossCost = EditorGUILayout.FloatField(new GUIContent("   Cross Cost", "Cost to cross this cell using path finding."), cellCrossCost);
                        GUI.enabled = cellCrossCost != selectedCell.GetSidesCost();
                        if (GUILayout.Button("Set Cost", GUILayout.Width(100))) {
                            for (int k = 0; k < selectedCount; k++) {
                                tgs.cells[cellSelectedIndices[k]].SetAllSidesCost(cellCrossCost);
                            }
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        if (selectedCount == 1) {
                            EditorGUILayout.BeginHorizontal();
                            cellTag = EditorGUILayout.IntField("   Tag", cellTag);
                            if (cellTag == selectedCell.tag)
                                GUI.enabled = false;
                            if (GUILayout.Button("Set Tag", GUILayout.Width(100))) {
                                tgs.CellSetTag(cellSelectedIndex, cellTag);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                        EditorGUILayout.BeginHorizontal();
                        cellTerritoryIndex = EditorGUILayout.IntField("   Territory Index", cellTerritoryIndex);
                        if (cellTerritoryIndex == selectedCell.territoryIndex)
                            GUI.enabled = false;
                        if (GUILayout.Button("Set Territory", GUILayout.Width(100))) {
                            for (int k = 0; k < selectedCount; k++) {
                                tgs.CellSetTerritory(cellSelectedIndices[k], cellTerritoryIndex);
                            }
                            needsRedraw = true;
                        }
                        EditorGUILayout.EndHorizontal();
                        GUI.enabled = true;
                        if (cellTerritoryIndex >= 0) {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(GUIContent.none);
                            if (GUILayout.Button("Export Territory Mesh")) {
                                if (EditorUtility.DisplayDialog("Export Territory Mesh", "A copy of the grid mesh of current territory will be created and assigned to a new gameobject. This operation does not modify current grid.\nContinue?", "Ok", "Cancel")) {
                                    tgs.ExportTerritoryMesh(cellTerritoryIndex);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        cellColor = EditorGUILayout.ColorField(new GUIContent("   Color"), cellColor, true, true, true);
                        cellTextureIndex = EditorGUILayout.IntField("   Texture", cellTextureIndex);
                        if (tgs.CellGetColor(cellSelectedIndex) == cellColor && tgs.CellGetTextureIndex(cellSelectedIndex) == cellTextureIndex)
                            GUI.enabled = false;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(labelWidth));
                        if (GUILayout.Button("Assign Color & Texture")) {
                            for (int k = 0; k < selectedCount; k++) {
                                GameObject o = tgs.CellToggleRegionSurface(cellSelectedIndices[k], true, cellColor, true, cellTextureIndex);
                                o.transform.parent.gameObject.hideFlags = 0;
                                o.hideFlags = 0;
                            }
                            needsRedraw = true;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("Clear Cell")) {
                            for (int k = 0; k < selectedCount; k++) {
                                tgs.CellHideRegionSurface(cellSelectedIndices[k]);
                            }
                            needsRedraw = true;
                        }
                        EditorGUILayout.EndHorizontal();


                        if (needsRedraw) {
                            RefreshGrid();
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }

                    GUILayout.Label("Textures", GUILayout.Width(labelWidth));

                    if (toggleButtonStyleNormal == null) {
                        toggleButtonStyleNormal = "Button";
                        toggleButtonStyleToggled = new GUIStyle(toggleButtonStyleNormal);
                        toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
                    }

                    if (tgs.textures != null) {
                        int textureMax = tgs.textures.Length - 1;
                        while (textureMax >= 1 && tgs.textures[textureMax] == null) {
                            textureMax--;
                        }
                        textureMax++;
                        if (textureMax >= tgs.textures.Length)
                            textureMax = tgs.textures.Length - 1;

                        for (int k = 1; k <= textureMax; k++) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("   " + k.ToString(), GUILayout.Width(40));
                            tgs.textures[k] = (Texture2D)EditorGUILayout.ObjectField(tgs.textures[k], typeof(Texture2D), false);
                            if (tgs.textures[k] != null) {
                                if (GUILayout.Button(new GUIContent("T", "Texture mode - if enabled, you can paint several cells just clicking over them."), textureMode == k ? toggleButtonStyleToggled : toggleButtonStyleNormal, GUILayout.Width(20))) {
                                    textureMode = textureMode == k ? 0 : k;
                                }
                                if (GUILayout.Button(new GUIContent("X", "Remove texture"), GUILayout.Width(20))) {
                                    if (EditorUtility.DisplayDialog("Remove texture", "Are you sure you want to remove this texture?", "Yes", "No")) {
                                        tgs.textures[k] = null;
                                        GUIUtility.ExitGUI();
                                        return;
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Separator();

            if (tgs.isDirty) {
                serializedObject.UpdateIfRequiredOrScript();
                if (isDirty == null)
                    OnEnable();
                isDirty.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);

                // Hide mesh in Editor
                HideEditorMesh();

                SceneView.RepaintAll();
            }
        }

        void OnSceneGUI() {
            if (tgs == null || Application.isPlaying || !tgs.enableGridEditor)
                return;
            if (tgs.terrain != null) {
                // prevents terrain from being selected
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            Event e = Event.current;
            bool gridHit = tgs.CheckRay(HandleUtility.GUIPointToWorldRay(e.mousePosition));
            if (cellHighlightedIndex != tgs.cellHighlightedIndex) {
                cellHighlightedIndex = tgs.cellHighlightedIndex;
                SceneView.RepaintAll();
            }
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EventType eventType = e.GetTypeForControl(controlID);
            if ((eventType == EventType.MouseDown && e.button == 0) || (eventType == EventType.MouseMove && e.shift)) {
                if (gridHit) {
                    e.Use();
                }
                if (cellHighlightedIndex < 0) {
                    return;
                }
                if (!e.shift && cellSelectedIndices.Contains(cellHighlightedIndex)) {
                    cellSelectedIndices.Remove(cellHighlightedIndex);
                } else {
                    if (!e.shift || (e.shift && !cellSelectedIndices.Contains(cellHighlightedIndex))) {
                        if (!e.shift && !e.control) {
                            cellSelectedIndices.Clear();
                        }
                        cellSelectedIndices.Add(cellHighlightedIndex);
                        if (textureMode > 0) {
                            tgs.CellToggleRegionSurface(cellHighlightedIndex, true, Color.white, true, textureMode);
                            SceneView.RepaintAll();
                        }
                        if (cellHighlightedIndex >= 0) {
                            cellTerritoryIndex = tgs.CellGetTerritoryIndex(cellHighlightedIndex);
                            cellColor = tgs.CellGetColor(cellHighlightedIndex);
                            if (cellColor.a == 0)
                                cellColor = Color.white;
                            cellTextureIndex = tgs.CellGetTextureIndex(cellHighlightedIndex);
                            cellTag = tgs.CellGetTag(cellHighlightedIndex);
                            cellCrossCost = tgs.CellGetCrossCost(cellHighlightedIndex);
                        }
                    }
                }
                EditorUtility.SetDirty(target);
            }
            int count = cellSelectedIndices.Count;
            for (int k = 0; k < count; k++) {
                int index = cellSelectedIndices[k];
                Vector3 pos = tgs.CellGetPosition(index);
                Handles.color = colorSelection;
                // Handle size
                Rect rect = tgs.CellGetRect(index);
                Vector3 min = tgs.transform.TransformPoint(rect.min);
                Vector3 max = tgs.transform.TransformPoint(rect.max);
                float dia = Vector3.Distance(min, max);
                float handleSize = dia * 0.05f;
                Handles.DrawSolidDisc(pos, tgs.transform.forward, handleSize);
            }
        }

        #region Utility functions

        Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            TextureFormat tf = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
            Texture2D result = new Texture2D(width, height, tf, false);
            result.hideFlags = HideFlags.DontSave;
            result.SetPixels(pix);
            result.Apply();

            return result;
        }


        LayerMask LayerMaskField(GUIContent label, LayerMask layerMask) {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "") {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++) {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++) {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }


        void HideEditorMesh() {
            Renderer[] rr = tgs.GetComponentsInChildren<Renderer>(true);
            for (int k = 0; k < rr.Length; k++) {
#if UNITY_5_5_OR_NEWER
                EditorUtility.SetSelectedRenderState(rr[k], EditorSelectedRenderState.Hidden);
#else
				EditorUtility.SetSelectedWireframeHidden (rr [k], true);
#endif
            }
        }


        void DrawTitleLabel(string s) {
            GUIStyle titleLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            titleLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.22f, 0.36f, 0.6f);
            GUILayout.Label(s, titleLabelStyle);
        }

        void DrawInfoLabel(string s) {
            if (infoLabelStyle == null)
                infoLabelStyle = new GUIStyle(GUI.skin.label);
            infoLabelStyle.normal.textColor = new Color(0.76f, 0.52f, 0.52f);
            GUILayout.Label(s, infoLabelStyle);
        }

        void ResetCells() {
            TGSConfig[] cc = tgs.GetComponents<TGSConfig>();
            for (int k = 0; k < cc.Length; k++) {
                cc[k].enabled = false;
            }
            cellSelectedIndices.Clear();
            cellColor = Color.white;
            tgs.GenerateMap();
            RefreshGrid();
        }

        void RefreshGrid() {
            tgs.Redraw();
            HideEditorMesh();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }

        void ExportGridConfig() {
            TGSConfig configComponent = tgs.gameObject.AddComponent<TGSConfig>();
            configComponent.SaveConfiguration(tgs);
            configComponent.enabled = false;
        }

        bool CheckTextureImportSettings(Texture2D tex) {
            if (tex == null)
                return false;
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path))
                return false;
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp != null && !imp.isReadable) {
                EditorGUILayout.HelpBox("Texture is not readable. Fix it?", MessageType.Warning);
                if (GUILayout.Button("Fix texture import setting")) {
                    imp.isReadable = true;
                    imp.SaveAndReimport();
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Editor integration

        [MenuItem("GameObject/3D Object/Terrain Grid System", false)]
        static void CreateTGSMenuOption(MenuCommand menuCommand) {
            GameObject go = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/TerrainGridSystem"));
            go.name = "Terrain Grid System";
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            if (Terrain.activeTerrain != null) {
                TerrainGridSystem tgs = go.GetComponent<TerrainGridSystem>();
                if (tgs != null && Terrain.activeTerrain != null) {
                    tgs.terrainObject = Terrain.activeTerrain.gameObject;
                }
            } else {
                go.transform.rotation = Quaternion.Euler(90, 0, 0);
            }

        }


        #endregion
    }

}