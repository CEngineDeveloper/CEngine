using UnityEngine;

namespace TGS {

    public delegate void CellEvent(TerrainGridSystem tgs, int cellIndex);
    public delegate void CellHighlightEvent(TerrainGridSystem tgs, int cellIndex, ref bool cancelHighlight);
    public delegate void CellClickEvent(TerrainGridSystem tgs, int cellIndex, int buttonIndex);
    public delegate void CellDragEvent(TerrainGridSystem tgs, int cellOriginIndex, int cellTargetIndex);

    public delegate void TerritoryEvent(TerrainGridSystem tgs, int territoryIndex);
    public delegate void TerritoryRegionEvent(TerrainGridSystem tgs, int territoryIndex, int regionIndex);
    public delegate void TerritoryHighlightEvent(TerrainGridSystem tgs, int territoryIndex, ref bool cancelHighlight);
    public delegate void TerritoryClickEvent(TerrainGridSystem tgs, int territoryIndex, int buttonIndex);
    public delegate void TerritoryRegionClickEvent(TerrainGridSystem tgs, int territoryIndex, int regionIndex, int buttonIndex);

    public delegate int PathFindingEvent(TerrainGridSystem tgs, int cellIndex);

    public delegate void OnGridSettingsChangedEvent(TerrainGridSystem tgs);

    public delegate void OnRectangleSelectionEvent(TerrainGridSystem tgs, Vector2 localStartPos, Vector2 localEndPos);



    public partial class TerrainGridSystem : MonoBehaviour {

        #region Cell events

        /// <summary>
        /// Occurs when the pointer enters a cell
        /// </summary>
        public event CellEvent OnCellEnter;

        /// <summary>
        /// Occurs when the pointer exits a cell
        /// </summary>
        public event CellEvent OnCellExit;

        /// <summary>
        /// Occurs when user presses the mouse button on a cell
        /// </summary>
        public event CellClickEvent OnCellMouseDown;

        /// <summary>
        /// Occurs when user releases the mouse button on the same cell that started clicking
        /// </summary>
        public event CellClickEvent OnCellClick;

        /// <summary>
        /// Occurs when user releases the mouse button on any cell
        /// </summary>
        public event CellClickEvent OnCellMouseUp;

        /// <summary>
        /// Occurs when a cell is about to get highlighted
        /// </summary>
        public event CellHighlightEvent OnCellHighlight;

        /// <summary>
        /// Occurs when user starts dragging on a cell
        /// </summary>
        public event CellEvent OnCellDragStart;

        /// <summary>
        /// Occurs when user drags a cell
        /// </summary>
        public event CellDragEvent OnCellDrag;

        /// <summary>
        /// Occurs when user ends drag on a cell
        /// </summary>
        public event CellDragEvent OnCellDragEnd;

        #endregion

        #region Territory events

        /// <summary>
        /// Occurs when the pointer enters a territory
        /// </summary>
        public event TerritoryEvent OnTerritoryEnter;

        /// <summary>
        /// Occurs when the pointer exits a territory
        /// </summary>
        public event TerritoryEvent OnTerritoryExit;

        /// <summary>
        /// Occurs when the pointer enters a territory region
        /// </summary>
        public event TerritoryRegionEvent OnTerritoryRegionEnter;

        /// <summary>
        /// Occurs when the pointer exits a territory region
        /// </summary>
        public event TerritoryRegionEvent OnTerritoryRegionExit;

        /// <summary>
        /// Occurs when user press the mouse button on a territory
        /// </summary>
        public event TerritoryClickEvent OnTerritoryMouseDown;

        /// <summary>
        /// Occurs when user releases the mouse button on the same territory that started clicking
        /// </summary>
        public event TerritoryClickEvent OnTerritoryClick;

        /// <summary>
        /// Occurs when user releases the mouse button on a territory
        /// </summary>
        public event TerritoryClickEvent OnTerritoryMouseUp;

        /// <summary>
        /// Occurs when user press the mouse button on a territory region
        /// </summary>
        public event TerritoryRegionClickEvent OnTerritoryRegionMouseDown;

        /// <summary>
        /// Occurs when user releases the mouse button on the same territory region that started clicking
        /// </summary>
        public event TerritoryRegionClickEvent OnTerritoryRegionClick;

        /// <summary>
        /// Occurs when user releases the mouse button on a territory region
        /// </summary>
        public event TerritoryRegionClickEvent OnTerritoryRegionMouseUp;

        /// <summary>
        /// Occurs when a territory is about to get highlighted
        /// </summary>
        public event TerritoryHighlightEvent OnTerritoryHighlight;

        #endregion

        #region Rectangle selection events

        /// <summary>
        /// Occurs when user performs a rectangle selection
        /// </summary>
        public OnRectangleSelectionEvent OnRectangleSelection;

        /// <summary>
        /// Occurs when user starts draggins a rectangle selection
        /// </summary>
        public OnRectangleSelectionEvent OnRectangleDrag;

        #endregion


        #region Other events

        /// <summary>
        /// Fired when path finding algorithmn evaluates a cell. Return the increased cost for cell.
        /// </summary>
        public event PathFindingEvent OnPathFindingCrossCell;

        /// <summary>
        /// Occurs when some grid settings are changed
        /// </summary>
        public event OnGridSettingsChangedEvent OnGridSettingsChanged;

        #endregion

    }
}

