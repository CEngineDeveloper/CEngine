//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
//  Some modifications by Kronnect to reuse grid buffers between calls and to allow different grid configurations in same grid array (uses bitwise differentiator)
//  Also including support for hexagonal grids and some other improvements

using UnityEngine;
using System;
using System.Collections.Generic;

namespace TGS.PathFinding {

    public class PathFinderFastNonSQR : IPathFinder {
        // Heap variables are initializated to default, but I like to do it anyway
        private Cell[] mGrid;
        private PriorityQueueB<int> mOpen;
        private List<PathFinderNode> mClose = new List<PathFinderNode>();
        private HeuristicFormula mFormula = HeuristicFormula.Manhattan;
        private bool mDiagonals = true;
        private CellType mCellShape;
        private float mHEstimate = 1;
        private float mHeavyDiagonalsCost = 1.4f;
        private int mMaxSteps = 2000;
        private float mMaxSearchCost = 100000;
        private PathFinderNodeFast[] mCalcGrid;
        private byte mOpenNodeValue = 1;
        private byte mCloseNodeValue = 2;
        private OnCellCross mOnCellCross;
        private int mMinClearance;

        //Promoted local variables to member variables to avoid recreation between calls
        private float mH = 0;
        private int mLocation = 0;
        private int mNewLocation = 0;
        private ushort mLocationX = 0;
        private ushort mLocationY = 0;
        private ushort mNewLocationX = 0;
        private ushort mNewLocationY = 0;
        private ushort mGridX = 0;
        private ushort mGridY = 0;
        private bool mFound = false;
        private sbyte[,] mDirectionBox = new sbyte[8, 2] {
            { 0, -1 },
            { 1, 0 },
            { 0, 1 },
            { -1, 0 },
            { 1, -1 },
            { 1, 1 },
            { -1, 1 },
            { -1, -1 }
        };
        private readonly sbyte[,] mDirectionFlatHex0 = new sbyte[6, 2] {
            { 0, -1 }, 
            { 1, 0 },
            { 0, 1 },
            { -1, 0 },
            { 1, 1 },
            { -1, 1 }
        };
        private readonly sbyte[,] mDirectionFlatHex1 = new sbyte[6, 2] {
            { 0, -1 },
            { 1, 0 },
            { 0, 1 },
            { -1, 0 },
            { -1, -1 },
            { 1, -1 }
        };
        private readonly int[] mCellFlatSide0 = new int[6] {
            (int)CELL_SIDE.Bottom,
            (int)CELL_SIDE.BottomRight,
            (int)CELL_SIDE.Top,
            (int)CELL_SIDE.BottomLeft,
            (int)CELL_SIDE.TopRight,
            (int)CELL_SIDE.TopLeft
        };
        private readonly int[] mCellFlatSide1 = new int[6] {
            (int)CELL_SIDE.Bottom,
            (int)CELL_SIDE.TopRight,
            (int)CELL_SIDE.Top,
            (int)CELL_SIDE.TopLeft,
            (int)CELL_SIDE.BottomLeft,
            (int)CELL_SIDE.BottomRight
        };
        private readonly sbyte[,] mDirectionPointyHex0 = new sbyte[6, 2] {
            { 0, -1 },
            { -1, 0 },
            { 0, 1 },
            { 1, 1 },
            { 1, 0 },
            { 1, -1 }
        };
        private readonly sbyte[,] mDirectionPointyHex1 = new sbyte[6, 2] {
            { -1, -1 },
            { -1, 0 },
            { -1, 1 },
            { 0, 1 },
            { 1, 0 },
            { 0, -1 }
        };
        private readonly int[] mCellPointySide0 = new int[6] {
            (int)CELL_SIDE.BottomLeft,
            (int)CELL_SIDE.Left,
            (int)CELL_SIDE.TopLeft,
            (int)CELL_SIDE.TopRight,
            (int)CELL_SIDE.Right,
            (int)CELL_SIDE.BottomRight
        };
        private readonly int[] mCellPointySide1 = new int[6] {
            (int)CELL_SIDE.BottomLeft,
            (int)CELL_SIDE.Left,
            (int)CELL_SIDE.TopLeft,
            (int)CELL_SIDE.TopRight,
            (int)CELL_SIDE.Right,
            (int)CELL_SIDE.BottomRight
        };
        private readonly int[] mCellBoxSides = new int[8] {
            (int)CELL_SIDE.Bottom,
            (int)CELL_SIDE.Right,
            (int)CELL_SIDE.Top,
            (int)CELL_SIDE.Left,
            (int)CELL_SIDE.BottomRight,
            (int)CELL_SIDE.TopRight,
            (int)CELL_SIDE.TopLeft,
            (int)CELL_SIDE.BottomLeft
        };
        private int mEndLocation = 0;
        private float mNewG = 0;
        private int mCellGroupMask = -1;
        private bool mIgnoreCanCrossCheck;
        private bool mIgnoreCellCost;
        private bool mIncludeInvisibleCells;

