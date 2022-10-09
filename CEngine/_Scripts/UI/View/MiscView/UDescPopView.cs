//------------------------------------------------------------------------------
// UScrollTipView.cs
// Copyright 2021 2021/10/14 
// Created by CYM on 2021/10/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UDescPopView : UStaticUIView<UDescPopView>
    {
        #region Inspector
        [SerializeField,FoldoutGroup("Inspector")]  
        UText Desc1;
        [SerializeField, FoldoutGroup("Inspector")]
        UText Desc2;
        [SerializeField, FoldoutGroup("Inspector")]
        UText Desc3;
        [SerializeField, FoldoutGroup("Inspector")]
        UText Desc4;
        #endregion

        #region life
        public UDescPopView ShowStr(string str,float? width=null)
        {
            base.Show(true);
            Desc2.SetActive(false);
            Desc3.SetActive(false);
            Desc4.SetActive(false);
            setExtra(Desc1, str, width);
            return this;
        }
        public UDescPopView SetExtraStr1(string str, float? width = null) => setExtra(Desc2, str, width);
        public UDescPopView SetExtraStr2(string str, float? width = null) => setExtra(Desc3, str, width);
        public UDescPopView SetExtraStr3(string str, float? width = null) => setExtra(Desc4, str, width);

        private UDescPopView setExtra(UText desc,string str, float? width = null)
        {
            desc.SetActive(true);
            desc.Refresh(str);
            SetDirtyLayout(desc.RectTrans);
            SetDirtyLayout(RectTrans);
            var layoutElement = desc.IName.gameObject.GetComponent<LayoutElement>();
            if(layoutElement==null)
                layoutElement = desc.IName.gameObject.AddComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.enabled = width != null;
                if(width.HasValue)
                    layoutElement.preferredWidth = width.Value;
            }
            return this;
        }
        #endregion
    }
}