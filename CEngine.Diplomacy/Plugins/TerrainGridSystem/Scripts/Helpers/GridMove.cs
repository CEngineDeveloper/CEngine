using UnityEngine;
using System.Collections.Generic;

namespace TGS {

    public delegate void MoveEvent(GameObject gameObject);
    public delegate void CellMoveEvent(Vector3 destination, int pathStepIndex);

    public class GridMove : MonoBehaviour {

        public TerrainGridSystem grid;
        public List<int> positions;
        public float velocity;
        public float elevation;
        public event MoveEvent OnMoveEnd;
        public event CellMoveEvent OnCellMoveEvent;
        Vector3 destination;
        int posIndex;
        float lastMoveTime;

        private void OnEnable() {
            lastMoveTime = Time.time;
        }

        public void Begin() {
            posIndex = 0;
            lastMoveTime = Time.time;
            if (velocity == 0) {
                if (positions != null) {
                    transform.position = grid.CellGetPosition(positions[positions.Count - 1], worldSpace: true, elevation);
                }
                Destroy(this);
                return;
            }
            ComputeNextDestination();
        }


        void Update() {
            float now = Time.time;
            float moveDistance = velocity * (now - lastMoveTime);
            lastMoveTime = now;

            float remainingDistance = Vector3.Distance(transform.position, destination);
            while (moveDistance >= remainingDistance) {
                moveDistance -= remainingDistance;
                transform.position = destination;
                if (!ComputeNextDestination()) return;
                remainingDistance = Vector3.Distance(transform.position, destination);
            }

            Vector3 moveDir = (destination - transform.position).normalized;
            transform.position += moveDir * moveDistance;

        }

        bool ComputeNextDestination() {
            if (positions == null || posIndex >= positions.Count) {
                if (OnMoveEnd != null) OnMoveEnd(gameObject);
                Destroy(this);
                return false;
            }
            destination = grid.CellGetPosition(positions[posIndex], worldSpace: true, elevation);
            if (OnCellMoveEvent != null) OnCellMoveEvent(destination, positions[posIndex]);
            posIndex++;
            return true;
        }
    }
}