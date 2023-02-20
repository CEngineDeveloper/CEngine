using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TGS.PathFinding;

namespace TGS {

    public partial class TerrainGridSystem : MonoBehaviour {

        int[] routeMatrix;

        IPathFinder finder;
        bool needRefreshRouteMatrix;

        bool clearanceComputed;
        int clearanceCellGroupMask;

        void ComputeRouteMatrix(bool needsClearance) {

            // prepare matrix
            if (routeMatrix == null || routeMatrix.Length == 0) {
                needRefreshRouteMatrix = true;
                routeMatrix = new int[_cellColumnCount * _cellRowCount];
            }

            if (!needRefreshRouteMatrix)
                return;

            needRefreshRouteMatrix = false;

            // Compute route
            for (int j = 0; j < _cellRowCount; j++) {
                int jj = j * _cellColumnCount;
                for (int k = 0; k < _cellColumnCount; k++) {
                    int cellIndex = jj + k;
                    Cell cell = cells[cellIndex];
                    if (cell != null && cell.canCross && cell.visible) {    // set navigation bit
                        routeMatrix[cellIndex] = cell.group;
                    } else {        // clear navigation bit
                        routeMatrix[cellIndex] = 0;
                    }
                }
            }

            if (finder == null) {
                if (_gridTopology == GridTopology.Irregular) {
                    finder = new PathFinderFastIrregular(cells.ToArray(), _cellColumnCount, _cellRowCount);
                } else {
                    if ((_cellColumnCount & (_cellColumnCount - 1)) == 0) { // is power of two?
                        finder = new PathFinderFast(cells.ToArray(), _cellColumnCount, _cellRowCount);
                    } else {
                        finder = new PathFinderFastNonSQR(cells.ToArray(), _cellColumnCount, _cellRowCount);
                    }
                }
            } else {
                finder.SetCalcMatrix(cells.ToArray());
            }
        }


        void ComputeClearance(int cellGroupMask) {

            if (clearanceComputed && clearanceCellGroupMask == cellGroupMask) return;

            clearanceComputed = true;
            clearanceCellGroupMask = cellGroupMask;

            int cellsCount = cells.Count;
            // clear clearance
            for (int k = 0; k < cellsCount; k++) {
                cells[k].clearance = 0;
            }

            int maxDim = Mathf.Max(rowCount, columnCount);
            // uses true clearance
            for (int j = rowCount - 1; j >= 0; j--) {
                for (int k = 0; k < columnCount; k++) {
                    Cell cell = CellGetAtPosition(k, j);
                    if (cell == null) continue;
                    for (int maxClearance = 2; maxClearance < maxDim; maxClearance++) {
                        bool blocked = false;
                        int maxIter = maxClearance * maxClearance;
                        for (int i = 1; i < maxIter; i++) {
                            int nj = j - (i / maxClearance);
                            int nk = k + (i % maxClearance);
                            if (nj < 0 || nk >= columnCount) {
                                blocked = true;
                                break;
                            }
                            Cell neighbour = CellGetAtPosition(nk, nj);
                            if (neighbour == null || (neighbour.group & cellGroupMask) == 0 || !neighbour.canCross) {
                                blocked = true;
                                break;
                            }
                        }
                        if (blocked) {
                            cell.clearance = (byte)(maxClearance - 1);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Used by FindRoute method to satisfy custom positions check
        /// </summary>
        float FindRoutePositionValidator(TerrainGridSystem grid, int cellIndex) {
            float cost = 1;
            if (OnPathFindingCrossCell != null) {
                cost = OnPathFindingCrossCell(grid, cellIndex);
            }
            return cost;
        }

    }

}