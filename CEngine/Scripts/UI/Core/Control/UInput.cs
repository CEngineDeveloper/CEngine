using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UInputData : UData
    {
        public Callback<string> OnValueChange;
        public Callback<string> OnEndEdit;
    }
    [AddComponentMenu("UI/Control/UInput")]
    [HideMonoScript]
    [RequireComponent(typeof(InputField))]
    public class UInput : UPres<UInputData>
    {
        #region prop
        InputField Com;
        #endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            var tempCom = GO.SafeAddComponet<InputField>();
            if (tempCom)
            {
                tempCom.hideFlags = HideFlags.None;
            }
        }
#endif

        #region life
        public override bool IsAtom => true;
        public override void Init(UInputData data)
        {
            base.Init(data);
            Com.onValueChanged.AddListener(this.OnValueChanged);
            Com.onEndEdit.AddListener(this.OnEndEdit);
        }
        protected override void Awake()
        {
            base.Awake();
            Com = GO.SafeAddComponet<InputField>();
            if (Com)
            {
            }
        }
        protected override void OnDestroy()
        {
            Com.onValueChanged.RemoveAllListeners();
            base.OnDestroy();
        }
        #endregion

        #region get
        /// <summary>
        /// 输入的字符窜
        /// </summary>
        public string InputText
        {
            get
            {
                return Com.text;
            }
            set
            {
                Com.text = value;
            }
        }
        #endregion

        #region set
        public void EnableInput(bool b)
        {
            Com.interactable = b;
            Com.readOnly = !b;
        }
        #endregion

        #region is
        public bool IsHaveText()
        {
            return !InputText.IsInv();
        }
        public bool IsHaveInputText()
        {
            return !Com.textComponent.text.IsInv();
        }
        #endregion

        #region Callback
        public override void OnInteractable(bool b, bool effect = true)
        {
            base.OnInteractable(b, effect);
            Com.readOnly = !b;
            Com.interactable = b;
        }
        void OnValueChanged(string text)
        {
            Data?.OnValueChange?.Invoke(text);
        }
        void OnEndEdit(string text)
        {
            Data?.OnEndEdit?.Invoke(text);
        }
        #endregion
    }

}