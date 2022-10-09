using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CYM.AI.NodesMap
{
    [ExecuteInEditMode]
    public class Node : MonoBehaviour
    {
        NodeMap nodeMap => NodeMap.Ins;

        [SerializeField]
        public string Type = "None";
        [SceneObjectsOnly]
        public List<Node> Nodes = new List<Node>();

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            bool isSelect = Selection.Contains(gameObject);
            if (nodeMap.showPath || isSelect)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                if (isSelect)
                {
                    Gizmos.color = new Color(0.8f, 0.8f, 0f, 0.5f);
                }

                Gizmos.DrawSphere(transform.position, NodeMap.Ins.nodeScale);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    Node curNode = Nodes[i];
                    if (curNode == null)
                    {
                        Nodes.RemoveAt(i);
                        continue;
                    }
                    Gizmos.DrawLine(transform.position, curNode.transform.position);
                }
            }
        }
#endif

        public Node GetClosestNode(bool disallowOccupied = false)
        {
            Node closestNode = null;
            float closestDist = Mathf.Infinity;

            List<Node> neighborNodes = GetNeighbors();
            foreach (Node node in neighborNodes)
            {

                float dist = Vector3.Distance(node.transform.position, this.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNode = node;
                }
            }
            return closestNode;
        }

        public List<Node> GetNeighbors()
        {
            return Nodes;
        }

    }
}