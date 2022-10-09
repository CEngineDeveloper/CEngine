using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class GreenColor : TextEffect
    {
        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            Color color = Color.green;

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
