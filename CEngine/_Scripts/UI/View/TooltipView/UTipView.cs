//------------------------------------------------------------------------------
// BaseErrorTipView.cs
// Copyright 2020 2020/7/1 
// Created by CYM on 2020/7/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTipView : UStaticUIView<UTipView>
    {
        [SerializeField]
        UText Desc;

        LayoutElement LayoutElement;
        CoroutineHandle Coroutine;
        float DurationTime = 2.0f;

        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            LayoutElement = GO.GetComponent<LayoutElement>();
            if (LayoutElement == null)
                LayoutElement = GO.AddComponent<LayoutElement>();
        }
        public UTipView SetWidth(float width)
        {
            LayoutElement.enabled = true;
            LayoutElement.preferredWidth = width;
            return this;
        }
        public UTipView SetTipTime(float time)
        {
            DurationTime = time;
            return this;
        }
        public UTipView SetTextAlign(TextAnchor alien)
        {
            Desc.IName.alignment = alien;
            return this;
        }
        public UTipView ShowStr(string str)
        {
            DurationTime = 2f;
            Show(true);
            LayoutElement.enabled = false;
            Desc.IName.alignment = TextAnchor.MiddleCenter;
            Desc.Refresh(str);
            startEnumerator();
            return this;
        }
        public UTipView ShowError(string key, params object[] ps)
        {
            ShowStr(UIUtil.Red(Util.GetStr(key, ps)));
            return this;
        }
        public UTipView Show(string key, params object[] ps)
        {
            ShowStr(Util.GetStr(key, ps));
            return this;
        }

        #region private
        void startEnumerator()
        {
            BaseGlobal.CommonCorouter.Kill(Coroutine);
            Coroutine = BaseGlobal.CommonCorouter.Run(_close());
        }
        IEnumerator<float> _close()
        {
            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitForSeconds(DurationTime);
            Close();
        }
        #endregion
    }
}