//------------------------------------------------------------------------------
// TextValItem.cs
// Copyright 2019 2019/6/14 
// Created by CYM on 2019/6/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTextValData : UTextData
    {
        public Func<string> Value;
        public Func<string> Adt;
    }
    [AddComponentMenu("UI/Control/UTextVal")]
    [HideMonoScript]
    public class UTextVal : UPres<UTextValData>
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField, Required,ChildGameObjectsOnly]
        public Text IName;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public Text IAdt;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public Text IValue;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public Image IIcon;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        public Image IBg;
        #endregion

        #region life
        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (IName != null)
            {
                if (Data.Name != null)
                    IName.text = Data.Name.Invoke();
                else if (!Data.NameKey.IsInv())
                {
                    if (Data.IsTrans)
                        IName.text = Data.NameKey.GetName();
                    else
                        IName.text = Data.NameKey;
                }

                //修改字体的颜色
                var tempColor = Data.GetColor();
                if (tempColor != null)
                {
                    IName.color = tempColor.Value;
                }
            }

            if (IValue != null)
            {
                if (Data.Value != null) 
                    IValue.text = Data.Value.Invoke();
            }

            if (IAdt != null)
            {
                if (Data.Adt != null) 
                    IAdt.text = Data.Adt.Invoke();
            }

            if (IIcon != null)
            {
                if (Data.Icon != null) 
                    IIcon.overrideSprite = Data.Icon.Invoke();
                else if (!Data.IconStr.IsInv()) 
                    IIcon.overrideSprite = Data.IconStr.GetIcon();
            }

            if (IBg != null)
            {
                IBg.overrideSprite = Data.GetIcon();
            }
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return IName.text; }
            set { IName.text = value; }
        }
        public string AdtText
        {
            get { return IAdt.text; }
            set { IAdt.text = value; }
        }
        public string ValueText
        {
            get { return IValue.text; }
            set { IValue.text = value; }
        }
        public Sprite IconSprite
        {
            get { return IIcon.sprite; }
            set { IIcon.sprite = value; }
        }
        public Sprite BgSprite
        {
            get { return IBg.sprite; }
            set { IBg.sprite = value; }
        }
        public Color IconColor
        {
            get { return IIcon.color; }
            set { IIcon.color = value; }
        }
        public Color BgColor
        {
            get { return IBg.color; }
            set { IBg.color = value; }
        }
        #endregion
    }
}