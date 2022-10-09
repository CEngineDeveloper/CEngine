using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Wobbly : TextEffect
    {
        [SerializeField]
        private float m_Speed;
        [SerializeField]
        private float m_Density;
        [SerializeField]
        private float m_Magnitude;

        protected Wobbly()
        {
        }

        protected override void OnParameterRebuild()
        {
            var a = parameter.Split(',');
            m_Speed = ParseFloat(a.Length > 0 ? a[0] : string.Empty, 3);
            m_Density = ParseFloat(a.Length > 1 ? a[1] : string.Empty, 1);
            m_Magnitude = ParseFloat(a.Length > 2 ? a[2] : string.Empty, 2);
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            UIVertex vertex = UIVertex.simpleVert;
            while (startCharIdx <= endCharIdx)
            {
                for (int i = 0; i < 4; i++)
                {
                    var k = startCharIdx * 4 + i;
                    vh.PopulateUIVertex(ref vertex, k);
                    vertex.position = vertex.position + new Vector3(0, m_Magnitude * Mathf.Sin((Time.timeSinceLevelLoad * m_Speed) + (startCharIdx * m_Density)), 0);
                    vh.SetUIVertex(vertex, k);
                }
                ++startCharIdx;
            }
        }

        void Update()
        {
            if (richText != null)
            {
                richText.SetVerticesDirty();
            }
        }
    }
}
