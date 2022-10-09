using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Strike : Line
    {
        protected Strike()
        {
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            var line = lines[lineIndex];
            var yMin = (line.topY - line.height * 0.6f) / richText.pixelsPerUnit;
            var yMax = yMin + 4;

            Draw(vh, startCharIdx, endCharIdx, yMin, yMax, chars);
        }
    }
}
