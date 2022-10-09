using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UCheckData : UButtonData
    {
        public Callback<UControl, PointerEventData,bool> OnCheckClick;

        //基于IconStr,自动关联_N和_On图标
        public bool IsAutoIcon = false;
        // 连接的Presenter
        public UControl LinkControl = null;
        // 连接的View
        public UUIView LinkView = null;
        public Func<bool> IsOn = null;
        public Callback<bool> OnValueChanged = null;

        public Func<Sprite> ActiveBg = null;
        public string ActiveBgStr = SysConst.STR_Inv;

        public Func<Sprite> ActiveIcon = null;
        public string ActiveIconStr = SysConst.STR_Inv;

        public Color? ActiveTextCol = null;
        public Color? ActiveIconCol = null;
        public Color? ActiveBgCol = null;

        #region get
        public Sprite GetActiveIcon()
        {
            if (ActiveIcon != null)
            {
                return ActiveIcon.Invoke();
            }
            if (!ActiveIconStr.IsInv())
                return BaseGlobal.RsIcon.Get(ActiveIconStr);
            return null;
        }
        public Sprite GetActiveBg()
        {
            if (ActiveBg != null)
            {
                return ActiveBg.Invoke();
            }
            if (!ActiveBgStr.IsInv())
                return BaseGlobal.RsIcon.Get(ActiveIconStr);
            return null;
        }
        #endregion
    }
    [AddComponentMenu("UI/Control/UCheck")]
    [HideMonoScript]
    public class UCheck : UPres<UCheckData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Text IName;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public GameObject ActiveObj;

        [FoldoutGroup("Inspector BG"), SerializeField, SceneObjectsOnly]
        public Image IBg;
        [FoldoutGroup("Inspector BG"),ShowIf("@IsSwitch"), SerializeField, SceneObjectsOnly]
        public Image IActiveBg;

        [FoldoutGroup("Inspector Icon"), SerializeField, SceneObjectsOnly]
        public Image IIcon;
        [FoldoutGroup("Inspector Icon"), ShowIf("@IsSwitch"), SerializeField, SceneObjectsOnly]
        public Image IActiveIcon;
        [FoldoutGroup("Inspector Icon"), SerializeField, SceneObjectsOnly]
        public Image IOverlayIcon;
        #endregion

        #region data
        [SerializeField, FoldoutGroup("Data"), Tooltip("此值为True的时候,当IsOn==true的时候关闭ActiveIcon,反之亦然")]
        bool Inverse = false;
        [SerializeField, FoldoutGroup("Data"), Tooltip("此值为True的时候,切换Bg/Icon和ActiveBg/ActiveIcon")]
        bool IsSwitch = true;
        [SerializeField, FoldoutGroup("Data")]
        float SwitchDuration = 0.1f;
        #endregion

        #region prop
        public bool IsOn { get; protected set; } = false;
        public bool IsToggleGroup
        {
            get
            {
                bool isPToggleGroup = false;
                if (PDupplicate != null)
                    isPToggleGroup = PDupplicate.GetIsToggleGroup();
                if (PScroll != null)
                    isPToggleGroup = PScroll.GetIsToggleGroup();
                if (Data != null && Data.IsOn != null)
                    isPToggleGroup = false;
                return isPToggleGroup;
            }
        }
        public bool IsHaveLink => Data.LinkControl != null || Data.LinkView != null;
        protected Sprite SourceSprite { get; private set; }
        protected Color SourceTextCol { get; private set; }
        protected Color SourceIconCol { get; private set; }
        protected Color SourceBgCol { get; private set; }
        #endregion

        #region life
        public override bool IsAtom => true;
        protected override void Awake()
        {
            base.Awake();
            if (IIcon != null)
            {
                SourceSprite = IIcon.sprite;
                SourceIconCol = IIcon.color;
            }
            if (IBg != null)
            {
                SourceBgCol = IBg.color;
            }
            if (IName != null)
            {
                SourceTextCol = IName.color;
            }
            if (IOverlayIcon != null)
            {
                IOverlayIcon.SetActive(true);
            }
        }
        public override void Init(UCheckData data)
        {
            base.Init(data);
            //自动获得激活图片,后缀有_N自动替换成_On 否则自动增加_On
            if (data.IsAutoIcon)
            {
                string sourceIcon = data.IconStr;
                if (!data.IconStr.EndsWith(SysConst.Suffix_Checkbox_N))
                {
                    data.IconStr = sourceIcon + SysConst.Suffix_Checkbox_N;
                }
                if (!data.ActiveIconStr.EndsWith(SysConst.Suffix_Checkbox_On))
                {
                    data.ActiveIconStr = sourceIcon + SysConst.Suffix_Checkbox_On;
                }
            }

            //link
            if (data.LinkView != null)
            {
                data.LinkView.Callback_OnShow += OnLinkShow;
            }
            if (data.LinkControl != null)
            {
                data.LinkControl.Callback_OnShow += OnLinkShow;
                if (data.LinkControl is UPanel panel)
                {
                    panel?.DettachFromPanelList();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Data != null)
            {
                if (Data.LinkView != null) Data.LinkView.Callback_OnShow -= OnLinkShow;
                if (Data.LinkControl != null) Data.LinkControl.Callback_OnShow -= OnLinkShow;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (!IsToggleGroup)
            {
                RefreshState();
            }
            else
            {
                RefreshStateBySelect();
            }
            RefreshEffect();
            RefreshActiveEffect();
            RefreshLink();
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            //如果不是ToggleGroup 则执行以下操作
            if (CheckCanClick() &&
                IsInteractable &&
                !IsToggleGroup)
            {
                RefreshStateByInput(!IsOn);
                Data.OnCheckClick?.Invoke(this, eventData, IsOn);
            }
            base.OnPointerClick(eventData);
            RefreshState();
            RefreshActiveEffect();
            RefreshLink();
        }
        #endregion

        #region set
        public void SetCheck(bool isOn)
        {
            RefreshStateByInput(isOn);
            RefreshActiveEffect();
            RefreshLink();
        }
        #endregion

        #region Callback
        private void OnLinkShow(bool arg1)
        {
            RefreshStateByLink();
            RefreshActiveEffect();
        }
        #endregion

        #region utile
        void RefreshState()
        {
            bool preIsOn = IsOn;
            if (Data.IsOn != null) IsOn = Data.IsOn.Invoke();
            else if (Data.LinkView != null) IsOn = Data.LinkView.IsShow;
            else if (Data.LinkControl != null) IsOn = Data.LinkControl.IsShow;
        }
        void RefreshStateByLink()
        {
            bool preIsOn = IsOn;
            if (Data?.LinkView != null) IsOn = Data.LinkView.IsShow;
            else if (Data?.LinkControl != null) IsOn = Data.LinkControl.IsShow;
            if (preIsOn != IsOn)
                Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByInput(bool inputState)
        {
            bool preIsOn = IsOn;
            IsOn = inputState;
            if (preIsOn != IsOn)
                Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshLink()
        {
            if (Data?.LinkControl != null) 
                Data?.LinkControl.ShowDirect(IsOn,false);
            else if (Data?.LinkView != null) 
                Data?.LinkView.ShowDirect(IsOn,false);
        }
        void RefreshEffect()
        {
            if (Data != null)
            {
                //刷新内容
                if (IName != null)
                {
                    IName.text = Data.GetName();
                }
                if (IIcon != null)
                {
                    var temp = Data.GetIcon();
                    if (temp != null)
                    {
                        IIcon.overrideSprite = temp;
                    }
                }
                if (IBg != null)
                {
                    var temp = Data.GetBg();
                    if (temp)
                    {
                        IBg.overrideSprite = temp;
                    }
                }
                if (IsSwitch)
                {
                    if (IActiveIcon != null)
                    {
                        var temp = Data.GetActiveIcon();
                        if (temp != null)
                        {
                            IActiveIcon.overrideSprite = temp;
                        }
                    }
                    if (IActiveBg != null)
                    {
                        var temp = Data.GetActiveBg();
                        if (temp)
                        {
                            IActiveBg.overrideSprite = temp;
                        }
                    }
                }
            }
        }
        void RefreshActiveEffect()
        {            
            //设置激活游戏对象
            if (ActiveObj != null)
            {
                if (IsOn)
                {
                    ActiveObj.SetActive(Inverse ? false : true);
                }
                else
                {
                    ActiveObj.SetActive(Inverse ? true : false);
                }
            }
            //叠加图标 overlay
            if (IOverlayIcon != null)
            {
                if (IsOn)
                {
                    IOverlayIcon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, SwitchDuration, true);
                }
                else
                {
                    IOverlayIcon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, SwitchDuration, true);
                }
            }
            //切换图标
            if (IsSwitch)
            {
                //Icon
                if (IIcon != null && IActiveIcon != null)
                {
                    IIcon.SetActive(true);
                    IActiveIcon.SetActive(true);
                    if (IsOn) IIcon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, SwitchDuration, true);
                    else IIcon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, SwitchDuration, true);
                    if (IsOn) IActiveIcon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, SwitchDuration, true);
                    else IActiveIcon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, SwitchDuration, true);
                }
                //Bg
                if (IBg != null && IActiveBg != null)
                {
                    IBg.SetActive(true);
                    IActiveBg.SetActive(true);
                    if (IsOn) IBg.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, SwitchDuration, true);
                    else IBg.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, SwitchDuration, true);
                    if (IsOn) IActiveBg.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, SwitchDuration, true);
                    else IActiveBg.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, SwitchDuration, true);
                }
            }
            //颜色变化
            if (Data != null)
            {
                //文字颜色变化
                if (IName != null && Data.ActiveTextCol != null)
                {
                    if (IsOn) IName.color = Inverse? SourceTextCol : Data.ActiveTextCol.Value;
                    else IName.color = Inverse? Data.ActiveTextCol.Value : SourceTextCol;
                }
                //Icon颜色变化
                if (IIcon != null && Data.ActiveIconCol != null)
                {
                    if (IsOn) IIcon.color = Inverse? SourceIconCol : Data.ActiveIconCol.Value;
                    else IIcon.color = Inverse? Data.ActiveIconCol.Value: SourceIconCol;
                }
                //Bg颜色变化
                if (IBg != null && Data.ActiveBgCol != null)
                {
                    if (IsOn) IBg.color = Inverse? SourceBgCol: Data.ActiveBgCol.Value;
                    else IBg.color = Inverse ? Data.ActiveBgCol.Value : SourceBgCol;
                }
            }
        }
        void RefreshStateBySelect()
        {
            if (PDupplicate != null)
                RefreshStateBySelect(PDupplicate.CurSelectIndex);
            else if (PScroll != null)
                RefreshStateBySelect(PScroll.CurSelectIndex);
            else
            {
                throw new Exception("没有BaseDupplicate 或者 BaseScroll," + GOName);
            }

            void RefreshStateBySelect(int index)
            {
                IsOn = index == Index;
                Data?.OnValueChanged?.Invoke(IsOn);
            }
        }
        public void RefreshStateAndActiveEffectBySelect()
        {
            RefreshStateBySelect();
            RefreshActiveEffect();
        }
        public void ForceCloseLink()
        {
            if (Data?.LinkControl != null) 
                Data?.LinkControl.ShowDirect(false, true);
            else if (Data?.LinkView != null) 
                Data?.LinkView.ShowDirect(false, true);
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return IName.text; }
            set { IName.text = value; }
        }
        public Sprite BgSprite
        {
            get { return IBg.sprite; }
            set { IBg.sprite = value; }
        }
        public Sprite ActiveBgSprite
        {
            get { return IActiveBg.sprite; }
            set { IActiveBg.sprite = value; }
        }
        public Sprite IconSprite
        {
            get { return IIcon.sprite; }
            set { IIcon.sprite = value; }
        }
        public Sprite ActiveIconSprite
        {
            get { return IActiveIcon.sprite; }
            set { IActiveIcon.sprite = value; }
        }
        public Sprite OverlayIconSprite
        {
            get { return IOverlayIcon.sprite; }
            set { IOverlayIcon.sprite = value; }
        }
        #endregion

        public override void AutoSetup()
        {
            base.AutoSetup();
            if (IName == null)
            {
                IName = gameObject.GetComponentInChildren<Text>();
            }
            if (IOverlayIcon == null)
            {
                var trans = transform.Find("Background/Checkmark");
                if (trans)
                {
                    IOverlayIcon = trans.GetComponentInChildren<Image>();
                }
            }
        }
    }
}