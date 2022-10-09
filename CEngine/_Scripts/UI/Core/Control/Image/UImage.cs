using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UImageData : UData
    {
        public Func<Color> Color;
        public Func<Sprite> Icon;
        public string IconStr;

        public Func<Sprite> Frame;
        public string FrameStr;
    }
    [AddComponentMenu("UI/Control/UImage")]
    [HideMonoScript]

    //[RequireComponent(typeof(Image))]
    public class UImage : UPres<UImageData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), Required, SceneObjectsOnly]
        public Image IIcon;
        [FoldoutGroup("Inspector"), SceneObjectsOnly]
        public Image IFrame;
        #endregion

        #region life
        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                if (IIcon)
                {
                    if (Data.Color != null)
                    {
                        IIcon.color = Data.Color.Invoke();
                    }

                    Sprite sprite = null;
                    if (Data.Icon != null) sprite = Data.Icon.Invoke();
                    else if (!Data.IconStr.IsInv()) sprite = Data.IconStr.GetIcon();
                    IIcon.overrideSprite = sprite;
                }
                if (IFrame)
                {
                    Sprite sprite = null;
                    if (Data.Frame != null) sprite = Data.Frame.Invoke();
                    else if (!Data.FrameStr.IsInv()) sprite = Data.FrameStr.GetIcon();
                    IFrame.overrideSprite = sprite;
                }
            }
        }
        public void Refresh(string icon)
        {
            if (!icon.IsInv() && IIcon!=null)
                IIcon.overrideSprite = icon.GetIcon();
        }
        public void Refresh(Sprite icon)
        {
            if(IIcon) IIcon.overrideSprite = icon;
        }
        public void Refresh(string icon,string frame)
        {
            if (IIcon != null)
            {
                IIcon.overrideSprite = icon.GetIcon();
            }
            if (IFrame != null)
            {
                IFrame.overrideSprite = frame.GetIcon();
            }
        }
        #endregion

        #region wrap
        public bool IsGrey
        {
            set
            {
                if (value)
                    IIcon.material = BaseGlobal.GRMgr.ImageGrey;
                else
                    IIcon.material = null;
            }
        }
        public Sprite IconSprite
        {
            get { return IIcon.sprite; }
            set { IIcon.sprite = value; }
        }
        public Sprite FrameSprite
        {
            get { return IFrame.sprite; }
            set { IFrame.sprite = value; }
        }
        public Sprite IconOverSprite
        {
            get { return IIcon.overrideSprite; }
            set { IIcon.overrideSprite = value; }
        }
        public Sprite FrameOverSprite
        {
            get { return IFrame.overrideSprite; }
            set { IFrame.overrideSprite = value; }
        }
        #endregion

        #region inspector
        public override void AutoSetup()
        {
            base.AutoSetup();
            IIcon = GetComponent<Image>();
        }
        #endregion
    }
}
