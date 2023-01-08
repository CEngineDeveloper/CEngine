using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Line : TextEffect
    {
        [SerializeField]
        protected Color[] m_Colors;

        protected Line()
        {
        }

        protected override void OnParameterRebuild()
        {
            m_Colors = ParseColors(parameter, 2);
        }

        private static CharacterInfo GetUnderlineCharacter(Font font)
        {
            CharacterInfo underline;
            if (!font.GetCharacterInfo('_', out underline, 64))
            {
                font.RequestCharactersInTexture("_", 64);
                font.GetCharacterInfo('_', out underline, 64);
            }
            var offset = new Vector2(0, (underline.uvBottomRight.y - underline.uvBottomLeft.y) * 0.45f);
            underline.uvTopLeft += offset;
            underline.uvTopRight -= offset;
            underline.uvBottomRight -= offset;
            underline.uvBottomLeft += offset;
            return underline;
        }

        protected void Draw(VertexHelper vh, int startCharIdx, int endCharIdx, float yMin, float yMax, IList<UICharInfo> chars)
        {
            CharacterInfo underline = GetUnderlineCharacter(richText.font);

            var text = richText.text;
            float xMin = 0, xMax = 0;
            UIVertex vertex = UIVertex.simpleVert;
            if (m_Colors.Length > 0)
            {
                vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 3);
                xMin = vertex.position.x;
                vh.PopulateUIVertex(ref vertex, endCharIdx * 4 + 2);
                xMax = vertex.position.x;
                AddUIVertexQuad(vh, ref xMin, xMax, yMin, yMax, m_Colors[0], m_Colors.Length > 1 ? m_Colors[1] : m_Colors[0], underline);
            }
            else
            {
                vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 3);
                xMin = vertex.position.x;

                Color leftColor = Color.white;
                Color rightColor = Color.white;
                while (startCharIdx <= endCharIdx)
                {
                    var info = chars[startCharIdx];
                    if (info.charWidth > 0)
                    {
                        if (char.IsWhiteSpace(text, startCharIdx))
                        {
                            while (true)
                            {
                                ++startCharIdx;
                                if (chars[startCharIdx].charWidth > 0 && !char.IsWhiteSpace(text, startCharIdx))
                                    break;
                            }
                            vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 3);
                            AddUIVertexQuad(vh, ref xMin, vertex.position.x, yMin, yMax, rightColor, rightColor, underline);
                        }
                        else
                        {
                            vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 3);
                        }
                        leftColor = vertex.color;
                        vh.PopulateUIVertex(ref vertex, startCharIdx * 4 + 2);
                        rightColor = vertex.color;
                        AddUIVertexQuad(vh, ref xMin, vertex.position.x, yMin, yMax, leftColor, rightColor, underline);
                    }
                    ++startCharIdx;
                }
            }
        }

        static private UIVertex[] s_TempUIVertexs = new UIVertex[4];

        private void AddUIVertexQuad(VertexHelper vh, ref float xMin, float xMax, float yMin, float yMax, Color left, Color right, CharacterInfo underline)
        {
            s_TempUIVertexs[0] = new UIVertex() { position = new Vector3(xMin, yMin), color = left, uv0 = underline.uvTopLeft };
            s_TempUIVertexs[1] = new UIVertex() { position = new Vector3(xMax, yMin), color = right, uv0 = underline.uvTopRight };
            s_TempUIVertexs[2] = new UIVertex() { position = new Vector3(xMax, yMax), color = right, uv0 = underline.uvBottomRight };
            s_TempUIVertexs[3] = new UIVertex() { position = new Vector3(xMin, yMax), color = left, uv0 = underline.uvBottomLeft };
            vh.AddUIVertexQuad(s_TempUIVertexs);
            xMin = xMax;
        }
    }
}
