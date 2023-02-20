using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace CYM.Diplomacy.NodeMap
{
    [ExecuteInEditMode]
    public class NodeMap : MonoSingleton<NodeMap>
    {

        public bool showPath = false;
        public float redrawThreshhold = 0.1f;
        public float nodeScale = 1f;

        //[SerializeField, SceneObjectsOnly]
        public List<Node> mapNodes = new List<Node>();

        void OnDrawGizmos()
        {
            nodeScale = Mathf.Clamp(nodeScale, 0, System.Single.MaxValue);

        }

        public void Refresh(bool isAdd = true)
        {
            mapNodes = transform.GetComponentsInChildren<Node>().ToList();
            for (int i = 0; i < mapNodes.Count; i++)
            {
                Node node = mapNodes[i];

                if (node == null)
                {
                    mapNodes.RemoveAt(i);
                    continue;
                }

                node.Nodes.Distinct();
                if (isAdd)
                {
                    foreach (var item in node.Nodes)
                    {
                        if (!item.Nodes.Contains(node))
                            item.Nodes.Add(node);
                    }
                }
                else
                {
                    List<Node> clears = new List<Node>();
                    foreach (var item in node.Nodes)
                    {
                        if (!item.Nodes.Contains(node))
                        {
                            clears.Add(item);
                        }
                    }
                    foreach (var item in clears)
                        node.Nodes.Remove(item);
                }
            }
        }

        private float HCost(Node curNode, Node goalNode)
        {
            return Vector3.Distance(curNode.transform.position, goalNode.transform.position);
        }

        private float GCost(Node curNode, Node neighbor)
        {
            return Vector3.Distance(curNode.transform.position, neighbor.transform.position);
        }

        public List<Node> FindRoute(Node startNode, Node goalNode)
        {
            PriorityQueue<float, Node> openNodes = new PriorityQueue<float, Node>();
            HashSet<Node> checkedNodes = new HashSet<Node>();

            Dictionary<Node, Node> pathTo = new Dictionary<Node, Node>();
            Dictionary<Node, float> gCost = new Dictionary<Node, float>();

            pathTo[startNode] = null;
            gCost[startNode] = 0f;

            openNodes.Push(0f + HCost(startNode, goalNode), startNode);

            while (openNodes.Count > 0)
            {
                Node leafNode = openNodes.Top.Value;
                if (leafNode == goalNode)
                {
                    // Success!
                    List<Node> route = new List<Node>();
                    Node pointer = goalNode;

                    while (pointer != null)
                    {
                        route.Add(pointer);
                        pointer = pathTo[pointer];
                    }

                    route.Reverse();    // Invert route so we can follow it from start to finish

                    return route;
                }
                openNodes.Pop();

                checkedNodes.Add(leafNode);

                List<Node> neighbors = leafNode.GetNeighbors();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Node neighbor = neighbors[i];
                    if (!checkedNodes.Contains(neighbor) && !openNodes.Contains(neighbor))
                    {
                        gCost[neighbor] = gCost[leafNode] + GCost(leafNode, neighbor);
                        pathTo[neighbor] = leafNode;
                        openNodes.Push(gCost[neighbor] + HCost(neighbor, goalNode), neighbor);
                    }
                }
            }

            // No route found
            return null;
        }
    }
}