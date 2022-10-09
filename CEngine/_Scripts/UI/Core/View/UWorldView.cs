using DG.Tweening;
using UnityEngine;
namespace CYM.UI
{
    public class UWorldView : UView
    {
        #region life
        public override void Awake()
        {
            base.Awake();
        }
        public sealed override void Attach(ViewLevel viewLevel, UView beAttchedView)
        {
            base.Attach(viewLevel, beAttchedView);
            if (ParentView != null) Trans.SetParent(ParentView.Trans);
            Trans.localPosition = sourceLocalPos;
        }
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            if (IsShow == b && !force) return;
            float tempFade = 0.0f;
            tempFade = IsSameTime ? Duration : (b ? InTime : OutTime);
            IsShow = b;

            if (scaleTween != null)
                scaleTween.Kill();
            if (IsScale)
                scaleTween = Trans.DOScale(IsShow ? 1.0f : 0.001f, tempFade).SetEase(IsShow ? TweenScale.InEase : TweenScale.OutEase);
            else
                scaleTween = Trans.DOScale(IsShow ? 1.0f : 0.001f, 0).SetEase(TweenScale.InEase);
            if (IsShow)
            {
                OnOpen(this, useGroup);
                scaleTween.OnComplete(OnFadeIn);
                scaleTween.SetDelay(Delay);
            }
            else
            {
                OnClose(useGroup);
                scaleTween.OnComplete(OnFadeOut);
                scaleTween.SetDelay(Delay);
            }

            if (IsShow)
            {
                SetDirtyRefresh();
                //UI互斥,相同UI组只能有一个UI被打开
                if (useGroup && Group > 0)
                {
                    for (int i = 0; i < SubViews.Count; ++i)
                    {
                        if (SubViews[i] != this && SubViews[i].Group == Group && SubViews[i].ViewLevel == ViewLevel)
                            SubViews[i].Show(false, false);
                    }
                }
            }
            else
            {
                //关闭界面的时候自动刷新父级界面
                if (ParentView.IsShow && !ParentView.IsRootView)
                {
                    ParentView.SetDirtyRefresh();
                }
            }

            OnShow();
        }
        #endregion

        #region get
        public Camera GetCamera()
        {
            if (IsRootView)
                return Canvas.worldCamera;
            return RootView.Canvas.worldCamera;
        }
        #endregion

    }

}