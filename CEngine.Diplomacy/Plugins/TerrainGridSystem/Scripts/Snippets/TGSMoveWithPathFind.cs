using System.Collections.Generic;
using UnityEngine;

namespace TGS {
    [AddComponentMenu("Kronnect/Terrain Grid System/TGS Move With Path-find")]
    public class TGSMoveWithPathFind : TGSSnippetBase {

        public Vector3 targetPosition;
        public float maxSearchCost;
        public int maxSteps;
        public int cellGroupMask = -1;
        public CanCrossCheckType canCrossCheckType = CanCrossCheckType.Default;
        public bool ignoreCellCosts;
        public bool includeInvisibleCells;

        List<int> path;

        protected override void Configure() {
            instructions = "Move this gameobject to specified cell using path-finding.";
            supportsTweening = true;
        }


        protected override bool Prepare() {
            int startCellIndex = tgs.CellGetIndex(transform.position, worldSpace: true);
            if (startCellIndex < 0) {
                Debug.Log("Starting position is not inside the grid.");
                return false;
            }

            int targetCellIndex = tgs.CellGetIndex(targetPosition, worldSpace: true);
            if (targetCellIndex < 0) {
                Debug.Log("Target position is not inside the grid.");
                return false;
            }

            path = tgs.FindPath(startCellIndex, targetCellIndex, maxSearchCost, maxSteps, cellGroupMask, canCrossCheckType, ignoreCellCosts, includeInvisibleCells);
            return true;
        }

        protected override void Execute(float t) {
            int steps = path.Count;
            if (steps == 0) return;
            float k = steps * t;
            if (k >= steps) k = steps - 1;
            int i = (int)k;
            float f = k - i;
            Vector3 startPosition = tgs.CellGetPosition(path[i]);
            int j = i + 1;
            if (j >= steps) j = steps - 1;
            Vector3 endPosition = tgs.CellGetPosition(path[j]);
            transform.position = Vector3.Lerp(startPosition, endPosition, f);
        }

    }

}