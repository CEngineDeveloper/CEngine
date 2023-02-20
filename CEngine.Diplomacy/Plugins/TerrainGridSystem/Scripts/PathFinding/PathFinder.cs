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
//  Modifications by Kronnect


using System;
using System.Collections.Generic;

namespace TGS.PathFinding {

    public struct PathFinderNode
    {
        public float     F;
		public float     G;
		public float     H;  // f = gone + heuristic
        public int     X;
        public int     Y;
        public int     PX; // Parent
        public int     PY;
    }

    public enum PathFinderNodeType
    {
        Start   = 1,
        End     = 2,
        Open    = 4,
        Close   = 8,
        Current = 16,
        Path    = 32
    }

    public enum HeuristicFormula
    {
        Manhattan           = 1,
        MaxDXDY             = 2,
        DiagonalShortCut    = 3,
        Euclidean           = 4,
        EuclideanNoSQR      = 5,
        Custom1             = 6
    }

	internal class ComparePFNode : IComparer<PathFinderNode>
	{
		public int Compare(PathFinderNode x, PathFinderNode y)
		{
			if (x.F > y.F)
				return 1;
			else if (x.F < y.F)
				return -1;
			return 0;
		}
	}

}
