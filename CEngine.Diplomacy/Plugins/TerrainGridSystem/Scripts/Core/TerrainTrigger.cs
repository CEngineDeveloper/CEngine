using UnityEngine;
using System.Collections.Generic;

namespace TGS {
    public class TerrainTrigger : MonoBehaviour {
        readonly List<TerrainGridSystem> grids = new List<TerrainGridSystem>();
        readonly RaycastHit[] hits = new RaycastHit[20];

        public void Init<T>(TerrainGridSystem tgs) where T : Component {
            if (!grids.Contains(tgs)) grids.Add(tgs);
            if (GetComponent<T>() == null) {
                gameObject.AddComponent<T>();
            }
        }

        void OnMouseEnter() {
            foreach (TerrainGridSystem grid in grids) {
                if (grid != null) {
                    grid.mouseIsOver = true;
                }
            }
        }

        void OnMouseExit() {
            foreach (TerrainGridSystem grid in grids) {
                // Make sure it's outside of grid
                Vector3 mousePos = grid.input.mousePosition;
                Camera cam = grid.cameraMain;
                Ray ray = cam.ScreenPointToRay(mousePos);
                int hitCount = Physics.RaycastNonAlloc(cam.transform.position, ray.direction, hits);
                if (hitCount > 0) {
                    for (int k = 0; k < hitCount; k++) {
                        if (hits[k].collider.gameObject == gameObject)
                            continue;
                    }
                }
                grid.mouseIsOver = false;
            }
        }

    }

}