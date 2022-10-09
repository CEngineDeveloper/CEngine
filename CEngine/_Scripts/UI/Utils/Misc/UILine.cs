//------------------------------------------------------------------------------
// UILine.cs
// Created by CYM on 2021/12/20
// 填写类的描述...
//------------------------------------------------------------------------------\
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CYM.Pool;

namespace CYM.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILine : MaskableGraphic
    {
        #region enum
        private enum SegmentType
        {
            Start,
            Middle,
            End,
        }
        public enum JoinType
        {
            Bevel,
            Miter
        }
        #endregion

        #region static
        private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;
        private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;
        private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0, 1);
        private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
        private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
        private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1, 0);
        private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1, 1);
        private static readonly Vector2[] startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };
        private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
        private static readonly Vector2[] endUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
        private static SimpleObjPool<UIVertex[]> VertexArrayPool = new SimpleObjPool<UIVertex[]>(10, null, CreateVertexArray);
        private static UIVertex[] CreateVertexArray() => new UIVertex[4];
        #endregion

        #region prop
        List<Vector2> _lastPoints;
        List<Vector2> _tempVertices;
        List<UIVertex[]> _tempSegments;
        [Sirenix.OdinInspector.ShowInInspector]
        List<Vector2> _points = new List<Vector2>();
        #endregion

        #region Inspector
        public float LineThickness = 1;
        public Vector2 Margin = new Vector2();
        public bool RelativeSize=true;
        public bool LineList = false;
        public bool LineCaps = false;
        public JoinType LineJoins = JoinType.Bevel;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            _lastPoints = new List<Vector2>(_points);

        }
        #endregion


        #region set
        public void ClearPoints(int? fixedCount=null)
        {
            _points.Clear();
            if (fixedCount != null)
            {
                for (int i = 0; i < fixedCount.Value;++i)
                {
                    _points.Add(new Vector2());
                }
            }
        }
        public void SetPoint(int index,Vector2 pt)
        {
            _points[index] = pt;
        }
        public void AddPoints(Vector2 pt)
        {
            _points.Add(pt);
        }
        public void SetDirtyIfPointsChanged()
        {
            bool isDirty = false;
            float threshHold = 1e-6f;
            if (_lastPoints.Count != _points.Count)
            {
                isDirty = true;
            }
            else if (_lastPoints.Count == _points.Count)
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    Vector2 delta = _points[i] - _lastPoints[i];
                    if (delta.sqrMagnitude > threshHold)
                    {
                        isDirty = true;
                        break;
                    }
                }
            }
            if (isDirty)
            {
                SetVerticesDirty();
                _lastPoints.Clear();
                _lastPoints.AddRange(_points);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_points == null)
                return;
            var sizeX = rectTransform.rect.width;
            var sizeY = rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

            // don't want to scale based on the size of the rect, so this is switchable now
            if (!RelativeSize)
            {
                sizeX = 1;
                sizeY = 1;
            }

            sizeX -= Margin.x;
            sizeY -= Margin.y;
            offsetX += Margin.x / 2f;
            offsetY += Margin.y / 2f;

            vh.Clear();
            if (_tempSegments == null)
            {
                _tempSegments = new List<UIVertex[]>();
            }
            _tempSegments.Clear();
            if (LineList)
            {
                for (var i = 1; i < _points.Count; i += 2)
                {
                    var start = _points[i - 1];
                    var end = _points[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (LineCaps)
                    {
                        _tempSegments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    _tempSegments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (LineCaps)
                    {
                        _tempSegments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }
            else
            {
                for (var i = 1; i < _points.Count; i++)
                {
                    var start = _points[i - 1];
                    var end = _points[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (LineCaps && i == 1)
                    {
                        _tempSegments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    _tempSegments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (LineCaps && i == _points.Count - 1)
                    {
                        _tempSegments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }

            // Add the line segments to the vertex helper, creating any joins as needed
            for (var i = 0; i < _tempSegments.Count; i++)
            {
                if (!LineList && i < _tempSegments.Count - 1)
                {
                    var vec1 = _tempSegments[i][1].position - _tempSegments[i][2].position;
                    var vec2 = _tempSegments[i + 1][2].position - _tempSegments[i + 1][1].position;
                    var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                    // Positive sign means the line is turning in a 'clockwise' direction
                    var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                    // Calculate the miter point
                    var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
                    var miterPointA = _tempSegments[i][2].position - vec1.normalized * miterDistance * sign;
                    var miterPointB = _tempSegments[i][3].position + vec1.normalized * miterDistance * sign;

                    var joinType = LineJoins;
                    if (joinType == JoinType.Miter)
                    {
                        // Make sure we can make a miter join without too many artifacts.
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                        {
                            _tempSegments[i][2].position = miterPointA;
                            _tempSegments[i][3].position = miterPointB;
                            _tempSegments[i + 1][0].position = miterPointB;
                            _tempSegments[i + 1][1].position = miterPointA;
                        }
                        else
                        {
                            joinType = JoinType.Bevel;
                        }
                    }

                    if (joinType == JoinType.Bevel)
                    {
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                        {
                            if (sign < 0)
                            {
                                _tempSegments[i][2].position = miterPointA;
                                _tempSegments[i + 1][1].position = miterPointA;
                            }
                            else
                            {
                                _tempSegments[i][3].position = miterPointB;
                                _tempSegments[i + 1][0].position = miterPointB;
                            }
                        }

                        var join = new UIVertex[] { _tempSegments[i][2], _tempSegments[i][3], _tempSegments[i + 1][0], _tempSegments[i + 1][1] };
                        vh.AddUIVertexQuad(join);
                    }
                }
                vh.AddUIVertexQuad(_tempSegments[i]);
            }
            // 把array们回收掉
            for (int i = 0; i < _tempSegments.Count; i++)
            {
                VertexArrayPool.Recycle(_tempSegments[i]);
            }
            _tempSegments.Clear();
        }
        private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
        {
            if (type == SegmentType.Start)
            {
                var capStart = start - ((end - start).normalized * LineThickness / 2);
                return CreateLineSegment(capStart, start, SegmentType.Start);
            }
            else if (type == SegmentType.End)
            {
                var capEnd = end + ((end - start).normalized * LineThickness / 2);
                return CreateLineSegment(end, capEnd, SegmentType.End);
            }

            Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
            return null;
        }
        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type)
        {
            var uvs = middleUvs;
            if (type == SegmentType.Start)
                uvs = startUvs;
            else if (type == SegmentType.End)
                uvs = endUvs;

            Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2;
            var v1 = start - offset;
            var v2 = start + offset;
            var v3 = end + offset;
            var v4 = end - offset;
            if (_tempVertices == null)
            {
                _tempVertices = new List<Vector2>();
            }
            _tempVertices.Clear();
            _tempVertices.Add(v1);
            _tempVertices.Add(v2);
            _tempVertices.Add(v3);
            _tempVertices.Add(v4);

            return CreateVbo(_tempVertices, uvs);
        }
        protected UIVertex[] CreateVbo(List<Vector2> vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = VertexArrayPool.Get();
            for (int i = 0; i < vertices.Count; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }
        #endregion
    }
}