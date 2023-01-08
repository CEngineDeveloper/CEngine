//------------------------------------------------------------------------------
// UTextScroll.cs
// Created by CYM on 2021/12/2
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
namespace CYM.UI
{
    [AddComponentMenu("UI/Control/UTextScroll")]
    [HideMonoScript]
    public class UTextScroll : UText
    {
        #region Inspector
        [SerializeField,Required,ChildGameObjectsOnly]
        ScrollRect IScrollRect;
        #endregion

        #region life
        protected override void Start()
        {
            base.Start();
            resetPosition();
        }
        public override void Refresh()
        {
            base.Refresh();
            resetPosition();
        }
        #endregion

        #region private
        void resetPosition()
        {
            if (IScrollRect != null)
            {
                //IName.resizeTextForBestFit = false;
                //IScrollRect.horizontalNormalizedPosition = 1;
                //IScrollRect.verticalNormalizedPosition = 1;
                if (IScrollRect.horizontalScrollbar)
                {
                    IScrollRect.horizontalScrollbar.size = 0;
                    IScrollRect.horizontalScrollbar.value = 1;
                }
                if (IScrollRect.verticalScrollbar)
                {
                    IScrollRect.verticalScrollbar.size = 0;
                    IScrollRect.verticalScrollbar.value = 1;
                }
            }
        }
        #endregion
    }
}