using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTextData : UData
    {
        public bool IsTrans = true;

        #region 函数
        public Func<Sprite> Icon = null;
        public Func<Sprite> Bg = null;
        public Func<string> Name = null;
        public Func<TDBaseData> Rename = null;
        public Func<Color> Color = null;
        #endregion

        #region 简化设置
        public string DefaultStr = SysConst.STR_Inv;
        public string BgStr = SysConst.STR_Inv;
        public string NameKey = SysConst.STR_Inv;
        public string IconStr = SysConst.STR_Inv; //如果没有设置,默认会使用NameKey
        #endregion

        #region get
        public string GetName()
        {
            string dynStr = SysConst.STR_Inv;
            string staStr = NameKey;
            if (Name != null)
            {
                dynStr = Name.Invoke();
            }
            if (staStr.IsInv() && dynStr.IsInv())
            {
                return DefaultStr;
            }
            if (!dynStr.IsInv())
            {
                return dynStr;
            }
            else return GetTransStr(staStr);
        }
        public Sprite GetIcon()
        {
            if (Icon != null)
            {
                return Icon.Invoke();
            }
            if (!IconStr.IsInv())
                return BaseGlobal.RsIcon.Get(IconStr);
            else if (!NameKey.IsInv() && BaseGlobal.RsIcon.IsHave(NameKey))
                return BaseGlobal.RsIcon.Get(NameKey);
            return null;
        }
        public Sprite GetBg()
        {
            if (Bg != null)
            {
                return Bg.Invoke();
            }
            if (!BgStr.IsInv())
                return BaseGlobal.RsIcon.Get(BgStr);
            return null;
        }
        public string GetTransStr(string str)
        {
            if (IsTrans)
                return BaseLangMgr.Get(str);
            return str;
        }
        public Color? GetColor()
        {
            if (Color == null)
                return null;
            return Color.Invoke();
        }
        #endregion
    }
    [AddComponentMenu("UI/Control/UText")]
    [HideMonoScript]
    public class UText : UPres<UTextData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text IName;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image IIcon;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image IBg;
        [FoldoutGroup("Data"),SerializeField]
        bool IsAnim = false;
        #endregion

        #region prop
        Tween Tween;
        #endregion

        #region life
        public override bool IsAtom => true;
        protected override void Awake()
        {
            base.Awake();
            if(IName is RichText)
                RichName = (RichText)IName;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (IName != null)
            {
                NameText = Data.GetName();
                //修改字体的颜色
                var tempColor = Data.GetColor();
                if (tempColor != null)
                {
                    IName.color = tempColor.Value;
                }
            }
            if (IIcon != null)
            {
                IIcon.sprite = Data.GetIcon();
            }
            if (IBg != null)
            {
                IBg.sprite = Data.GetIcon();
            }
        }
        public void Refresh(string key, params object[] objs)
        {
            if (IName) IName.text = BaseLangMgr.Get(key, objs);
        }
        public void Refresh(string desc)
        {
            if (IName) IName.text = desc;
        }
        #endregion

        #region callback
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (!CheckCanClick()) return;
            if (Data.Rename != null)
            {
                URenameView.Default?.Show(Data.Rename.Invoke());
            }
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return IName.text; }
            set
            {
                if (IsAnim)
                {
                    if (Tween != null)
                        Tween.Kill();
                    Tween = DOTween.To(() => IName.text, (x) => IName.text = x, value, 0.3f);
                }
                else
                {
                    IName.text = value;
                }
            }
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
            get { if (IIcon == null) return Color.white; return IIcon.color; }
            set { if (IIcon == null) return; IIcon.color = value; }
        }
        public Color BgColor
        {
            get { if (IBg == null) return Color.white; return IBg.color; }
            set { if (IBg == null) return; IBg.color = value; }
        }
        public bool IsAnimation
        {
            get { return IsAnim; }
            set { IsAnim = value; }
        }
        public RichText RichName
        {
            get; private set;
        }
        #endregion

        #region inspector
        public override void AutoSetup()
        {
            base.AutoSetup();
            if(IName==null)
                IName = GetComponent<Text>();
            if (IIcon == null)
                IIcon = GetComponentInChildren<Image>();
        }
        #endregion
    }
}
