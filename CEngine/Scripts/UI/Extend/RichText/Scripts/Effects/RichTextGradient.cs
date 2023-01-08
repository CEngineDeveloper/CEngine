using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class RichTextGradient : TextEffect
    {
        [SerializeField]
        protected Color[] m_Colors;

        protected RichTextGradient()
        {
        }

        protected override void OnParameterRebuild()
        {
            m_Colors = ParseColors(parameter, 4);
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            if (m_Colors.Length < 2)
            {
                return;
            }

            Color topLeft = m_Colors[0];
            Color topRight = m_Colors[1];
            Color bottomRight = m_Colors.Length > 2 ? m_Colors[2] : topRight;
            Color bottomLeft = m_Colors.Length > 3 ? m_Colors[3] : topLeft;

            while (startCharIdx <= endCharIdx)
            {
                var k = startCharIdx * 4;
                SetUIVertexColor(vh, k++, topLeft);
                SetUIVertexColor(vh, k++, topRight);
                SetUIVertexColor(vh, k++, bottomRight);
                SetUIVertexColor(vh, k++, bottomLeft);
                ++startCharIdx;
            }
        }
    }
}
