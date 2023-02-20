using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;
using TGS.PathFinding;

namespace TGS {

    public enum HighlightMode {
        None = 0,
        Territories = 1,
        Cells = 2
    }

    public enum OverlayMode {
        Overlay = 0,
        Ground = 1
    }

    public enum GridTopology {
        Irregular = 0,
        Box = 1,
        //		Rectangular = 2,	// deprecated: use Box
        Hexagonal = 3
    }

    public enum HighlightEffect {
        Default = 0,
        DualColors = 6,
        TextureAdditive = 1,
        TextureMultiply = 2,
        TextureColor = 3,
        TextureScale = 4,
        None = 5
    }

    public enum CentroidType {
        GeometricCenter = 0,
        Centroid = 1,
        BetterCentroid = 2
    }


    public partial class TerrainGridSystem : MonoBehaviour {

        /// <summary>
        /// Access to the input class
        /// </summary>
        public IInputProxy input;


        [SerializeField]
        Terrain _terrain;

        [SerializeField]
        GameObject _terrainObject;

        /// <summary>
        /// Terrain reference. Assign a terrain to this property to fit the grid to terrain height and dimensions
        /// </summary>
        public GameObject terrainObject {
            get {
                return _terrainObject;
            }
            set {
                if (_terrainObject != value) {
                    _terrainObject = value;
                    _terrainWrapper = null;
                    BuildTerrainWrapper();
                    isDirty = true;
                    Redraw();
                }
            }
        }

        [SerializeField]
        bool _multiTerrain;

