using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class USliderData : UButtonData
    {
        public Callback<float> OnValueChanged;
        public Func<float, string> ValueText = (x) => { return UIUtil.Per(x); };
        public Func<float> Value = () => 0;
        public float MaxVal = 1.0f;
        public float MinVal = 0.0f;
        public string WVCClip = "UI_WVSlider";
    }
    [AddComponentMenu("UI/Control/USlider")]
    [HideMonoScript]
    [RequireComponent(typeof(Slider))]
    public class USlider : UPres<USliderData>
    {
        #region presenter
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        Text IName;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        Text IValue;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        RectTransform IFillRect;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        RectTransform IHandleRect;

        [FoldoutGroup("Data"), SerializeField, SceneObjectsOnly]
        Slider.Direction Direction = Slider.Direction.LeftToRight;
        #endregion

        #region prop
        Slider Com;
        bool isInRefresh = false;
        RectTransform[] childRectTrans;
        #endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            var tempCom = GO.SafeAddComponet<Slider>();
            if (tempCom)
            {
                tempCom.hideFlags = HideFlags.None;
                IFillRect = tempCom.fillRect;
                IHandleRect = tempCom.handleRect;
            }
        }
#endif

        #region life
        public override bool IsAtom => true; 
        protected override void Awake()
        {
            base.Awake();
            childRectTrans = GetComponentsInChildren<RectTransform>();
            Com = GO.SafeAddComponet<Slider>();
            ResetCom();
        }
        public override void Init(USliderData data)
        {
            base.Init(data);
            ResetCom();
        }
        void ResetCom()
        {
            if (Com != null)
            {
                isInRefresh = true;
                Com.fillRect = IFillRect;
                Com.handleRect = IHandleRect;
                Com.onValueChanged.RemoveAllListeners();
                Com.onValueChanged.AddListener(this.OnValueChanged);
                Com.maxValue = Data.MaxVal;
                Com.minValue = Data.MinVal;
                Com.value = Data.Value.Invoke();
                isInRefresh = false;
            }
        }
        public override void Cleanup()
        {
            if (Com != null)
                Com.onValueChanged.RemoveAllListeners();
            base.Cleanup();
        }
        public override void Refresh()
        {
            isInRefresh = true;
            base.Refresh();
            if (Com != null)
            {
                Com.maxValue = Data.MaxVal;
                Com.minValue = Data.MinVal;
                Com.value = Data.Value.Invoke();
            }
            if (IName != null)
                IName.text = Data.GetName();
            RefreshValueChange();
            isInRefresh = false;
        }
        #endregion

        #region set
        void RefreshValueChange()
        {
            if (IValue != null)
            {
                IValue.text = Data.ValueText.Invoke(Com.value);
            }
        }
        #endregion

        #region callback
        void OnValueChanged(float value)
        {
            Data.OnValueChanged?.Invoke(value);
            if (isInRefresh)
                return;
            RefreshValueChange();
            if (Com.wholeNumbers)
                PlayClip(Data.WVCClip);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            foreach (var item in childRectTrans)
            {
                if (item == Com.handleRect)
                    base.OnPointerClick(eventData);
            }
        }
        public override void OnInteractable(bool b, bool effect = true)
        {
            base.OnInteractable(b, effect);
            Com.interactable = b;
        }
        #endregion

        public override void AutoSetup()
        {
            base.AutoSetup();
        }
        [Button("RefreshLayout")]
        void Inspector_ChangeDirection()
        {
            var tempCom = GO.SafeAddComponet<Slider>();
            if (tempCom)
            {
                tempCom.SetDirection(Direction, true);
            }
        }
    }

}