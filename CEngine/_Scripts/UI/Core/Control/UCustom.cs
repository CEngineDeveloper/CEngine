//------------------------------------------------------------------------------
// UCustom.cs
// Copyright 2021 2021/4/28 
// Created by CYM on 2021/4/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using CYM.UI;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System;
namespace CYM
{
    public class UCustomData : UData
    {
        public Func<string> Name1 = null;
        public Func<Sprite> Icon1 = null;
        public Func<Sprite> Bg1 = null;

        public Func<string> Name2 = null;
        public Func<Sprite> Icon2 = null;
        public Func<Sprite> Bg2 = null;
    }
    [HideMonoScript,AddComponentMenu("UI/Control/UCustom")]
    public class UCustom : UPres<UCustomData>
    {
        #region 组建
        [FoldoutGroup("Inspector 1"), SerializeField, SceneObjectsOnly]
        public Text IName1;
        [FoldoutGroup("Inspector 1"), SerializeField, SceneObjectsOnly]
        public Image IIcon1;
        [FoldoutGroup("Inspector 1"), SerializeField, SceneObjectsOnly]
        public Image IBg1;

        [FoldoutGroup("Inspector 2"), SerializeField, SceneObjectsOnly]
        public Text IName2;
        [FoldoutGroup("Inspector 2"), SerializeField, SceneObjectsOnly]
        public Image IIcon2;
        [FoldoutGroup("Inspector 2"), SerializeField, SceneObjectsOnly]
        public Image IBg2;
        #endregion

        #region life
        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                if (IName1 != null && Data.Name1 != null)
                    IName1.text = Data.Name1.Invoke();
                if (IIcon1 != null && Data.Icon1 != null)
                    IIcon1.overrideSprite = Data.Icon1.Invoke();
                if (IBg1 != null && Data.Bg1 != null)
                    IBg1.overrideSprite = Data.Bg1.Invoke();

                if (IName2 != null && Data.Name2 != null)
                    IName2.text = Data.Name2.Invoke();
                if (IIcon2 != null && Data.Icon2 != null)
                    IIcon2.overrideSprite = Data.Icon2.Invoke();
                if (IBg2 != null && Data.Bg2 != null)
                    IBg2.overrideSprite = Data.Bg2.Invoke();
            }
        }
        #endregion

        #region wrap
        public string NameText1
        {
            get { return IName1.text; }
            set { IName1.text = value; }
        }
        public Sprite IconSprite1
        {
            get { return IIcon1.sprite; }
            set { IIcon1.sprite = value; }
        }
        public Sprite BgSprite1
        {
            get { return IBg1.sprite; }
            set { IBg1.sprite = value; }
        }
        public string NameText2
        {
            get { return IName2.text; }
            set { IName2.text = value; }
        }
        public Sprite IconSprite2
        {
            get { return IIcon2.sprite; }
            set { IIcon2.sprite = value; }
        }
        public Sprite BgSprite2
        {
            get { return IBg2.sprite; }
            set { IBg2.sprite = value; }
        }
        #endregion
    }
}