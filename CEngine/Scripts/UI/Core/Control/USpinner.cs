//------------------------------------------------------------------------------
// USpinner.cs
// Created by CYM on 2021/12/18
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class USpinnerData : UTextData
    {
        public UButtonData LeftData = new UButtonData();
        public UButtonData RightData = new UButtonData();
        public Callback<UControl, PointerEventData> OnLeftClick;
        public Callback<UControl, PointerEventData> OnRightClick;
        public Callback<bool> OnValueChange;
        public Func<string> Value;
    }
    public class USpinner : UPres<USpinnerData>
    {
        #region Inspector
        [Required, ChildGameObjectsOnly]
        public Text IVal;
        [Required, ChildGameObjectsOnly]
        public UButton ILeft;
        [Required, ChildGameObjectsOnly]
        public UButton IRight;
        #endregion

        #region Life
        public override void Init(USpinnerData data)
        {
            base.Init(data);
            
            ILeft?.Init(data.LeftData);
            IRight?.Init(data.RightData);

            data.LeftData.OnClick += OnLeft;
            data.RightData.OnClick += OnRight;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Value != null && IVal)
            {
                IVal.text = Data.Value.Invoke();
            }
        }
        #endregion

        #region Callback
        private void OnValueChange(bool isleft)
        {
            Data?.OnValueChange?.Invoke(isleft);
            if (Data.Value != null && IVal)
            {
                IVal.text = Data.Value.Invoke();
            }
        }
        private void OnLeft(UControl arg1, PointerEventData arg2)
        {
            Data.OnLeftClick?.Invoke(arg1,arg2);
            OnValueChange(true);
        }
        private void OnRight(UControl arg1, PointerEventData arg2)
        {
            Data.OnRightClick?.Invoke(arg1, arg2);
            OnValueChange(false);
        }
        #endregion
    }
}