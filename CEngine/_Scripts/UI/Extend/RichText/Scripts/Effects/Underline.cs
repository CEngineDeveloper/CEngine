using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Underline : Line
    {
        protected Underline()
        {
        }

        protected override int priority
        {
            get
            {
                return base.priority;
            }

            set
            {
                base.priority = value + 10000;
            }
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            var line = lines[lineIndex];
            var yMin = (line.topY - line.height) / richText.pixelsPerUnit + 1;
            var yMax = yMin + 2;

            Draw(vh, startCharIdx, endCharIdx, yMin, yMax, chars);
        }
    }
}
