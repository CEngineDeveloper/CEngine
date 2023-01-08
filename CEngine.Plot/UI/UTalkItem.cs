//**********************************************
// Class Name	: HelpItem
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.Plot;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UTalkItemData : UData
    {
        public UImageData Bg = new UImageData();
        public UTextData KeyTip = new UTextData();
        public UTextData SelectTip = new UTextData();
        public UButtonData Option = new UButtonData();

        public Func<int> CurSelectOptionIndex = () => 0;
    }
    public class UTalkItem : UPres<UTalkItemData>
    {
        #region inspector
        [SerializeField,FoldoutGroup("Option")]
        protected UImage Icon;
        [SerializeField, FoldoutGroup("Option")]
        protected UImage Bg;
        [SerializeField, FoldoutGroup("Option")]
        protected UImage Next;
        [SerializeField, FoldoutGroup("Option")]
        protected UText KeyTip;
        [SerializeField, FoldoutGroup("Option")]
        protected UText SelectTip;

        [SerializeField]
        protected UText Name;
        [SerializeField]
        protected UText Desc;
        [SerializeField]
        protected UDupplicate DP_Select;
        #endregion

        #region prop
        Tween Tween;
        TDBaseTalkData CurTalkData;
        AudioSource PreAudioSource;
        TalkFragment CurTalkFragment;
        protected LayoutElement TextLayoutElement;
        public bool IsTypeEnd { get; set; } = false;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            TextLayoutElement = Desc.gameObject.GetComponent<LayoutElement>();
            if(TextLayoutElement!=null)
                Desc.gameObject.AddComponent<LayoutElement>();
        }
        public override void Init(UTalkItemData data)
        {
            base.Init(data);
            Bg?.Init(data.Bg);
            KeyTip?.Init(data.KeyTip);
            SelectTip?.Init(data.SelectTip);
            DP_Select.Init(TDBaseTalkData.Val_MaxTalkOptionCount, data.Option,(p, d) =>
            {
                var pressenter = p as UButton;
                if (CurTalkData.Option.Count > pressenter.Index)
                {
                    pressenter.Show(true);
                    pressenter.NameText = BaseLangMgr.Get(CurTalkData.Option[pressenter.Index]);
                }
                else
                {
                    pressenter.Show(false);
                }

                if (data.CurSelectOptionIndex() == pressenter.Index)
                    pressenter.SetSelected(true);
                else
                    pressenter.SetSelected(false);
            }, null);
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        #region set
        public void Show(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            CurTalkData = talkData;
            CurTalkFragment = talkFragment;
            Show(true, true);

            if (Icon != null) Icon.IIcon.overrideSprite = GetIcon(talkData, talkFragment);
            if (Name != null) Name.NameText = GetName(talkData, talkFragment);
            if (PreAudioSource != null)
            {
                PreAudioSource.Stop();
            }
            PreAudioSource = PlayClip(GetAudio(talkData, talkFragment));

            Desc.NameText = "";
            Desc.IsAnimation = false;
            if (Tween != null)
                Tween.Kill();
            Tween = DOTween.To(() => 
                Desc.RichName.Content,  (x) => Desc.RichName.Content = x,talkFragment.GetDesc(), 0.5f)
                .SetDelay(0.5f)
                .OnComplete(OnTypeEnd)
                .OnStart(OnTweenStart);

            bool isHaveOpt = talkData.IsHaveOption() && talkFragment.IsLasted;
            DP_Select.Show(isHaveOpt);
            if (isHaveOpt)
            {
                TextLayoutElement.minHeight = 50.0f;
            }
            else
            {
                TextLayoutElement.minHeight = 100.0f;
            }
        }
        public override void Show(bool b = true, bool isForce = false)
        {
            IsTypeEnd = !b;
            base.Show(b, isForce);
        }
        #endregion

        #region must override
        protected virtual Sprite GetIcon(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.GetIcon();
        }
        protected virtual string GetName(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.GetName();
        }
        protected virtual string GetAudio(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.Audio;
        }
        #endregion

        #region Callback
        void OnTweenStart()
        {

        }
        void OnTypeEnd()
        {
            IsTypeEnd = true;
            Desc.RichName.RefreshRichText();
        }
        #endregion
    }
}
