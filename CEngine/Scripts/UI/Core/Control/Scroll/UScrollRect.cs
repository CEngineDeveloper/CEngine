using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    [AddComponentMenu("UI/Control/UScrollRect")]
    [HideMonoScript]
    public class UScrollRect : UPres<UData>
    {
        #region Inspector
        [FoldoutGroup("Inspector"),SerializeField, ChildGameObjectsOnly,Required]
        public RectTransform IContent;
        [FoldoutGroup("Inspector"), SerializeField]
        public Scrollbar IScrollbar;
        [FoldoutGroup("Data"), SerializeField]
        public UScroll.ScrollDirectionType ScrollDirection = UScroll.ScrollDirectionType.Vertical;
        [FoldoutGroup("Data"), SerializeField]
        public ScrollRect.ScrollbarVisibility ScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        [FoldoutGroup("Data"), SerializeField]
        public ScrollRect.MovementType MovementType = ScrollRect.MovementType.Clamped;
        [FoldoutGroup("Data"), SerializeField]
        public bool IsResetPosition = true;
        #endregion

        #region prop
        ScrollRect ScrollRect;
        ContentSizeFitter ContentSizeFitter;
        RectTransform ScrollRectTrans;
        #endregion

        #region life
        public override bool IsCanBeControlFetch => false;
        public override bool IsCanBeViewFetch => true;
        public override bool NeedUpdate => true;
        public override bool IsAutoInit => true;
        protected override void FetchSubControls()
        {
            //do noting
        }
        protected override void Start()
        {
            base.Start();
            if (IContent == null)
                return;
            ScrollRect = GO.SafeAddComponet<ScrollRect>();
            ScrollRectTrans = ScrollRect.transform as RectTransform;
            ScrollRect.content = IContent;
            if (ScrollRect != null)
            {
                if (ScrollDirection == UScroll.ScrollDirectionType.Vertical)
                    ScrollRect.verticalScrollbar = IScrollbar;
                else if (ScrollDirection == UScroll.ScrollDirectionType.Horizontal)
                    ScrollRect.horizontalScrollbar = IScrollbar;


                ScrollRect.horizontalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.verticalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.movementType = MovementType;
                ScrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
                ScrollRect.inertia = true;
                ScrollRect.decelerationRate = 0.2f;
                ScrollRect.scrollSensitivity = 15;
            }
            if (IScrollbar != null)
            {
                IScrollbar.size = 0.0f;
                IScrollbar.onValueChanged.AddListener(_ScrollBar_OnValueChanged);
            }

            ContentSizeFitter = IContent.GetComponentInChildren<ContentSizeFitter>();
            if (ContentSizeFitter != null)
            {
                if (LayoutElement == null)
                    EnsureLayoutElement();
                if (ScrollDirection == UScroll.ScrollDirectionType.Vertical) 
                    LayoutElement.minHeight = ScrollRectTrans.sizeDelta.y + 100;
                else if (ScrollDirection == UScroll.ScrollDirectionType.Horizontal) 
                    LayoutElement.minWidth = ScrollRectTrans.sizeDelta.x + 100;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (IScrollbar) IScrollbar.size = 0.0f;
            if (IsResetPosition)
                ResetScrollBar(1.0f);
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            if (b)
            {
                if (IScrollbar) IScrollbar.size = 0.0f;
                ResetScrollBar(1.0f);
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IScrollbar != null && ScrollRect != null)
            {
                if (BaseInputMgr.IsScrollWheel) ScrollRect.velocity = Vector2.zero;
                IScrollbar.size = 0.0f;
            }
        }
        #endregion

        #region set
        public void ResetScrollBar(float val)
        {
            if (IScrollbar != null && ScrollRect != null)
            {
                IScrollbar.value = val;
            }
        }
        #endregion

        #region Callback

        private void _ScrollRect_OnValueChanged(Vector2 arg0)
        {
            if (IScrollbar != null && ScrollRect != null)
            {
                IScrollbar.size = 0.0f;
            }
        }

        private void _ScrollBar_OnValueChanged(float arg0)
        {
            if (IScrollbar != null && ScrollRect != null)
            {
                IScrollbar.size = 0.0f;
            }
        }
        #endregion

        #region Inspector
        [Button("AddMask")]
        void Inspector_AddMask()
        {
            var mask = gameObject.AddComponent<Mask>();
            var image = gameObject.AddComponent<Image>();
            image.sprite = UIUtil.GetUISprite();
        }
        [Button("AddPlaceholder")]
        void Inspector_AddPlaceholder()
        {
            if (IContent != null)
                return;
            IContent = new GameObject("Placeholder", typeof(RectTransform)).transform as RectTransform;
            IContent.transform.localScale = Vector3.one;
            IContent.SetParent(transform);
        }
        #endregion
    }

}