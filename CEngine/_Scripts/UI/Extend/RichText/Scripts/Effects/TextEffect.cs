using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RichText))]
    public class TextEffect : UIBehaviour, IComparable<TextEffect>
    {
        [SerializeField]
        private int m_BeginIndex;

        [SerializeField]
        private int m_EndIndex;

        [SerializeField]
        private string m_Parameter;

        [SerializeField]
        private int m_Priority;

        private float m_Alpha = 1f;
        private RichText m_RichText;
        private bool m_ParameterDirty;

        protected RichText richText
        {
            get
            {
                if (m_RichText == null)
                {
                    m_RichText = GetComponent<RichText>();
                }
                return m_RichText;
            }
        }

        protected virtual int beginIndex
        {
            get
            {
                return m_BeginIndex;
            }
            private set
            {
                m_BeginIndex = value;
            }
        }

        protected virtual int endIndex
        {
            get
            {
                return m_EndIndex;
            }
            private set
            {
                m_EndIndex = value;
            }
        }

        protected virtual string parameter
        {
            get
            {
                return m_Parameter;
            }
            private set
            {
                m_Parameter = value;
            }
        }

        protected virtual int priority
        {
            get
            {
                return m_Priority;
            }
            set
            {
                m_Priority = value;
            }
        }

        public void Set(int beginIndex, int endIndex, string parameter, int priority)
        {
            if (richText != null)
            {
                m_Alpha = richText.color.a;
                richText.SetVerticesDirty();
            }
            this.beginIndex = beginIndex;
            this.endIndex = endIndex;
            this.parameter = parameter ?? string.Empty;
            this.priority = priority;
            m_ParameterDirty = true;
        }

        protected virtual void OnParameterRebuild()
        {
        }

        public virtual void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var gen = richText.cachedTextGenerator;
            var charCount = gen.characterCount;
            if (charCount <= 1 || m_BeginIndex >= charCount - 1)
                return;

            if (m_ParameterDirty || richText.color.a != m_Alpha)
            {
                m_ParameterDirty = false;
                OnParameterRebuild();
            }

            var text = richText.text;
            var lines = gen.lines;
            var chars = gen.characters;
            var endCharIdx = chars.Count - 2;
            for (int i = lines.Count - 1; i >= 0 && endCharIdx >= m_BeginIndex; i--)
            {
                var line = lines[i];
                var startCharIdx = line.startCharIdx;
                if (startCharIdx <= m_EndIndex)
                {
                    endCharIdx = Math.Min(endCharIdx, m_EndIndex);
                    startCharIdx = Math.Max(startCharIdx, m_BeginIndex);
                    //if (skipWhitespace) {
                    while (startCharIdx <= endCharIdx)
                    { // skip begin whitespaces
                        var info = chars[startCharIdx];
                        if (info.charWidth > 0 && !char.IsWhiteSpace(text, startCharIdx))
                            break;
                        ++startCharIdx;
                    }
                    while (endCharIdx >= startCharIdx)
                    { // skip end whitespaces
                        var info = chars[endCharIdx];
                        if (info.charWidth > 0 && !char.IsWhiteSpace(text, endCharIdx))
                            break;
                        --endCharIdx;
                    }
                    //}
                    if (startCharIdx <= endCharIdx)
                    {
                        ProcessCharactersAtLine(vh, i, startCharIdx, endCharIdx, lines, chars);
                    }
                }
                endCharIdx = line.startCharIdx - 1;
            }
        }

        protected virtual void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
        }

        public int CompareTo(TextEffect other)
        {
            return priority.CompareTo(other.priority);
        }

        static private List<Color> s_TempColors = new List<Color>();
        protected Color[] ParseColors(string param, int maxCount)
        {
            s_TempColors.Clear();
            if (m_Alpha > 0)
            {
                Color color;
                var a = param.Split(',');
                for (int i = 0; i < a.Length; i++)
                {
                    var str = a[i];
                    if (!string.IsNullOrEmpty(str) && ColorUtility.TryParseHtmlString(str, out color))
                    {
                        color.a *= m_Alpha;
                        s_TempColors.Add(color);
                        if (s_TempColors.Count < maxCount)
                            continue;
                    }
                    break;
                }
            }
            return s_TempColors.ToArray();
        }
        static protected int ParseInt(string v, int defaultValue)
        {
            if (string.IsNullOrEmpty(v))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return Convert.ToInt32(v);
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }
        static protected float ParseFloat(string v, float defaultValue)
        {
            if (string.IsNullOrEmpty(v))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return Convert.ToSingle(v);
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        static private UIVertex s_TempUIVertex = UIVertex.simpleVert;

        static public void SetUIVertexColor(VertexHelper vh, int index, Color color)
        {
            vh.PopulateUIVertex(ref s_TempUIVertex, index);
            s_TempUIVertex.color = color;
            vh.SetUIVertex(s_TempUIVertex, index);
        }
        static public void SetUIVertexColorAlpha(VertexHelper vh, int index, byte alpha)
        {
            vh.PopulateUIVertex(ref s_TempUIVertex, index);
            s_TempUIVertex.color.a = alpha;
            vh.SetUIVertex(s_TempUIVertex, index);
        }
        static public void SetUIVertexPosition(VertexHelper vh, int index, Vector3 position)
        {
            vh.PopulateUIVertex(ref s_TempUIVertex, index);
            s_TempUIVertex.position = position;
            vh.SetUIVertex(s_TempUIVertex, index);
        }
    }
}
