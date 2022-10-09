using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class Emoji : TextEffect
    {
        static private Dictionary<Emoji, float> s_RebuildQueue;

        static Emoji()
        {
            s_RebuildQueue = new Dictionary<Emoji, float>();
            Canvas.willRenderCanvases += () =>
            {
                foreach (var entry in s_RebuildQueue)
                {
                    entry.Key.Rebuild(entry.Value);
                }
                s_RebuildQueue.Clear();
            };
        }

        [SerializeField]
        private Emoticon m_Icon;

        private string m_SpriteName = "";
        private int m_SpriteCurrentIndex;
        private float m_SpriteDeltaTime;
        private SpritesData SpritesData;

        protected Emoji()
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
                base.priority = value + 50000;
            }
        }

        protected override void OnParameterRebuild()
        {
            m_SpriteName = parameter;
            m_SpriteDeltaTime = 0f;
        }

        private void CreateEmoticon()
        {
            var go = new GameObject("emoticon");
            go.layer = gameObject.layer;
            //go.SetActive(false);
            var t = go.transform;
            t.SetParent(transform);
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;
            t.localPosition = SysConst.VEC_FarawayPos;
            m_Icon = go.AddComponent<Emoticon>();
            m_Icon.rectTransform.pivot = Vector2.zero;
            m_Icon.raycastTarget = false;
        }

        protected override void OnEnable()
        {
            if (m_Icon == null)
            {
                CreateEmoticon();
            }
            //m_Icon.gameObject.hideFlags = HideFlags.HideInHierarchy;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            s_RebuildQueue.Remove(this);
            if (m_Icon != null)
            {
                m_Icon.gameObject.SetActive(false);
            }
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            if (m_Icon != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(m_Icon);
                }
                else
                {
                    GameObject.DestroyImmediate(m_Icon);
                }
                m_Icon = null;
            }
            base.OnDestroy();
        }

        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            Vector3 min, max;
            //byte topLeft, topRight, bottomLeft, bottomRight;

            var k = startCharIdx * 4;
            UIVertex vertex = UIVertex.simpleVert;

            vh.PopulateUIVertex(ref vertex, k);
            //topLeft = vertex.color.a;
            vertex.uv0 = Vector2.zero;
            vh.SetUIVertex(vertex, k);

            vh.PopulateUIVertex(ref vertex, k + 1);
            //topRight = vertex.color.a;
            vertex.uv0 = Vector2.zero;
            vh.SetUIVertex(vertex, k + 1);

            vh.PopulateUIVertex(ref vertex, k + 2);
            //vertex.position = new Vector3(SizeFaction*richText.fontSize+vertex.position.x, vertex.position.y);
            max = vertex.position;
            //bottomRight = vertex.color.a;
            vertex.uv0 = Vector2.zero;
            vh.SetUIVertex(vertex, k + 2);

            vh.PopulateUIVertex(ref vertex, k + 3);
            //vertex.position = new Vector3(SizeFaction * richText.fontSize + vertex.position.x, vertex.position.y);
            min = vertex.position;
            //bottomLeft = vertex.color.a;
            vertex.uv0 = Vector2.zero;
            vh.SetUIVertex(vertex, k + 3);

            if (m_Icon != null)
            {
                m_Icon.rectTransform.localPosition = new Vector2(min.x, min.y - 2);
                m_Icon.SetColorAlphas(255, 255, 255, 255);
            }
            s_RebuildQueue[this] = (max.x - min.x);
        }

        private void Rebuild(float size)
        {
            if (m_Icon != null)
            {
                ShowSpriteName(m_SpriteName);
                m_Icon.rectTransform.sizeDelta = new Vector2(size, size);
                var go = m_Icon.gameObject;
                if (!go.activeSelf)
                {
                    m_Icon.gameObject.SetActive(true);
                }
            }
        }
        private void ShowSpriteName(string name)
        {
            if (UIConfig.Ins.SpriteGroupConfigs == null)
            {
                CLog.Error("RichText图集为空");
                return;
            }
            bool isFind = true;
            foreach (var group in UIConfig.Ins.SpriteGroupConfigs)
            {
                if (group != null)
                {
                    var sprites = group.KeySpritesData;
                    if (sprites.ContainsKey(name))
                    {
                        SpritesData = sprites[name];
                        m_Icon.sprite = SpritesData.First;
                    }
                    else
                    {
                        isFind = false;
                    }
                }
            }
            if (!isFind)
            {
                CLog.Error("没有这个图片:{0}", name);
            }
        }

        void Update()
        {
            if (SpritesData == null)
                return;
            if (!SpritesData.IsAnim)
                return;
            m_SpriteDeltaTime += Time.deltaTime;
            if (m_SpriteDeltaTime >= SpritesData.AnimSpeed)
            {
                m_SpriteDeltaTime = 0.0f;
                ++m_SpriteCurrentIndex;
                if (m_SpriteCurrentIndex >= SpritesData.Sprites.Length)
                    m_SpriteCurrentIndex = 0;
                if (!CanvasUpdateRegistry.IsRebuildingGraphics())
                {
                    m_Icon.sprite = SpritesData.Sprites[m_SpriteCurrentIndex];
                }
            }
        }
    }
}
