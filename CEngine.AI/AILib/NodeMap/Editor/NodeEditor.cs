using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
namespace CYM.AI.NodesMap
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(Node))]
    [CanEditMultipleObjects]
    public class NodeEditor : OdinEditor
    {
        private int lastCount = 0;
        private Node targetNode;
        private Vector3 lastDrawnPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            targetNode = target as Node;
            lastCount = targetNode.Nodes.Count;
        }

        void OnSceneGUI()
        {
            Tools.hidden = true;
            targetNode.transform.position = Handles.PositionHandle(targetNode.transform.position, Quaternion.identity);

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.control)
                {
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                Node node = target as Node;
                if (NodeMap.Ins.redrawThreshhold != 0f && Vector3.Distance(node.transform.position, lastDrawnPosition) > NodeMap.Ins.redrawThreshhold)
                {
                    lastDrawnPosition = node.transform.position;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var dropObj = EditorUtil.DropOjbect("拖拽Node对象到这");
            if (dropObj != null && dropObj is GameObject go)
            {
                var node = go.GetComponent<Node>();
                if (node != null)
                {
                    if(!targetNode.Nodes.Contains(node))
                        targetNode.Nodes.Add(node);
                }
            }

            if (targetNode.Nodes.Count < lastCount)
            {
                NodeMap.Ins.Refresh(false);
                lastCount = targetNode.Nodes.Count;
            }
            else if (targetNode.Nodes.Count > lastCount)
            {
                NodeMap.Ins.Refresh(true);
                lastCount = targetNode.Nodes.Count;
            }
        }

        void OnDestroy()
        {
            Node delNode = target as Node;
            if (Application.isEditor && delNode == null && targetNode != null && NodeMap.Ins != null)
            {
                NodeMap.Ins.mapNodes.Remove(targetNode);
                NodeMap.Ins.Refresh();
            }
        }
    }

}