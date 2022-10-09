using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Link : TextEffect, IPointerClickHandler, IEventSystemHandler
    {
        private List<Rect> m_AreaList = new List<Rect>();

        protected Link()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(richText.rectTransform, eventData.position, richText.canvas.worldCamera, out localPosition))
            {
                foreach (var rect in m_AreaList)
                {
                    if (rect.Contains(localPosition))
                    {
                        eventData.Use();
                        var onLink = richText.OnLink;
                        if (onLink != null)
                        {
                            onLink.Invoke(parameter);
                        }
                        break;
                    }
                }
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            m_AreaList.Clear();
            base.ModifyMesh(vh);
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 3);
            float xMin = vertex.position.x;
            vh.PopulateUIVertex(ref vertex, endCharIdx * 4 + 2);
            float xMax = vertex.position.x;

            var line = lines[lineIndex];
            var factor = 1f / richText.pixelsPerUnit;
            m_AreaList.Add(Rect.MinMaxRect(xMin, (line.topY - line.height) * factor, xMax, line.topY * factor));
        }
    }
}