        public PathFinderFastNonSQR(Cell[] grid, int gridWidth, int gridHeight) {
            if (grid == null)
                throw new Exception("Grid cannot be null");

            mGrid = grid;
            mGridX = (ushort)gridWidth;
            mGridY = (ushort)gridHeight;

            if (mCalcGrid == null || mCalcGrid.Length != (mGridX * mGridY))
                mCalcGrid = new PathFinderNodeFast[mGridX * mGridY];

            mOpen = new PriorityQueueB<int>(new ComparePFNodeMatrix(mCalcGrid));
        }

        public void SetCalcMatrix(Cell[] grid) {
            if (grid == null)
                throw new Exception("Grid cannot be null");
            if (grid.Length != mGrid.Length) // mGridX != (ushort) (mGrid.GetUpperBound(0) + 1) || mGridY != (ushort) (mGrid.GetUpperBound(1) + 1))
                throw new Exception("SetCalcMatrix called with matrix with different dimensions. Call constructor instead.");
            mGrid = grid;

            Array.Clear(mCalcGrid, 0, mCalcGrid.Length);
            ComparePFNodeMatrix comparer = (ComparePFNodeMatrix)mOpen.comparer;
            comparer.SetMatrix(mCalcGrid);
        }

        public HeuristicFormula Formula {
            get { return mFormula; }
            set { mFormula = value; }
        }

        public bool Diagonals {
            get { return mDiagonals; }
            set {
                mDiagonals = value;
                if (mDiagonals)
                    mDirectionBox = new sbyte[8, 2] {
                        { 0, -1 },
                        { 1, 0 },
                        { 0, 1 },
                        { -1, 0 },
                        { 1, -1 },
                        { 1, 1 },
                        { -1, 1 },
                        { -1, -1 }
                    };
                else
                    mDirectionBox = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            }
        }

        public float HeavyDiagonalsCost {
            get { return mHeavyDiagonalsCost; }
            set { mHeavyDiagonalsCost = value; }
        }

        public CellType CellShape
        {
            get { return mCellShape; }
            set { mCellShape = value; }
        }

        public float HeuristicEstimate {
            get { return mHEstimate; }
            set { mHEstimate = value; }
        }

        public float MaxSearchCost {
            get { return mMaxSearchCost; }
            set { mMaxSearchCost = value; }
        }

        public int MaxSteps {
            get { return mMaxSteps; }
            set { mMaxSteps = value; }
        }


        public OnCellCross OnCellCross {
            get { return mOnCellCross; }
            set { mOnCellCross = value; }
        }

        public int CellGroupMask {
            get { return mCellGroupMask; }
            set { mCellGroupMask = value; }
        }

        public bool IgnoreCanCrossCheck {
            get { return mIgnoreCanCrossCheck; }
            set { mIgnoreCanCrossCheck = value; }
        }

        public bool IgnoreCellCost {
            get { return mIgnoreCellCost; }
            set { mIgnoreCellCost = value; }
        }

        public bool IncludeInvisibleCells {
            get { return mIncludeInvisibleCells; }
            set { mIncludeInvisibleCells = value; }
        }

        public int MinClearance {
            get { return mMinClearance; }
            set { mMinClearance = value; }
        }

