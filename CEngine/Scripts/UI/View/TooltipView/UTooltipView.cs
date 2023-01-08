//**********************************************
// Class Name	: BaseTooltipView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
//using JacobGames.SuperInvoke;
using Invoke;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTooltipView : UStaticUIView<UTooltipView>
    {
        // 当这个标志为true的时候,自动关闭,用于UI控件的OnExit触发
        static bool IsDirtyCloseTip { get; set; } = false;
        public static UTooltipView CurShow { get; protected set; }

        #region presenter
        [FoldoutGroup("Inspector"), SerializeField]
        protected UText Text;
        [FoldoutGroup("Data"), SerializeField, Tooltip("是否自动计算ContentSize")]
        protected bool AutoContentSize = true;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 TopOffset = Vector2.zero;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 BottomOffset = Vector2.zero * 20f;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 AnchoredOffset = Vector2.zero;
        [FoldoutGroup("Data"), SerializeField]
        Anchoring Anchoring = Anchoring.Corners;
        [FoldoutGroup("Data"), SerializeField]
        Graphic AnchorGraphic;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 AnchorGraphicOffset = Vector2.zero;
        [FoldoutGroup("Data"), SerializeField]
        float TipDelay = 0;
        [FoldoutGroup("Data"), SerializeField]
        float MaxWidth = 400;
        [FoldoutGroup("Data"), SerializeField]
        float MinWidth = 100;
        #endregion

        #region Input Prop
        string InputDesc = "No Set";
        float? InputWidth;
        Corner? InputCorner = null;
        Vector2? InputMousePos;
        Vector3? InputWorldPos;
        RectTransform InputAnchorTarget;
        #endregion

        #region prop
        Anchor CurrentAnchor = Anchor.BottomLeft;
        Corner CurrentCorner = Corner.TopLeft;
        //打开界面时候第一次记录的Corner,防止闪烁
        Corner? FirstCorner = null;
        ContentSizeFitter SizeFitter;
        float? Width = 0;
        private IJob closeInvoke;
        private IJob openInvoke;
        private bool isTipShow = false;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (Default == null)
                Default = this;
        }
        public override void Awake()
        {
            base.Awake();
            SizeFitter = GetComponent<ContentSizeFitter>();
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!Application.isMobilePlatform)
            {
                if (!IsCompleteClose)
                {
                    //有鼠标点击的时候自动关闭
                    if (Input.GetMouseButtonDown(0) ||
                        Input.GetMouseButtonDown(1) ||
                        Input.GetMouseButtonDown(2) ||
                        IsDirtyCloseTip)
                    {
                        Close();
                    }
                }
            }
            else
            {
                if (!IsCompleteClose)
                {
                    //有鼠标点击的时候自动关闭
                    if (BaseInputMgr.GetTouchUp(false) ||
                        IsDirtyCloseTip)
                    {
                        Close();
                    }
                }
            }
            if (IsShow && isTipShow)
            {
                UpdatePivot();
                UpdatePosition();
            }
            void UpdatePosition()
            {
                // Update the tooltip position to the mosue position
                // If the tooltip is not anchored to a target
                if (this.InputAnchorTarget == null)
                {
                    // Convert the offset based on the pivot
                    Vector2 pivotBasedOffset = Vector2.zero;
                    Vector2 localPoint;

                    if (this.Anchoring == Anchoring.Corners)
                    {
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RootView.Canvas.transform as RectTransform, GetShowPos(), UICamera(), out localPoint))
                        {
                            if (CurrentCorner == Corner.BottomLeft ||
                                 CurrentCorner == Corner.BottomRight)
                            {
                                pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.TopOffset.x * -1f) : this.TopOffset.x),
                                                                       ((this.RectTrans.pivot.y == 1f) ? (this.TopOffset.y * -1f) : this.TopOffset.y));
                            }
                            else if (CurrentCorner == Corner.TopLeft ||
                                     CurrentCorner == Corner.TopRight)
                            {

                                pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.BottomOffset.x * -1f) : this.BottomOffset.x),
                                                   ((this.RectTrans.pivot.y == 1f) ? (this.BottomOffset.y * -1f) : this.BottomOffset.y));
                            }
                            this.RectTrans.anchoredPosition = pivotBasedOffset + localPoint;
                        }
                    }
                    else if (this.Anchoring == Anchoring.LeftOrRight || this.Anchoring == Anchoring.TopOrBottom)
                    {

                    }
                    else if (this.Anchoring == Anchoring.Cant)
                    {
                        Vector2 curPos = Vector2.up;
                        if (FirstCorner != null)
                        {
                            if (FirstCorner == Corner.BottomLeft)
                                curPos = new Vector2(0, 0);
                            else if (FirstCorner == Corner.BottomRight)
                                curPos = new Vector2(Screen.width, 0);
                            else if (FirstCorner == Corner.TopLeft)
                                curPos = new Vector2(0, Screen.height);
                            else if (FirstCorner == Corner.TopRight)
                                curPos = new Vector2(Screen.width, Screen.height);
                            else
                                curPos = Vector2.zero;
                            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RootView.Canvas.transform as RectTransform, curPos, UICamera(), out localPoint))
                            {
                                this.RectTrans.anchoredPosition = pivotBasedOffset + localPoint;
                            }
                        }
                    }
                }
                // Check if we are anchored to a target
                else
                {
                    if (this.Anchoring == Anchoring.Corners)
                    {
                        // Set the anchor position to the opposite of the tooltip's pivot
                        Vector3[] targetWorldCorners = new Vector3[4];
                        this.InputAnchorTarget.GetWorldCorners(targetWorldCorners);

                        // Convert the tooltip pivot to corner
                        Corner pivotCorner = VectorPivotToCorner(this.RectTrans.pivot);

                        // Get the opposite corner of the pivot corner
                        Corner oppositeCorner = GetOppositeCorner(pivotCorner);

                        // Convert the offset based on the pivot
                        Vector2 pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.AnchoredOffset.x * -1f) : this.AnchoredOffset.x),
                                                               ((this.RectTrans.pivot.y == 1f) ? (this.AnchoredOffset.y * -1f) : this.AnchoredOffset.y));

                        // Get the anchoring point
                        Vector2 anchorPoint = this.RootView.Canvas.transform.InverseTransformPoint(targetWorldCorners[(int)oppositeCorner]);

                        // Apply anchored position
                        this.RectTrans.anchoredPosition = pivotBasedOffset + anchorPoint;
                    }
                    else if (this.Anchoring == Anchoring.LeftOrRight || this.Anchoring == Anchoring.TopOrBottom)
                    {
                        Vector3[] targetWorldCorners = new Vector3[4];
                        InputAnchorTarget.GetWorldCorners(targetWorldCorners);

                        Vector2 topleft = this.RootView.Canvas.transform.InverseTransformPoint(targetWorldCorners[1]);

                        if (this.Anchoring == Anchoring.LeftOrRight)
                        {
                            Vector2 pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.AnchoredOffset.x * -1f) : this.AnchoredOffset.x), this.AnchoredOffset.y);

                            if (this.RectTrans.pivot.x == 0f)
                            {
                                this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.InputAnchorTarget.rect.width, (this.InputAnchorTarget.rect.height / 2f) * -1f);
                            }
                            else
                            {
                                this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(0f, (this.InputAnchorTarget.rect.height / 2f) * -1f);
                            }
                        }
                        else if (this.Anchoring == Anchoring.TopOrBottom)
                        {
                            Vector2 pivotBasedOffset = new Vector2(this.AnchoredOffset.x, ((this.RectTrans.pivot.y == 1f) ? (this.AnchoredOffset.y * -1f) : this.AnchoredOffset.y));

                            if (this.RectTrans.pivot.y == 0f)
                            {
                                this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.InputAnchorTarget.rect.width / 2f, 0f);
                            }
                            else
                            {
                                this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.InputAnchorTarget.rect.width / 2f, this.InputAnchorTarget.rect.height * -1f);
                            }
                        }
                    }
                    else if (this.Anchoring == Anchoring.Cant)
                    {

                    }
                }
                // Fix position to nearest even number
                this.RectTrans.anchoredPosition = new Vector2(Mathf.Round(this.RectTrans.anchoredPosition.x), Mathf.Round(this.RectTrans.anchoredPosition.y));
                this.RectTrans.anchoredPosition = new Vector2(this.RectTrans.anchoredPosition.x + (this.RectTrans.anchoredPosition.x % 2f), this.RectTrans.anchoredPosition.y + (this.RectTrans.anchoredPosition.y % 2f));
            }
            void UpdatePivot()
            {
                // Get the mouse position
                Vector3 targetPosition = GetShowPos();
                if (Anchoring == Anchoring.None)
                {
                    Vector2 corner = new Vector2(0f, 1f);
                    // Set the pivot
                    SetPivotByCorner(corner);
                }
                else if (this.Anchoring == Anchoring.Corners)
                {
                    // Determine which corner of the screen is closest to the mouse position
                    Vector2 corner = new Vector2(
                        ((targetPosition.x > (Screen.width / 2f)) ? 1f : 0f),
                        ((targetPosition.y > (Screen.height / 2f)) ? 1f : 0f)
                    );

                    // Set the pivot
                    SetPivotByCorner(corner);
                }
                else if (this.Anchoring == Anchoring.LeftOrRight)
                {
                    // Determine the pivot
                    Vector2 pivot = new Vector2(((targetPosition.x > (Screen.width / 2f)) ? 1f : 0f), 0.5f);
                    // Set the pivot
                    SetPivotByPosition(pivot);
                }
                else if (this.Anchoring == Anchoring.TopOrBottom)
                {
                    // Determine the pivot
                    Vector2 pivot = new Vector2(0.5f, ((targetPosition.y > (Screen.height / 2f)) ? 1f : 0f));
                    // Set the pivot
                    SetPivotByPosition(pivot);
                }
                else if (this.Anchoring == Anchoring.Cant)
                {
                    // Determine which corner of the screen is closest to the mouse position
                    Vector2 pivot = new Vector2(
                        ((targetPosition.x > (Screen.width / 2f)) ? 0f : 1f),
                        ((targetPosition.y > (Screen.height / 2f)) ? 0f : 1f)
                    );
                    // Set the pivot
                    SetPivotByCorner(pivot);
                }
            }
            void UpdateAnchorGraphicPosition()
            {
                if (this.AnchorGraphic == null)
                    return;
                // Get the rect transform
                RectTransform rt = (this.AnchorGraphic.transform as RectTransform);
                if (this.Anchoring == Anchoring.None)
                {
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, (this.AnchorGraphicOffset.y * -1f) - rt.rect.height, rt.localPosition.z);
                    rt.localScale = new Vector3(1f, -1f, rt.localScale.z);
                }
                else if (this.Anchoring == Anchoring.Corners)
                {
                    // Pivot should always be bottom left
                    rt.pivot = Vector2.zero;

                    // Update it's anchor to the tooltip's pivot
                    rt.anchorMax = this.RectTrans.pivot;
                    rt.anchorMin = this.RectTrans.pivot;

                    // Update it's local position to the defined offset
                    rt.localPosition = new Vector3(((this.RectTrans.pivot.x == 1f) ? (this.AnchorGraphicOffset.x * -1f) : this.AnchorGraphicOffset.x),
                                                   ((this.RectTrans.pivot.y == 1f) ? (this.AnchorGraphicOffset.y * -1f) : this.AnchorGraphicOffset.y),
                                                   rt.localPosition.z);

                    // Flip the anchor graphic based on the pivot
                    rt.localScale = new Vector3(((this.RectTrans.pivot.x == 0f) ? 1f : -1f), ((this.RectTrans.pivot.y == 0f) ? 1f : -1f), rt.localScale.z);
                }
                else if (this.Anchoring == Anchoring.LeftOrRight || this.Anchoring == Anchoring.TopOrBottom)
                {
                    switch (this.CurrentAnchor)
                    {
                        case Anchor.Left:
                            rt.pivot = new Vector2(0f, 0.5f);
                            rt.anchorMax = new Vector2(0f, 0.5f);
                            rt.anchorMin = new Vector2(0f, 0.5f);
                            rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, this.AnchorGraphicOffset.y, rt.localPosition.z);
                            rt.localScale = new Vector3(1f, 1f, rt.localScale.z);
                            break;
                        case Anchor.Right:
                            rt.pivot = new Vector2(1f, 0.5f);
                            rt.anchorMax = new Vector2(1f, 0.5f);
                            rt.anchorMin = new Vector2(1f, 0.5f);
                            rt.localPosition = new Vector3((this.AnchorGraphicOffset.x * -1f) - rt.rect.width, this.AnchorGraphicOffset.y, rt.localPosition.z);
                            rt.localScale = new Vector3(-1f, 1f, rt.localScale.z);
                            break;
                        case Anchor.Bottom:
                            rt.pivot = new Vector2(0.5f, 0f);
                            rt.anchorMax = new Vector2(0.5f, 0f);
                            rt.anchorMin = new Vector2(0.5f, 0f);
                            rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, this.AnchorGraphicOffset.y, rt.localPosition.z);
                            rt.localScale = new Vector3(1f, 1f, rt.localScale.z);
                            break;
                        case Anchor.Top:
                            rt.pivot = new Vector2(0.5f, 1f);
                            rt.anchorMax = new Vector2(0.5f, 1f);
                            rt.anchorMin = new Vector2(0.5f, 1f);
                            rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, (this.AnchorGraphicOffset.y * -1f) - rt.rect.height, rt.localPosition.z);
                            rt.localScale = new Vector3(1f, -1f, rt.localScale.z);
                            break;
                    }
                }
                else if (this.Anchoring == Anchoring.Cant)
                {
                    // Pivot should always be bottom left
                    rt.pivot = Vector2.zero;

                    // Update it's anchor to the tooltip's pivot
                    rt.anchorMax = this.RectTrans.pivot;
                    rt.anchorMin = this.RectTrans.pivot;

                    // Update it's local position to the defined offset
                    rt.localPosition = new Vector3(((this.RectTrans.pivot.x == 1f) ? (this.AnchorGraphicOffset.x * -1f) : this.AnchorGraphicOffset.x),
                                                   ((this.RectTrans.pivot.y == 1f) ? (this.AnchorGraphicOffset.y * -1f) : this.AnchorGraphicOffset.y),
                                                   rt.localPosition.z);

                    // Flip the anchor graphic based on the pivot
                    rt.localScale = new Vector3(((this.RectTrans.pivot.x == 0f) ? 1f : -1f), ((this.RectTrans.pivot.y == 0f) ? 1f : -1f), rt.localScale.z);
                }
            }

            void SetPivotByPosition(Vector2 pivot)
            {
                Corner corner = VectorPivotToCorner(pivot);
                if (FirstCorner == null)
                {
                    FirstCorner = InputCorner ?? corner;
                }
                else if (Anchoring == Anchoring.Cant)
                {
                    return;
                }
                // Update the pivot
                this.RectTrans.pivot = pivot;
                // Update the current anchor value
                this.CurrentAnchor = VectorPivotToAnchor(pivot);
                // Update the anchor graphic position to the new pivot point
                UpdateAnchorGraphicPosition();
            }
            void SetPivotByCorner(Vector2 pivot)
            {
                Corner corner = VectorPivotToCorner(pivot);
                if (FirstCorner == null)
                {
                    FirstCorner = InputCorner ?? corner;
                }
                else if (Anchoring == Anchoring.Cant)
                {
                    return;
                }
                CurrentCorner = FirstCorner.Value;
                // Update the pivot
                switch (FirstCorner.Value)
                {
                    case Corner.BottomLeft:
                        this.RectTrans.pivot = new Vector2(0f, 0f);
                        break;
                    case Corner.BottomRight:
                        this.RectTrans.pivot = new Vector2(1f, 0f);
                        break;
                    case Corner.TopLeft:
                        this.RectTrans.pivot = new Vector2(0f, 1f);
                        break;
                    case Corner.TopRight:
                        this.RectTrans.pivot = new Vector2(1f, 1f);
                        break;
                }
                // Update the current anchor value
                this.CurrentAnchor = VectorPivotToAnchor(this.RectTrans.pivot);
                // Update the anchor graphic position to the new pivot point
                UpdateAnchorGraphicPosition();
            }

            Corner GetOppositeCorner(Corner corner)
            {
                switch (corner)
                {
                    case Corner.BottomLeft:
                        return Corner.TopRight;
                    case Corner.BottomRight:
                        return Corner.TopLeft;
                    case Corner.TopLeft:
                        return Corner.BottomRight;
                    case Corner.TopRight:
                        return Corner.BottomLeft;
                }

                // Default
                return Corner.BottomLeft;
            }
            Vector2 GetShowPos()
            {
                if (InputMousePos != null)
                    return InputMousePos.Value;
                else if (InputWorldPos != null)
                    return BaseGlobal.MainCamera.WorldToScreenPoint(InputWorldPos.Value);
                else
                {
                    return BaseInputMgr.ScreenPos;
                }

            }

            Corner VectorPivotToCorner(Vector2 pivot)
            {
                // Pivot to that corner
                if (pivot.x == 0f && pivot.y == 0f) return Corner.BottomLeft;
                else if (pivot.x == 0f && pivot.y == 1f) return Corner.TopLeft;
                else if (pivot.x == 1f && pivot.y == 0f) return Corner.BottomRight;
                // 1f, 1f
                return Corner.TopRight;
            }
            Anchor VectorPivotToAnchor(Vector2 pivot)
            {
                // Pivot to anchor
                if (pivot.x == 0f && pivot.y == 0f) return Anchor.BottomLeft;
                else if (pivot.x == 0f && pivot.y == 1f) return Anchor.TopLeft;
                else if (pivot.x == 1f && pivot.y == 0f) return Anchor.BottomRight;
                else if (pivot.x == 0.5f && pivot.y == 0f) return Anchor.Bottom;
                else if (pivot.x == 0.5f && pivot.y == 1f) return Anchor.Top;
                else if (pivot.x == 0f && pivot.y == 0.5f) return Anchor.Left;
                else if (pivot.x == 1f && pivot.y == 0.5f) return Anchor.Right;
                // 1f, 1f
                return Anchor.TopRight;
            }
            Camera UICamera()
            {
                if (this.RootView.Canvas == null) return null;
                if (this.RootView.Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (this.RootView.Canvas.renderMode == RenderMode.ScreenSpaceCamera && this.RootView.Canvas.worldCamera == null))
                    return null;
                return (!(this.RootView.Canvas.worldCamera != null)) ? BaseGlobal.MainCamera : this.RootView.Canvas.worldCamera;
            }
        }
        #endregion

        #region set
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            FirstCorner = null;
            InputWidth = null;
            InputCorner = null;
            InputMousePos = null;
            InputWorldPos = null;
            InputAnchorTarget = null;
            if (b)
            {
                openInvoke?.Kill();
                closeInvoke?.Kill();
                openInvoke = Util.Invoke(() =>
                {
                    //所有的Tooltip均为互斥
                    if (CurShow != null && CurShow != this)
                        CurShow.Show(false, true, true);
                    CurShow = this;
                    RectTrans.SetAsLastSibling();
                    base.Show(b, useGroup, force);
                    RefreshLayout(InputWidth);
                }, Mathf.Clamp(TipDelay, 0.01f, float.MaxValue));
            }
            else
            {
                openInvoke?.Kill();
                closeInvoke?.Kill();
                if (force)
                {
                    base.Show(b, useGroup, force);
                }
                else
                {
                    closeInvoke = Util.Invoke(() => base.Show(b, useGroup, force), 0.01f);
                }
            }
            isTipShow = b;

            void RefreshLayout(float? width)
            {
                if (SizeFitter && AutoContentSize && Text)
                {
                    Text.NameText = InputDesc;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
                    SizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    SizeFitter.SetLayoutHorizontal();

                    if (width != null)
                    {
                        RectTrans.sizeDelta = new Vector2(Width.Value, RectTrans.sizeDelta.y);
                        Width = width;
                    }
                    else
                    {
                        if (RectTrans.sizeDelta.x > MaxWidth)
                        {
                            SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                            RectTrans.sizeDelta = new Vector2(MaxWidth, RectTrans.sizeDelta.y);
                            Width = MaxWidth;
                        }
                        else if (RectTrans.sizeDelta.x < MinWidth)
                        {
                            SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                            RectTrans.sizeDelta = new Vector2(MinWidth, RectTrans.sizeDelta.y);
                            Width = MinWidth;
                        }
                        else
                        {
                            SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                            RectTrans.sizeDelta = new Vector2(RectTrans.sizeDelta.x, RectTrans.sizeDelta.y);
                        }
                    }
                    SizeFitter.SetLayoutHorizontal();
                    SizeFitter.SetLayoutVertical();
                }
            }
        }
        public UTooltipView ShowStr(string str) => Show(str);
        public UTooltipView Show(string key, params object[] ps) => Show(GetStr(key, ps));
        public UTooltipView Show(string str)
        {
            if (str.IsInv()) return this;
            Text.NameText = InputDesc = str;
            IsDirtyCloseTip = false;
            Show(true, true, false);
            return this;
        }
        #endregion

        #region Show tip
        public UTooltipView ShowAuto(string key, string second)=> Show("Tip_二级", GetStr(key), second);
        public UTooltipView ShowAuto(string key, string second, string third)=> Show("Tip_三级", GetStr(key), second, third);
        public UTooltipView ShowAuto(string key, string second, string third, string fourth)=> Show("Tip_四级", GetStr(key), second, third, fourth);
        public UTooltipView ShowAuto(string key, string second, string third, string fourth, string fifth)=> Show("Tip_五级", GetStr(key), second, third, fourth, fifth);
        public UTooltipView ShowAuto(string key, string second, string third, string fourth, string fifth, string sixth)=> Show("Tip_六级", GetStr(key), second, third, fourth, fifth, sixth);

        public UTooltipView ShowAutoStr(string first, string second) => Show("Tip_二级", first, second);
        public UTooltipView ShowAutoStr(string first, string second, string third) => Show("Tip_三级", first, second, third);
        public UTooltipView ShowAutoStr(string first, string second, string third, string fourth) => Show("Tip_四级", first, second, third, fourth);
        public UTooltipView ShowAutoStr(string first, string second, string third, string fourth, string fifth) => Show("Tip_五级", first, second, third, fourth, fifth);
        public UTooltipView ShowAutoStr(string first, string second, string third, string fourth, string fifth, string sixth) => Show("Tip_六级", first, second, third, fourth, fifth, sixth);
        #endregion

        #region Set Input Val
        public UTooltipView SetInputInputAnchorTarget(RectTransform anchorToTarget)
        {
            InputAnchorTarget = anchorToTarget;
            return this;
        }
        public UTooltipView SetInputMousePos(Vector2 mousePos)
        {
            InputMousePos = mousePos;
            return this;
        }
        public UTooltipView SetInputWorldPos(Vector3 worldPos)
        {
            InputWorldPos = worldPos;
            return this;
        }
        public UTooltipView SetInputWidth(float width)
        {
            InputWidth = width;
            return this;
        }
        public UTooltipView SetInputCorner(Corner corner)
        {
            InputCorner = corner;
            return this;
        }
        #endregion

        #region Inspector
        protected override bool Inspector_HideGroup()=>true;
        protected override bool Inspector_HideIsExclusive()=>true;
        #endregion
    }
}