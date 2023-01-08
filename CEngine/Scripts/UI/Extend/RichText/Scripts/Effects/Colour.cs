using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Colour : TextEffect
    {
        [SerializeField]
        private Color[] m_Colors;

        protected override void OnParameterRebuild()
        {
            m_Colors = ParseColors(parameter, 2);
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            if (m_Colors.Length == 0)
                return;

            Color color = m_Colors[0];

            if (m_Colors.Length > 1)
            {
                Color right = m_Colors[1];
                Color color1 = color, color2 = right;
                float total = endCharIdx - startCharIdx + 1f;
                for (int i = startCharIdx; i <= endCharIdx; i++)
                {
                    var k = i * 4;
                    color2 = Color.Lerp(color, right, (i - startCharIdx + 1) / total);
                    SetUIVertexColor(vh, k++, color1);
                    SetUIVertexColor(vh, k++, color2);
                    SetUIVertexColor(vh, k++, color2);
                    SetUIVertexColor(vh, k++, color1);
                    color1 = color2;
                }
            }
            else
            {
                while (startCharIdx <= endCharIdx)
                {
                    var k = startCharIdx * 4;
                    SetUIVertexColor(vh, k++, color);
                    SetUIVertexColor(vh, k++, color);
                    SetUIVertexColor(vh, k++, color);
                    SetUIVertexColor(vh, k++, color);
                    ++startCharIdx;
                }
            }
        }
    }
}