        public bool multiTerrain {
            get { return _multiTerrain; }
            set {
                if (_multiTerrain != value) {
                    _multiTerrain = value;
                    _terrainWrapper = null;
                    BuildTerrainWrapper();
                    isDirty = true;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Sets the terrain gameobject. This is equivalent to use the terrainObject property.
        /// </summary>
        public void SetTerrain(GameObject terrain) {
            terrainObject = terrain;
        }


        [SerializeField]
        string _terrainObjectsPrefix;

        public string terrainObjectsPrefix {
            get {
                return _terrainObjectsPrefix;
            }
            set {
                if (_terrainObjectsPrefix != value) {
                    _terrainObjectsPrefix = value;
                    _terrainWrapper = null;
                    BuildTerrainWrapper();
                    isDirty = true;
                    Redraw();
                }
            }
        }


        [SerializeField]
        LayerMask _terrainObjectsLayerMask = -1;

        public LayerMask terrainObjectsLayerMask {
            get {
                return _terrainObjectsLayerMask;
            }
            set {
                if (_terrainObjectsLayerMask != value) {
                    _terrainObjectsLayerMask = value;
                    _terrainWrapper = null;
                    BuildTerrainWrapper();
                    isDirty = true;
                    Redraw();
                }
            }
        }


        [SerializeField]
        bool _terrainObjectsSearchGlobal;

        public bool terrainObjectsSearchGlobal {
            get {
                return _terrainObjectsSearchGlobal;
            }
            set {
                if (_terrainObjectsSearchGlobal != value) {
                    _terrainObjectsSearchGlobal = value;
                    _terrainWrapper = null;
                    BuildTerrainWrapper();
                    isDirty = true;
                    Redraw();
                }
            }
        }

        // Terrain data wrapper
        ITerrainWrapper _terrainWrapper;

        public ITerrainWrapper terrain {
            get { return _terrainWrapper; }
        }

        /// <summary>
        /// Returns the terrain center in world space.
        /// </summary>
        public Vector3 terrainCenter {
            get {
                return _terrainWrapper.transform.position + new Vector3(terrainWidth * 0.5f, 0, terrainDepth * 0.5f);
            }
        }

        [SerializeField]
        Vector2 _terrainMeshPivot = new Vector2(0.5f, 0.5f);

        public Vector2 terrainMeshPivot {
            get { return _terrainMeshPivot; }
            set {
                if (_terrainMeshPivot != value) {
                    _terrainMeshPivot = value;
                    if (_terrainWrapper != null && _terrainWrapper is MeshTerrainWrapper) {
                        ((MeshTerrainWrapper)_terrainWrapper).pivot = _terrainMeshPivot;
                        gridNeedsUpdate = true;
                    }
                }
            }
        }

        public Texture2D canvasTexture;

        [SerializeField]
        bool _transparentBackground;

        /// <summary>
        /// When enabled, grid is visible without background mesh
        /// </summary>
        public bool transparentBackground {
            get {
                return _transparentBackground;
            }
            set {
                if (_transparentBackground != value) {
                    _transparentBackground = value;
                    isDirty = true;
                    Redraw(true);
                }
            }
        }

        [SerializeField]
        bool _useStencilBuffer = true;

        /// <summary>
        /// When enabled, stencil buffer will be used to avoid overdraw and ensure correct rendering order of grid features. You can disable this option if it creates conflicts with other stencil-based renderers.
        /// </summary>
        public bool useStencilBuffer {
            get {
                return _useStencilBuffer;
            }
            set {
                if (_useStencilBuffer != value) {
                    _useStencilBuffer = value;
                    isDirty = true;
                    UpdatePreventOverdrawSettings();
                }
            }
        }


        /// <summary>
        /// Returns the current cursor position in grid local coordinates (use transform.TransformPoint(x) to convert to world space)
        /// </summary>
        public Vector3 localCursorPosition {
            get {
                return localCursorPos;
            }
        }


        /// <summary>
        /// Returns the position of the last click on the grid (in local coordinates. Use transform.TransformPoint(x) to convert to world space)
        /// </summary>
        public Vector3 localCursorLastClickedPosition {
            get {
                return localCursorLastClickedPos;
            }
        }



        [SerializeField]
        bool _useGeometryShaders = true;

        /// <summary>
        /// When enabled, geometry shaders will be used (if platform supports them)
        /// </summary>
        public bool useGeometryShaders {
            get {
                return _useGeometryShaders;
            }
            set {
                if (_useGeometryShaders != value) {
                    _useGeometryShaders = value;
                    isDirty = true;
                    territoriesGeoMat = territoriesDisputedGeoMat = cellsGeoMat = null;
                    LoadGeometryShaders();
                    Redraw(true);
                }
            }
        }


        [SerializeField]
        int _sortingOrder;

        /// <summary>
        /// Sets the sorting layer for the grid elements (only valid when rendering in transparent queue)
        /// </summary>
        public int sortingOrder {
            get { return _sortingOrder; }
            set {
                if (_sortingOrder != value) {
                    _sortingOrder = value;
                    Redraw(true);
                }
            }
        }


        [UnityEngine.Serialization.FormerlySerializedAs("_heightMapSize")]
        [SerializeField]
        int _heightmapWidth = 513;

        public int heightmapWidth {
            get { return _heightmapWidth; }
            set {
                value = GetFittedSizeForHeightmap(value);
                if (_heightmapWidth != value) {
                    _heightmapWidth = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        int _heightmapHeight = 513;

        public int heightmapHeight {
            get { return _heightmapHeight; }
            set {
                value = GetFittedSizeForHeightmap(value);
                if (_heightmapHeight != value) {
                    _heightmapHeight = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Texture2D _gridMask;

        /// <summary>
        /// Gets or sets the grid mask. The alpha component of this texture is used to determine cells visibility (0 = cell invisible)
        /// </summary>
        public Texture2D gridMask {
            get { return _gridMask; }
            set {
                if (_gridMask != value) {
                    _gridMask = value;
                    isDirty = true;
                    ReloadGridMask();
                }
            }
        }


        [SerializeField]
        bool _gridMaskUseScale;

        /// <summary>
        /// When set to true, the mask will be mapped onto the scaled grid rectangle (using offset and scale parameters) instead of the full quad which ignores the offset and scale
        /// </summary>
        public bool gridMaskUseScale {
            get {
                return _gridMaskUseScale;
            }
            set {
                if (_gridMaskUseScale != value) {
                    _gridMaskUseScale = value;
                    isDirty = true;
                    Redraw(true);
                }
            }
        }



        [SerializeField]
        int _gridMaskInsideCount = 1;

        /// <summary>
        /// Minimum number of vertices that must be inside the grid mask to consider the entire cell is inside the mask
        /// </summary>
        public int gridMaskInsideCount {
            get {
                return _gridMaskInsideCount;
            }
            set {
                value = Mathf.Max(1, value);
                if (_gridMaskInsideCount != value) {
                    _gridMaskInsideCount = value;
                    isDirty = true;
                    Redraw(true);
                }
            }
        }


        [SerializeField]
        GridTopology _gridTopology = GridTopology.Irregular;

        /// <summary>
        /// The grid type (boxed, hexagonal or irregular)
        /// </summary>
        public GridTopology gridTopology {
            get { return _gridTopology; }
            set {
                if (_gridTopology != value) {
                    _gridTopology = value;
                    needGenerateMap = true;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        int _seed = 1;

        /// <summary>
        /// Randomize seed used to generate cells. Use this to control randomization.
        /// </summary>
        public int seed {
            get { return _seed; }
            set {
                if (_seed != value) {
                    _seed = value;
                    needGenerateMap = true;
                    isDirty = true;
                }
            }
        }


        /// <summary>
        /// Returns the actual number of cells created according to the current grid topology
        /// </summary>
        /// <value>The cell count.</value>
        public int cellCount {
            get {
                if (_gridTopology == GridTopology.Irregular) {
                    return _numCells;
                } else {
                    return _cellRowCount * _cellColumnCount;
                }
            }
        }

        [SerializeField]
        bool _evenLayout = false;

        /// <summary>
        /// Toggle even corner in hexagonal topology.
        /// </summary>
        public bool evenLayout {
            get {
                return _evenLayout;
            }
            set {
                if (value != _evenLayout) {
                    _evenLayout = value;
                    isDirty = true;
                    needGenerateMap = true;
                }
            }
        }


        [SerializeField]
        bool _pointyTopHexagons;

        public bool pointyTopHexagons {
            get { return _pointyTopHexagons; }
            set {
                if (value != _pointyTopHexagons) {
                    _pointyTopHexagons = value;
                    isDirty = true;
                    needGenerateMap = true;
                }
            }
        }

        [SerializeField]
        bool _regularHexagons;

        public bool regularHexagons {
            get { return _regularHexagons; }
            set {
                if (value != _regularHexagons) {
                    _regularHexagons = value;
                    isDirty = true;
                    CellsUpdateBounds();
                    UpdateTerritoryBoundaries();
                    Redraw();
                }
            }
        }

        [SerializeField]
        float _hexSize = 0.01f;

        [SerializeField]
        float _regularHexagonsWidth;

        public float regularHexagonsWidth {
            get { return _regularHexagonsWidth; }
            set {
                if (value != _regularHexagonsWidth) {
                    _regularHexagonsWidth = value;
                    ComputeGridScale();
                    isDirty = true;
                    CellsUpdateBounds();
                    UpdateTerritoryBoundaries();
                    Redraw();
                }
            }
        }

        [SerializeField]
        int _gridRelaxation = 1;

        /// <summary>
        /// Sets the relaxation iterations used to normalize cells sizes in irregular topology.
        /// </summary>
        public int gridRelaxation {
            get { return _gridRelaxation; }
            set {
                if (_gridRelaxation != value) {
                    _gridRelaxation = value;
                    needGenerateMap = true;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float _gridCurvature = 0.0f;

        /// <summary>
        /// Gets or sets the grid's curvature factor.
        /// </summary>
        public float gridCurvature {
            get { return _gridCurvature; }
            set {
                if (_gridCurvature != value) {
                    _gridCurvature = value;
                    needGenerateMap = true;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        HighlightMode _highlightMode = HighlightMode.Cells;

        public HighlightMode highlightMode {
            get {
                return _highlightMode;
            }
            set {
                if (_highlightMode != value) {
                    _highlightMode = value;
                    isDirty = true;
                    ClearLastOver();
                    HideCellRegionHighlight();
                    HideTerritoryRegionHighlight();
                    CheckCells();
                    CheckTerritories();
                }
            }
        }

        [SerializeField]
        bool _allowHighlightWhileDragging = false;

        public bool allowHighlightWhileDragging {
            get {
                return _allowHighlightWhileDragging;
            }
            set {
                if (_allowHighlightWhileDragging != value) {
                    _allowHighlightWhileDragging = value;
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        float _highlightFadeMin = 0f;

        public float highlightFadeMin {
            get {
                return _highlightFadeMin;
            }
            set {
                if (_highlightFadeMin != value) {
                    _highlightFadeMin = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _highlightFadeAmount = 0.5f;

        public float highlightFadeAmount {
            get {
                return _highlightFadeAmount;
            }
            set {
                if (_highlightFadeAmount != value) {
                    _highlightFadeAmount = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _highlightScaleMin = 0.75f;

        public float highlightScaleMin {
            get {
                return _highlightScaleMin;
            }
            set {
                if (_highlightScaleMin != value) {
                    _highlightScaleMin = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        float _highlightScaleMax = 1.1f;

        public float highlightScaleMax {
            get {
                return _highlightScaleMax;
            }
            set {
                if (_highlightScaleMax != value) {
                    _highlightScaleMax = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _highlightFadeSpeed = 1f;

        public float highlightFadeSpeed {
            get {
                return _highlightFadeSpeed;
            }
            set {
                if (_highlightFadeSpeed != value) {
                    _highlightFadeSpeed = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        float _highlightMinimumTerrainDistance = 1f;

        /// <summary>
        /// Minimum distance from camera for cells to be highlighted on terrain
        /// </summary>
        public float highlightMinimumTerrainDistance {
            get {
                return _highlightMinimumTerrainDistance;
            }
            set {
                if (_highlightMinimumTerrainDistance != value) {
                    _highlightMinimumTerrainDistance = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        HighlightEffect _highlightEffect = HighlightEffect.Default;

        public HighlightEffect highlightEffect {
            get {
                return _highlightEffect;
            }
            set {
                if (_highlightEffect != value) {
                    _highlightEffect = value;
                    isDirty = true;
                    UpdateHighlightEffect();
                }
            }
        }

        [SerializeField]
        OverlayMode _overlayMode = OverlayMode.Overlay;

        public OverlayMode overlayMode {
            get {
                return _overlayMode;
            }
            set {
                if (_overlayMode != value) {
                    _overlayMode = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        Vector2 _gridCenter;

        /// <summary>
        /// Center of the grid relative to the Terrain (by default, 0,0, which means center of terrain)
        /// </summary>
        public Vector2 gridCenter {
            get { return _gridCenter; }
            set {
                if (_gridCenter != value) {
                    _gridCenter = value;
                    isDirty = true;
                    CellsUpdateBounds();
                    UpdateTerritoryBoundaries();
                    Redraw(true);
                }
            }
        }

        /// <summary>
        /// Center of the grid in world space coordinates. You can also use this property to reposition the grid on a given world position coordinate.
        /// </summary>
        public Vector3 gridCenterWorldPosition {
            get { return GetWorldSpacePosition(_gridCenter); }
            set { SetGridCenterWorldPosition(value, false); }
        }


        [SerializeField]
        Vector2 _gridScale = new Vector2(1, 1);

        /// <summary>
        /// Scale of the grid on the Terrain (by default, 1,1, which means occupy entire terrain)
        /// </summary>
        public Vector2 gridScale {
            get { return _gridScale; }
            set {
                if (_gridScale != value) {
                    _gridScale = value;
                    ComputeGridScale();
                    isDirty = true;
                    CellsUpdateBounds();
                    UpdateTerritoryBoundaries();
                    Redraw(true);
                }
            }
        }


        [SerializeField]
        float _gridElevation = 0;

        public float gridElevation {
            get { return _gridElevation; }
            set {
                if (_gridElevation != value) {
                    _gridElevation = value;
                    isDirty = true;
                    FitToTerrain();
                }
            }
        }

        [SerializeField]
        float _gridElevationBase = 0;

        public float gridElevationBase {
            get { return _gridElevationBase; }
            set {
                if (_gridElevationBase != value) {
                    _gridElevationBase = value;
                    isDirty = true;
                    FitToTerrain();
                }
            }
        }

        public float gridElevationCurrent { get { return _gridElevation + _gridElevationBase; } }


        [SerializeField]
        float _gridMinElevationMultiplier = 1f;

        public float gridMinElevationMultiplier {
            get { return _gridMinElevationMultiplier; }
            set {
                if (_gridMinElevationMultiplier != value && value >= 0) {
                    _gridMinElevationMultiplier = value;
                    isDirty = true;
                    Redraw(true);
                }
            }
        }

        [SerializeField]
        float _gridCameraOffset = 0;

        public float gridCameraOffset {
            get { return _gridCameraOffset; }
            set {
                if (_gridCameraOffset != value) {
                    _gridCameraOffset = value;
                    isDirty = true;
                    FitToTerrain();
                }
            }
        }


        [SerializeField]
        float _gridNormalOffset = 0;

        public float gridNormalOffset {
            get { return _gridNormalOffset; }
            set {
                if (_gridNormalOffset != value) {
                    _gridNormalOffset = value;
                    isDirty = true;
                    Redraw();
                }
            }
        }

        [Obsolete("Use gridMeshDepthOffset or gridSurfaceDepthOffset.")]
        public int gridDepthOffset {
            get { return _gridMeshDepthOffset; }
            set { gridMeshDepthOffset = value; }
        }

        [SerializeField]
        int _gridMeshDepthOffset = -1;

        public int gridMeshDepthOffset {
            get { return _gridMeshDepthOffset; }
            set {
                if (_gridMeshDepthOffset != value) {
                    _gridMeshDepthOffset = value;
                    UpdateMaterialDepthOffset();
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        int _gridSurfaceDepthOffset = -1;

        public int gridSurfaceDepthOffset {
            get { return _gridSurfaceDepthOffset; }
            set {
                if (_gridSurfaceDepthOffset != value) {
                    _gridSurfaceDepthOffset = value;
                    UpdateMaterialDepthOffset();
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        int _gridSurfaceDepthOffsetTerritory = -1;

        public int gridSurfaceDepthOffsetTerritory {
            get { return _gridSurfaceDepthOffsetTerritory; }
            set {
                if (_gridSurfaceDepthOffsetTerritory != value) {
                    _gridSurfaceDepthOffsetTerritory = value;
                    UpdateMaterialDepthOffset();
                    isDirty = true;
                }
            }
        }



        [SerializeField]
        float _gridRoughness = 0.01f;

        public float gridRoughness {
            get { return _gridRoughness; }
            set {
                if (_gridRoughness != value) {
                    _gridRoughness = value;
                    isDirty = true;
                    Redraw();
                }
            }
        }

        [SerializeField]
        int _cellRowCount = 8;

        /// <summary>
        /// Returns the number of rows for box and hexagonal grid topologies
        /// </summary>
        public int rowCount {
            get {
                return _cellRowCount;
            }
            set {
                if (value != _cellRowCount) {
                    _cellRowCount = Mathf.Clamp(value, 2, MAX_ROWS_OR_COLUMNS);
                    isDirty = true;
                    needGenerateMap = true;
                    CheckGridChanges();
                }
            }

        }


        [SerializeField]
        int _cellColumnCount = 8;

        /// <summary>
        /// Returns the number of columns for box and hexagonal grid topologies
        /// </summary>
        public int columnCount {
            get {
                return _cellColumnCount;
            }
            set {
                if (value != _cellColumnCount) {
                    _cellColumnCount = Mathf.Clamp(value, 2, MAX_ROWS_OR_COLUMNS);
                    isDirty = true;
                    needGenerateMap = true;
                    CheckGridChanges();
                }
            }
        }


        /// <summary>
        /// Sets the dimensions of the grid in one step. This is faster than setting rowCount and columnCount separately.
        /// </summary>
        /// <param name="rows">Rows.</param>
        /// <param name="columns">Columns.</param>
        /// <param name="keepCellSize">Ensures the individual cell size is preserved</param>
        public void SetDimensions(int rows, int columns, bool keepCellSize = false) {
            _cellRowCount = Mathf.Clamp(rows, 2, MAX_ROWS_OR_COLUMNS);
            _cellColumnCount = Mathf.Clamp(columns, 2, MAX_ROWS_OR_COLUMNS);
            if (keepCellSize) {
                SetScaleByCellSize();
            }
            isDirty = true;
            needGenerateMap = true;
        }


        public Texture2D[] textures;


        [SerializeField]
        bool
            _respectOtherUI = true;

        /// <summary>
        /// When enabled, will prevent interaction if pointer is over an UI element
        /// </summary>
        public bool respectOtherUI {
            get { return _respectOtherUI; }
            set {
                if (value != _respectOtherUI) {
                    _respectOtherUI = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        LayerMask _blockingMask;

        /// <summary>
        /// Used to cancel highlighting then some other objects block raycasting
        /// </summary>
        public LayerMask blockingMask {
            get { return _blockingMask; }
            set {
                if (value != _blockingMask) {
                    _blockingMask = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
            _nearClipFadeEnabled;

        /// <summary>
        /// When enabled, lines near the camera will fade out gracefully
        /// </summary>
        public bool nearClipFadeEnabled {
            get { return _nearClipFadeEnabled; }
            set {
                if (value != _nearClipFadeEnabled) {
                    _nearClipFadeEnabled = value;
                    isDirty = true;
                    UpdateMaterialNearClipFade();
                }
            }
        }


        [SerializeField]
        float _nearClipFade = 25f;

        public float nearClipFade {
            get { return _nearClipFade; }
            set {
                if (_nearClipFade != value) {
                    _nearClipFade = Mathf.Max(0, value);
                    isDirty = true;
                    UpdateMaterialNearClipFade();
                }
            }
        }

        [SerializeField]
        float _nearClipFadeFallOff = 50f;

        public float nearClipFadeFallOff {
            get { return _nearClipFadeFallOff; }
            set {
                if (_nearClipFadeFallOff != value) {
                    _nearClipFadeFallOff = Mathf.Max(value, 0.001f);
                    isDirty = true;
                    UpdateMaterialNearClipFade();
                }
            }
        }


        [SerializeField]
        bool
            _farFadeEnabled;

        /// <summary>
        /// When enabled, lines far the camera will fade out gracefully
        /// </summary>
        public bool farFadeEnabled {
            get { return _farFadeEnabled; }
            set {
                if (value != _farFadeEnabled) {
                    _farFadeEnabled = value;
                    isDirty = true;
                    UpdateMaterialFarFade();
                }
            }
        }


        [SerializeField]
        float _farFadeDistance = 500f;

        public float farFadeDistance {
            get { return _farFadeDistance; }
            set {
                if (_farFadeDistance != value) {
                    _farFadeDistance = Mathf.Max(0, value);
                    isDirty = true;
                    UpdateMaterialFarFade();
                }
            }
        }

        [SerializeField]
        float _farFadeFallOff = 50f;

        public float farFadeFallOff {
            get { return _farFadeFallOff; }
            set {
                if (_farFadeFallOff != value) {
                    _farFadeFallOff = Mathf.Max(value, 0.001f);
                    isDirty = true;
                    UpdateMaterialFarFade();
                }
            }
        }


        [SerializeField]
        bool
            _circularFadeEnabled;

        /// <summary>
        /// When enabled, grid is faded out according to distance to a gameoject
        /// </summary>
        public bool circularFadeEnabled {
            get { return _circularFadeEnabled; }
            set {
                if (value != _circularFadeEnabled) {
                    _circularFadeEnabled = value;
                    isDirty = true;
                    UpdateMaterialCircularFade();
                }
            }
        }


        [SerializeField]
        Transform _circularFadeTarget;

        /// <summary>
        /// The gameobject reference for the circular fade out effect
        /// </summary>
        public Transform circularFadeTarget {
            get { return _circularFadeTarget; }
            set {
                if (_circularFadeTarget != value) {
                    _circularFadeTarget = value;
                    isDirty = true;
                    UpdateMaterialCircularFade();
                }
            }
        }

        [SerializeField]
        float _circularFadeDistance = 500f;

        public float circularFadeDistance {
            get { return _circularFadeDistance; }
            set {
                if (_circularFadeDistance != value) {
                    _circularFadeDistance = Mathf.Max(0, value);
                    isDirty = true;
                    UpdateMaterialCircularFade();
                }
            }
        }

        [SerializeField]
        float _circularFadeFallOff = 50f;

        public float circularFadeFallOff {
            get { return _circularFadeFallOff; }
            set {
                if (_circularFadeFallOff != value) {
                    _circularFadeFallOff = Mathf.Max(value, 0.001f);
                    isDirty = true;
                    UpdateMaterialCircularFade();
                }
            }
        }

        [SerializeField]
        bool _enableGridEditor = true;

        /// <summary>
        /// Enabled grid editing options in Scene View
        /// </summary>
        public bool enableGridEditor {
            get {
                return _enableGridEditor;
            }
            set {
                if (value != _enableGridEditor) {
                    _enableGridEditor = value;
                    if (!_enableGridEditor && !Application.isPlaying) {
                        HideTerritoryRegionHighlight();
                        HideCellRegionHighlight();
                    }
                    isDirty = true;
                }
            }
        }


        public static TerrainGridSystem instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<TerrainGridSystem>();
                    if (_instance == null) {
                        Debug.LogWarning("TerrainGridSystem gameobject not found in the scene!");
                    } else {
                        if (_instance.cells == null) {
                            _instance.OnEnable();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Returns a reference of the currently highlighted gameobject (cell or territory)
        /// </summary>
        public GameObject highlightedObj { get { return _highlightedObj; } }

        /// <summary>
        /// The camera reference used in certain computations
        /// </summary>
        public Camera cameraMain;

        #region Public General Functions

        /// <summary>
        /// Used to cancel highlighting on a given gameobject. This call is ignored if go is not currently highlighted.
        /// </summary>
        public void HideHighlightedObject(GameObject go) {
            if (go != _highlightedObj)
                return;
            _cellHighlightedIndex = -1;
            _cellHighlighted = null;
            _territoryHighlightedIndex = -1;
            _territoryHighlightedRegionIndex = -1;
            _territoryRegionHighlighted = null;
            _territoryLastOverIndex = -1;
            _territoryRegionLastOverIndex = -1;
            _highlightedObj = null;
            ClearLastOver();
        }


        public void SetGridCenterWorldPosition(Vector3 position, bool snapToGrid) {
            if (snapToGrid) {
                position = SnapToCell(position, true, false);
            }
            if (_terrainWrapper != null) {
                gridCenter = _terrainWrapper.GetLocalPoint(_terrainObject, position);
            } else {
                transform.position = position;
            }
        }


        /// <summary>
        /// Snaps a position to the grid
        /// </summary>
        public Vector3 SnapToCell(Vector3 position, bool worldSpace = true, bool snapToCenter = true) {

            if (worldSpace) {
                position = transform.InverseTransformPoint(position);
            }
            position.x = (float)FastMath.Round(position.x, 6);
            position.y = (float)FastMath.Round(position.y, 6);
            if (_gridTopology == GridTopology.Box) {
                float stepX = _gridScale.x / _cellColumnCount;
                position.x -= _gridCenter.x;
                if (snapToCenter && _cellColumnCount % 2 == 0) {
                    position.x = (Mathf.FloorToInt(position.x / stepX) + 0.5f) * stepX;
                } else {
                    position.x = (Mathf.FloorToInt(position.x / stepX + 0.5f)) * stepX;
                }
                position.x += _gridCenter.x;
                float stepY = _gridScale.y / _cellRowCount;
                position.y -= _gridCenter.y;
                if (snapToCenter && _cellRowCount % 2 == 0) {
                    position.y = (Mathf.FloorToInt(position.y / stepY) + 0.5f) * stepY;
                } else {
                    position.y = (Mathf.FloorToInt(position.y / stepY + 0.5f)) * stepY;
                }
                position.y += _gridCenter.y;
            } else if (_gridTopology == GridTopology.Hexagonal) {

                if (snapToCenter) {
                    Cell cell = CellGetAtPosition(position);
                    if (cell != null) {
                        position = cell.scaledCenter;
                    }
                } else {
                    float qx = 1f + (_cellColumnCount - 1f) * 3f / 4f;
                    float qy = _cellRowCount + 0.5f;

                    float stepX = _gridScale.x / qx;
                    float stepY = _gridScale.y / qy;

                    float halfStepX = stepX * 0.5f;
                    float halfStepY = stepY * 0.5f;

                    int evenLayout = _evenLayout ? 1 : 0;

                    float k = Mathf.FloorToInt(position.x * _cellColumnCount / _gridScale.x);
                    float j = Mathf.FloorToInt(position.y * _cellRowCount / _gridScale.y);
                    position.x = k * stepX; // + halfStepX;
                    position.y = j * stepY;
                    position.x -= k * halfStepX / 2;
                    float offsetY = (k % 2 == evenLayout) ? 0 : -halfStepY;
                    position.y += offsetY;
                }

            } else {
                // try to get cell under position and returns its center
                Cell c = CellGetAtPosition(position);
                if (c != null) {
                    position = c.center;
                }
            }
            if (worldSpace) {
                position = transform.TransformPoint(position);
            }
            return position;
        }

        /// <summary>
        /// Returns the rectangle area where cells are drawn in local or world space coordinates.
        /// </summary>
        /// <returns>The rect.</returns>
        public Rect GetRect(bool worldSpace = true) {
            Rect rect = new Rect();
            Vector3 min = GetScaledVector(new Vector3(-0.5f, -0.5f, 0));
            Vector3 max = GetScaledVector(new Vector3(0.5f, 0.5f, 0));
            if (worldSpace) {
                min = transform.TransformPoint(min);
                max = transform.TransformPoint(max);
            }
            rect.min = min;
            rect.max = max;
            return rect;
        }


        /// <summary>
        /// Hides current highlighting effect
        /// </summary>
        public void HideHighlightedRegions() {
            HideTerritoryRegionHighlight();
            HideCellRegionHighlight();
        }


        public void ReloadGridMask() {
            ReadMaskContents();
            CellsApplyVisibilityFilters();
            recreateTerritories = true;
            Redraw();
            if (territoriesTexture != null) {
                CreateTerritories(territoriesTexture, territoriesTextureNeutralColor, territoriesHideNeutralCells);
            }
        }

        public void ReloadFlatMask() {
            ReadFlatMaskContents();
            CellsApplyVisibilityFilters();
            recreateTerritories = true;
            Redraw();
        }


        /// <summary>
        /// Escales the gameobject of a colored/textured surface
        /// </summary>
        /// <param name="surf">Surf.</param>
        /// <param name="center">Center.</param>
        /// <param name="scale">Scale.</param>
        public void ScaleSurface(GameObject surf, Vector2 center, float scale) {
            if (surf == null)
                return;
            Transform t = surf.transform;

            t.localScale = new Vector3(t.localScale.x * scale, t.localScale.y * scale, 1f);
            Vector3 originShift = center;
            originShift.x *= t.localScale.x;
            originShift.y *= t.localScale.y;
            originShift.x -= center.x;
            originShift.y -= center.y;
            originShift.z = 0;
            t.localPosition -= originShift;
        }

        /// <summary>
        /// Returns current bounds of grid in world space
        /// </summary>
        /// <value>The bounds.</value>
        public Bounds bounds {
            get {
                Vector3 min = transform.TransformPoint(GetScaledVector(new Vector3(-0.5f, -0.5f, 0)));
                Vector3 max = transform.TransformPoint(GetScaledVector(new Vector3(0.5f, 0.5f, 0)));
                Vector3 size = max - min;
                if (size.y <= 0) size.y = 0.0001f;
                return new Bounds((min + max) * 0.5f, size);
            }
        }

        /// <summary>
        /// Returns true if a given position lies within this grid on the X/Z plane
        /// </summary>
        /// <returns></returns>
        public bool Contains(Vector3 position) {
            Bounds bb = bounds;
            return position.x >= bb.min.x && position.x <= bb.max.x && position.z >= bb.min.z && position.z <= bb.max.z;
        }

        #endregion



    }
}

