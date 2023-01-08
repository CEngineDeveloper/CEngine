using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UDropdownData : UButtonData
    {
        public Type Enum=null;
        public Func<string[]> Opts;
        public Callback<int> OnValueChanged;
        public Func<int> Value;
    }
    [AddComponentMenu("UI/Control/UDropdown")]
    [HideMonoScript]
    [RequireComponent(typeof(Dropdown))]
    public class UDropdown : UPres<UDropdownData>
    {
        #region presenter
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly]
        Text IName;
        #endregion

        #region prop
        string[] opts;
        Dropdown Com;
        #endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            var tempCom = GO.SafeAddComponet<Dropdown>();
            if (tempCom)
            {
                tempCom.hideFlags = HideFlags.None;
            }
        }
#endif

        #region life
        public override bool IsAtom => true;
        protected override void Awake()
        {
            base.Awake();
            Com = GO.SafeAddComponet<Dropdown>();
            if (Com)
            {
                Com.template.gameObject.SetActive(false);
                Com.alphaFadeSpeed = 0.15f;
            }
        }
        public override void Init(UDropdownData data)
        {
            base.Init(data);
            if (Com != null)
            {
                Com.onValueChanged.AddListener(this.OnValueChanged);
            }
            if (Data.Enum != null)
            {
                opts = Data.Enum.GetEnumNames();
                for(int i=0;i<opts.Length;++i)
                {
                    opts[i] = (Data.Enum.Name + "." + opts[i]).GetName();
                }
            }
        }
        public override void Cleanup()
        {
            if (Com != null)
            {
                Com.ClearOptions();
                Com.onValueChanged.RemoveAllListeners();
            }
            base.Cleanup();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Com != null)
            {                
                List<Dropdown.OptionData> listOp = new List<Dropdown.OptionData>();
                if (opts != null)
                {
                    foreach (var item in opts)
                        listOp.Add(new Dropdown.OptionData(item));
                }
                else if (Data.Opts != null)
                {
                    foreach (var item in Data.Opts.Invoke())
                        listOp.Add(new Dropdown.OptionData(item));
                }
                Com.ClearOptions();
                Com.AddOptions(listOp);

                if (Data.IsInteractable != null)
                    Com.interactable = Data.IsInteractable.Invoke(Index);
                if (Data.Value != null)
                    Com.SetValueWithoutNotify(Data.Value.Invoke());
            }
            if (IName != null)
                IName.text = Data.GetName();
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }
        void OnValueChanged(int index)
        {
            Data.OnValueChanged?.Invoke(index);
            PlayClickAudio();
        }
        #endregion
    }

}