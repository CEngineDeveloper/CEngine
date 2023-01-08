//------------------------------------------------------------------------------
// GuideView.cs
// Created by CYM on 2021/3/22
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.UI
{
    public class UGuideView :UStaticUIView<UGuideView>
    {
        #region inspector
        [SerializeField]
        UICircleFocusing UICircle;
        [SerializeField]
        UIRectFocusing UIRect;
        [SerializeField]
        UPointer UIPointer;
        [SerializeField]
        UPointer UIArrow;
        [SerializeField]
        UImage UIHilight;
        #endregion

        #region prop
        public bool IsMaskOnce { get; private set; }
        public UControl MaskedControl { get; private set; }
        public UControl PointedControl { get; private set; }
        public Callback CustomClick { get; private set; }
        #endregion

        #region life
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsShow)
            {
                if (UIPointer != null && UIPointer.IsShow)
                {
                    if (PointedControl != null)
                    {
                        var view = PointedControl.PUIView;
                        var panel = PointedControl.PPanel;
                        var container = PointedControl.PContainer;
                        if (container != null)
                        {
                            UIPointer.ShowDirect(container.IsShow);
                        }
                        else if (panel != null)
                        {
                            UIPointer.ShowDirect(panel.IsShow);
                        }
                        else if (view != null)
                        {
                            UIPointer.ShowDirect(view.IsShow);
                        }
                    }
                }
            }
        }
        #endregion

        #region is       
        public bool IsInMask
        {
            get {
                if (!IsShow)
                    return false;
                if (UICircle && UICircle.gameObject.activeSelf)
                    return true;
                if (UIRect && UIRect.gameObject.activeSelf)
                    return true;
                return false;
            }
        }
        #endregion

        #region pointer
        //只用于显示,不涉及点击,或者屏蔽按钮
        public void ShowArrow(UControl control, float rot = 0)
        {
            if (control == null) return;
            if (control.RectTrans == null) return;
            Show(true);
            UIHilight?.Show(false);
            UIPointer?.Show(false);
            UIArrow?.Show(true);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            if (UIArrow)
            {
                UIPointer.SetPosAndRot(RectTrans.InverseTransformPoint(control.RectTrans.position), new Vector3(0, 0, rot));
            }
        }
        public void ShowArrow3D(Vector3 pos, float rot = 0)
        {
            Show(true);
            UIHilight?.Show(false);
            UIPointer?.Show(false);
            UIArrow?.Show(true);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            if (UIArrow)
            {
                UIArrow.SetPosAndRot(RectTrans.InverseTransformPoint(Camera.main.WorldToScreenPoint(pos)), new Vector3(0, 0, rot));
            }
        }
        public void ShowPointer(UControl control,float rot=90, bool maskOnce = true, Callback callback = null)
        {
            if (control == null) return;
            if (control.RectTrans == null) return;
            Show(true);
            UIHilight?.Show(false);
            UIPointer?.Show(true);
            UIArrow?.Show(false);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            if (UIPointer)
            {
                UIPointer.SetPosAndRot(RectTrans.InverseTransformPoint(control.RectTrans.position),new Vector3(0,0, rot));
                IsMaskOnce = maskOnce;
                PointedControl = control;
                CustomClick = callback;
                if (maskOnce)
                    BaseGlobal.PlotMgr?.AddIgnoreBlockClickOnce(control);
            }
        }

        #endregion

        #region hilight
        //只用于显示,不涉及点击,或者屏蔽按钮
        public void ShowHilight(RectTransform rectTrans)
        {
            Show(true);
            UIHilight?.Show(true);
            UIPointer?.Show(false);
            UIArrow?.Show(false);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            if (UIHilight)
            {
                UIHilight.RectTrans.anchoredPosition = RectTrans.InverseTransformPoint(rectTrans.position);
                UIHilight.RectTrans.pivot = rectTrans.pivot;
                UIHilight.RectTrans.sizeDelta = rectTrans.sizeDelta;
            }
        }
        #endregion

        #region circle
        public void ShowCircle(UControl control, bool ignoreOnce = true)
        {
            ShowCircle(control, 1, ignoreOnce);
        }
        public void ShowCircle(UControl control, float scale, bool ignoreOnce = true)
        {
            if (control == null)
            {
                CLog.Error("错误！，ShowCircle(control) 不能为空");
                return;
            }
            RectTransform rectTrans = control.RectTrans;
            float maxVal = Mathf.Max(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y);
            ShowRawCircle(control, maxVal / 2 * scale, ignoreOnce);
        }
        public void ShowRawCircle(UControl control, float radius, bool ignoreOnce = true)
        {
            if (control == null) return;
            if (control.RectTrans == null) return;
            if (!control.IsActiveSelf) return;
            if (!control.PUIView.IsShow)
            {
                CLog.Error("错误！，控件的父节点UIView没有显示");
                return;
            }
            Show(true);
            UIHilight?.Show(false);
            UIPointer?.Show(false);
            UIArrow?.Show(false);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(true);
            UICircle?.SetCenter(control);
            UICircle?.SetRadius(radius);
            IsMaskOnce = ignoreOnce;
            MaskedControl = control;
            if (ignoreOnce)
                BaseGlobal.PlotMgr?.AddIgnoreBlockClickOnce(control);
        }
        #endregion

        #region rect
        public void ShowRect(UControl control, bool ignoreOnce = true)
        {
            ShowRect(control, 1, ignoreOnce);
        }
        public void ShowRect(UControl control, float scale, bool ignoreOnce = true)
        {
            if (control == null)
            {
                CLog.Error("错误！，ShowRect(control) 不能为空");
                return;
            }
            RectTransform rectTrans = control.RectTrans;
            ShowRawRect(control, rectTrans.sizeDelta.x / 2 * scale, rectTrans.sizeDelta.y / 2 * scale, ignoreOnce);
        }
        public void ShowRawRect(UControl control, float x, float y, bool ignoreOnce = true)
        {
            if (control == null) return;
            if (control.RectTrans == null) return;
            if (!control.IsActiveSelf) return;
            if (!control.PUIView.IsShow)
            {
                CLog.Error("错误！，控件的父节点UIView没有显示");
                return;
            }
            Show(true);
            UIHilight?.Show(false);
            UIPointer?.Show(false);
            UIArrow?.Show(false);
            UICircle?.gameObject.SetActive(false);
            UIRect?.gameObject.SetActive(true);
            UIRect?.SetCenter(control);
            UIRect?.SetRect(x, y);
            IsMaskOnce = ignoreOnce;
            MaskedControl = control;
            if (ignoreOnce)
                BaseGlobal.PlotMgr?.AddIgnoreBlockClickOnce(control);
        }
        #endregion

        #region close
        public void CloseMask()
        {
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            UIPointer?.Close();
            UIArrow?.Close();
            UIHilight?.Close();
            MaskedControl = null;
            PointedControl = null;
        }
        public void CloseWhenMaskOnce()
        {
            if (IsMaskOnce)
                CloseMask();
            CustomClick?.Invoke();
            CustomClick = null;
        }
        public void ClosePointer()
        {
            UIPointer?.Close();
        }
        public void CloseArrow()
        {
            UIArrow?.Close();
        }
        public void CloseHilight()
        {
            UIHilight?.Close();
        }
        #endregion

    }
}