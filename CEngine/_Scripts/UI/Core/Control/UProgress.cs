using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public enum ProgressType
    {
        Horizontal,
        Vertical,
    }

    public class UProgressData : UTextData
    {
        public Func<float> Value = () => { return 0.0f; };
        public Func<float, string> ValueText = (x) => { return UIUtil.D2(x); };
        public bool IsTween = false;
        public float TweenDuration = 1.0f;
    }
    [AddComponentMenu("UI/Control/UProgress")]
    [HideMonoScript]
    public class UProgress : UPres<UProgressData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text IName;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text IValue;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Image IFill;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image IIcon;
        #endregion

        #region Data
        [FoldoutGroup("Data"), SerializeField]
        public bool IsFill = true;
        [FoldoutGroup("Data"), SerializeField, ShowIf("Inspector_ShowIsProgressType")]
        public ProgressType ProgressType = ProgressType.Horizontal;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空"), HideIf("Inspector_ShowIsProgressType")]
        public GameObject IFollowObj;
        #endregion

        #region prop
        Tweener tweener;
        float Amount = 1.0f;
        Vector2 SourceForceGroundSizeData = new Vector2();
        #endregion

        #region life
        public override bool IsAtom => true;
        protected override void Awake()
        {
            base.Awake();
            if (IFill != null)
                SourceForceGroundSizeData = IFill.rectTransform.sizeDelta;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                float perVal = Data.Value.Invoke();
                SetFillAmount(perVal, Data.IsTween,Data.TweenDuration);
                if (IValue != null)
                    IValue.text = Data.ValueText.Invoke(FillAmount);
                if (IIcon != null)
                    IIcon.sprite = Data.GetIcon();
                if (IName != null)
                    IName.text = Data.GetName();
            }
        }
        public virtual void Refresh(float val, string text)
        {
            SetFillAmount(val, false,0);
            if (IValue != null)
                IValue.text = text;
        }
        public virtual void Refresh(float val)
        {
            SetFillAmount(val, false, 0);
        }
        #endregion

        #region wrap
        public float FillAmount
        {
            get
            {
                if (IFill == null)
                    return 0;
                if (IsFill)
                    return IFill.fillAmount;
                else
                    return Amount;
            }
            set
            {
                if (IFill == null) return;
                Amount = value;
                if (IsFill)
                {
                    IFill.fillAmount = value;
                    if (IFollowObj != null)
                    {
                        RectTransform rectT = IFollowObj.transform as RectTransform;
                        if(IFill.fillMethod == Image.FillMethod.Horizontal)
                            rectT.anchoredPosition = new Vector2( IFill.fillAmount * SourceForceGroundSizeData.x, rectT.anchoredPosition.y);
                        else if (IFill.fillMethod == Image.FillMethod.Vertical)
                            rectT.anchoredPosition = new Vector2(rectT.anchoredPosition.x, IFill.fillAmount * SourceForceGroundSizeData.y);
                    }
                }
                else
                {
                    if (SourceForceGroundSizeData == Vector2.zero) 
                        return;
                    if (ProgressType == ProgressType.Horizontal)
                    {
                        IFill.rectTransform.sizeDelta = new Vector2(SourceForceGroundSizeData.x * Amount, SourceForceGroundSizeData.y);
                    }
                    else
                    {
                        IFill.rectTransform.sizeDelta = new Vector2(SourceForceGroundSizeData.x, SourceForceGroundSizeData.y * Amount);
                    }
                }
            }
        }
        public string NameText
        {
            get { return IName.text; }
            set { IName.text = value; }
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
        #endregion

        #region set
        public void SetFillAmount(float percent, bool isTween = false,float duration=0.1f)
        {
            if (IFill != null)
            {
                if (isTween)
                {
                    if (tweener != null) tweener.Kill();
                    tweener = DOTween.To(() => FillAmount, x => FillAmount = x, percent, duration);
                }
                else
                {
                    FillAmount = percent;
                }
            }
        }
        public void TweenFillAmount(float start,float end, float duration = 0.1f)
        {
            if (IFill != null)
            {
                FillAmount = start;
                SetFillAmount(end,true,duration);
            }
        }
        #endregion

        #region inspector
        bool Inspector_ShowIsProgressType()
        {
            return !IsFill;
        }
        public override void AutoSetup()
        {
            base.AutoSetup();
            if (IFill == null)
                IFill = GetComponentInChildren<Image>();
            if (IValue == null)
                IValue = GetComponentInChildren<Text>();
        }
        #endregion
    }

}