        public List<PathFinderNode> FindPath(TerrainGridSystem tgs, Cell startCell, Cell endCell, out float totalCost, bool evenLayout) {
            PathFindingPoint start = new PathFindingPoint(startCell.column, startCell.row);
            PathFindingPoint end = new PathFindingPoint(endCell.column, endCell.row);

            totalCost = 0;
            mFound = false;
            int evenLayoutValue = evenLayout ? 1 : 0;
            if (mOpenNodeValue > 250) {
                Array.Clear(mCalcGrid, 0, mCalcGrid.Length);
                mOpenNodeValue = 1;
                mCloseNodeValue = 2;
            } else {
                mOpenNodeValue += 2;
                mCloseNodeValue += 2;
            }
            mOpen.Clear();
            mClose.Clear();
            int maxi;
            bool isHexagonal = mCellShape == CellType.FlatTopHexagon || mCellShape == CellType.PointyTopHexagon;
            if (isHexagonal)
            {
                maxi = 6;
            } else {
                maxi = mDiagonals ? 8 : 4;
            }

            mLocation = (start.y * mGridX) + start.x;
            mEndLocation = (end.y * mGridX) + end.x;
            mCalcGrid[mLocation].G = 0;
            mCalcGrid[mLocation].F = mHEstimate;
            mCalcGrid[mLocation].PX = (ushort)start.x;
            mCalcGrid[mLocation].PY = (ushort)start.y;
            mCalcGrid[mLocation].Status = mOpenNodeValue;
            mCalcGrid[mLocation].Steps = 0;

            mOpen.Push(mLocation);
            while (mOpen.Count > 0) {
                mLocation = mOpen.Pop();

                //Is it in closed list? means this node was already processed
                if (mCalcGrid[mLocation].Status == mCloseNodeValue)
                    continue;

                if (mLocation == mEndLocation) {
                    mCalcGrid[mLocation].Status = mCloseNodeValue;
                    mFound = true;
                    break;
                }

                mLocationX = (ushort)(mLocation % mGridX);
                mLocationY = (ushort)(mLocation / mGridX);

                //Lets calculate each successors
                bool hasSideCosts = false;
                float[] sideCosts = mGrid[mLocation].crossCost;
                if (!mIgnoreCellCost && sideCosts != null) {
                    hasSideCosts = true;
                }

                for (int i = 0; i < maxi; i++) {

                    int cellSide;
                    if (mCellShape == CellType.FlatTopHexagon)
                    {
                        if (mLocationX % 2 == evenLayoutValue)
                        {
                            mNewLocationX = (ushort)(mLocationX + mDirectionFlatHex0[i, 0]);
                            mNewLocationY = (ushort)(mLocationY + mDirectionFlatHex0[i, 1]);
                            cellSide = mCellFlatSide0[i];
                        }
                        else
                        {
                            mNewLocationX = (ushort)(mLocationX + mDirectionFlatHex1[i, 0]);
                            mNewLocationY = (ushort)(mLocationY + mDirectionFlatHex1[i, 1]);
                            cellSide = mCellFlatSide1[i];
                        }
                    }
                    else if (mCellShape == CellType.PointyTopHexagon)
                    {
                        if (mLocationY % 2 == evenLayoutValue)
                        {
                            mNewLocationX = (ushort)(mLocationX + mDirectionPointyHex0[i, 0]);
                            mNewLocationY = (ushort)(mLocationY + mDirectionPointyHex0[i, 1]);
                            cellSide = mCellPointySide0[i];
                        }
                        else
                        {
                            mNewLocationX = (ushort)(mLocationX + mDirectionPointyHex1[i, 0]);
                            mNewLocationY = (ushort)(mLocationY + mDirectionPointyHex1[i, 1]);
                            cellSide = mCellPointySide1[i];
                        }

                    }
                    else
                    {
                        mNewLocationX = (ushort)(mLocationX + mDirectionBox[i, 0]);
                        mNewLocationY = (ushort)(mLocationY + mDirectionBox[i, 1]);
                        cellSide = mCellBoxSides[i];
                    }

                    if (mNewLocationY >= mGridY)
                        continue;

                    if (mNewLocationX >= mGridX)
                        continue;

                    // Unbreakeable?
                    mNewLocation = (mNewLocationY * mGridX) + mNewLocationX;
                    if (!mGrid[mNewLocation].canCross && !mIgnoreCanCrossCheck)
                        continue;

                    Cell ncell = mGrid[mNewLocation];

                    if (ncell.clearance < mMinClearance) {
                        continue;
                    }

                    if (!mIncludeInvisibleCells && !ncell.visible)
                        continue;

                    float gridValue = (ncell.group & mCellGroupMask) != 0 ? 1 : 0;
                    if (gridValue == 0)
                        continue;

                    if (hasSideCosts) {
                        gridValue = sideCosts[cellSide];
                        if (gridValue <= 0)
                            gridValue = 1;
                    }                                                                               // Check custom validator
                    if (mOnCellCross != null) {
                        gridValue += mOnCellCross(tgs, mNewLocation);
                    }

                    if (!isHexagonal && i > 3)
                        mNewG = mCalcGrid[mLocation].G + gridValue * mHeavyDiagonalsCost;
                    else
                        mNewG = mCalcGrid[mLocation].G + gridValue;

                    if (mNewG > mMaxSearchCost || mCalcGrid[mLocation].Steps >= mMaxSteps)
                        continue;

                    //Is it open or closed?
                    if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue) {
                        // The current node has less code than the previous? then skip this node
                        if (mCalcGrid[mNewLocation].G <= mNewG)
                            continue;
                    }

                    mCalcGrid[mNewLocation].PX = mLocationX;
                    mCalcGrid[mNewLocation].PY = mLocationY;
                    mCalcGrid[mNewLocation].G = mNewG;
                    mCalcGrid[mNewLocation].Steps = mCalcGrid[mLocation].Steps + 1;

                    int dist = Math.Abs(mNewLocationX - end.x);
                    switch (mFormula) {
                        default:
                        case HeuristicFormula.Manhattan:
                            mH = mHEstimate * (dist + Math.Abs(mNewLocationY - end.y));
                            break;
                        case HeuristicFormula.MaxDXDY:
                            mH = mHEstimate * (Math.Max(dist, Math.Abs(mNewLocationY - end.y)));
                            break;
                        case HeuristicFormula.DiagonalShortCut:
                            int h_diagonal = Math.Min(dist, Math.Abs(mNewLocationY - end.y));
                            int h_straight = (dist + Math.Abs(mNewLocationY - end.y));
                            mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                            break;
                        case HeuristicFormula.Euclidean:
                            mH = mHEstimate * (float)(Math.Sqrt(Math.Pow(dist, 2) + Math.Pow((mNewLocationY - end.y), 2)));
                            break;
                        case HeuristicFormula.EuclideanNoSQR:
                            mH = mHEstimate * (float)(Math.Pow(dist, 2) + Math.Pow((mNewLocationY - end.y), 2));
                            break;
                        case HeuristicFormula.Custom1:
                            PathFindingPoint dxy = new PathFindingPoint(dist, Math.Abs(end.y - mNewLocationY));
                            float Orthogonal = Math.Abs(dxy.x - dxy.y);
                            float Diagonal = Math.Abs(((dxy.x + dxy.y) - Orthogonal) / 2);
                            mH = mHEstimate * (Diagonal + Orthogonal + dxy.x + dxy.y);
                            break;
                    }
                    mCalcGrid[mNewLocation].F = mNewG + mH;

                    mOpen.Push(mNewLocation);
                    mCalcGrid[mNewLocation].Status = mOpenNodeValue;
                }

                mCalcGrid[mLocation].Status = mCloseNodeValue;
            }

