//------------------------------------------------------------------------------
// UTutorialView.cs
// Copyright 2022 2022/11/5 
// Created by CYM on 2022/11/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
namespace CYM.Example
{
    public class UExampleView : UStaticUIView<UExampleView> 
    {
        #region Inspector
        [SerializeField]
        UImage UImage;
        [SerializeField]
        UText UText;
        [SerializeField]
        UText UBgText;
        [SerializeField]
        UCustom UIconAttr;
        [SerializeField]
        UButton UButton;
        [SerializeField]
        UDropdown UDropdown;
        [SerializeField]
        UCheck UCheck;
        [SerializeField]
        UInput UInput;
        [SerializeField]
        UProgress UProgress;
        [SerializeField]
        USlider USlider;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            UImage.Init(new UImageData { IconStr = "Empty" });
            UText.Init(new UTextData { Name =()=> "Name" });
            UBgText.Init(new UTextData { Name = ()=> "这是一段测试文字" });
            UIconAttr.Init(new UCustomData { Icon1 = ()=>"Empty".GetIcon(),Name1 = ()=>"生命",Name2 = ()=>"10".Green() });
            UButton.Init(new UButtonData { NameKey = "点击按钮" });
            UDropdown.Init(new UDropdownData { Opts =()=>new string[] { "选项1","水果","香蕉","娃哈哈" } });
            UCheck.Init(new UCheckData { NameKey = "背景音乐" });
            UInput.Init(new UInputData { });
            UProgress.Init(new UProgressData { Value = ()=>0.5f });
            USlider.Init(new USliderData { Value = ()=>0.3f });
        }
        #endregion
    }
}