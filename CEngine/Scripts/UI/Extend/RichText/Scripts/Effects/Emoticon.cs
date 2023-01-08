using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class Emoticon : UnityEngine.UI.Image
    {
        [SerializeField]
        private byte m_TopLeft;
        [SerializeField]
        private byte m_TopRight;
        [SerializeField]
        private byte m_BottomLeft;
        [SerializeField]
        private byte m_BottomRight;

        Tweener tweener;
        protected override void Awake()
        {
            transform.localScale = SysConst.VEC_MiniScale;
            transform.localPosition = SysConst.VEC_FarawayPos;
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (tweener == null)
            {
                tweener = transform.DOScale(1.0f, 0.5f).SetEase(Ease.OutBack);
            }
        }
        protected override void OnDisable()
        {
            transform.localScale = SysConst.VEC_MiniScale;
            transform.localPosition = SysConst.VEC_FarawayPos;
            tweener.Kill();
            tweener = null;
            base.OnDisable();
        }
        protected override void Start()
        {
            base.Start();

        }

        public void SetColorAlphas(byte topLeft, byte topRight, byte bottomLeft, byte bottomRight)
        {
            m_TopLeft = topLeft;
            m_TopRight = topRight;
            m_BottomLeft = bottomLeft;
            m_BottomRight = bottomRight;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            Emoji.SetUIVertexColorAlpha(toFill, 0, m_BottomLeft);
            Emoji.SetUIVertexColorAlpha(toFill, 1, m_TopLeft);
            Emoji.SetUIVertexColorAlpha(toFill, 2, m_TopRight);
            Emoji.SetUIVertexColorAlpha(toFill, 3, m_BottomRight);
        }
    }
}