            if (mFound) {
                mClose.Clear();

                PathFinderNodeFast fNodeTmp = mCalcGrid[(end.y * mGridX) + end.x];
                totalCost = fNodeTmp.G;
                PathFinderNode fNode;
                fNode.F = fNodeTmp.F;
                fNode.G = fNodeTmp.G;
                fNode.H = 0;
                fNode.PX = fNodeTmp.PX;
                fNode.PY = fNodeTmp.PY;
                fNode.X = end.x;
                fNode.Y = end.y;

                while (fNode.X != fNode.PX || fNode.Y != fNode.PY) {
                    mClose.Add(fNode);
                    int posX = fNode.PX;
                    int posY = fNode.PY;
                    fNodeTmp = mCalcGrid[(posY * mGridX) + posX];
                    fNode.F = fNodeTmp.F;
                    fNode.G = fNodeTmp.G;
                    fNode.H = 0;
                    fNode.PX = fNodeTmp.PX;
                    fNode.PY = fNodeTmp.PY;
                    fNode.X = posX;
                    fNode.Y = posY;
                }

                //																mClose.Add (fNode);

                return mClose;
            }
            return null;
        }

        internal class ComparePFNodeMatrix : IComparer<int> {
            protected PathFinderNodeFast[] mMatrix;

            public ComparePFNodeMatrix(PathFinderNodeFast[] matrix) {
                mMatrix = matrix;
            }

            public int Compare(int a, int b) {
                if (mMatrix[a].F > mMatrix[b].F)
                    return 1;
                else if (mMatrix[a].F < mMatrix[b].F)
                    return -1;
                return 0;
            }

            public void SetMatrix(PathFinderNodeFast[] matrix) {
                mMatrix = matrix;
            }
        }
    }
}
