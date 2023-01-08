//------------------------------------------------------------------------------
// UTestView.cs
// Created by CYM on 2021/9/19
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
namespace CYM.UI
{
    public class UTestView : UUIView
    {
        #region Inspector
        [SerializeField]
        UDupplicate Dupplicate;
        [SerializeField]
        UScroll ScrollV;
        [SerializeField]
        UDupplicate DPCheckBox;
        [SerializeField]
        UText ScrollDesc;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Dupplicate.Init(
                new UButtonData { NameKey = "新游戏" },
                new UButtonData { NameKey = "加载游戏" },
                new UButtonData { NameKey = "设置" },
                new UButtonData { NameKey = "退出" }
                );
            ScrollV.Init(ScrollData,ScrollRefresh);
            DPCheckBox.Init(
                new UCheckData { NameKey = "选项1" },
                new UCheckData { NameKey = "选项2" },
                new UCheckData { NameKey = "选项3" }
                );
            ScrollDesc.Init(new UTextData { Name = ()=>
                @"aaaaaaaaaaaaaaaaaaaaa
                aaaaaaadddddddddddddd
                aaaaaaaaaaaaaaaaaaa
                eq阿斯顿发射点十分士大夫撒旦
                dsfdsasdasda
                asuizxcxcasda
                啊实打实打算
                阿三大苏打实打实大苏打
                asdaszcz
                asdasdasdasdfsdfsdfs
                fsdfsdsdfsd
                啊手动阀实打实大苏打
                asdassdfsdfgdsfgds
                撒旦发射点发射点发射点风格
                啊实打实的发发撒打发
                撒地方撒旦发射点发
                asdasdfsadfsadfas" });
            CLog.Red("界面热更新");
        }
        #endregion

        #region Callback
        private void ScrollRefresh(object arg1, object arg2)
        {
            UText press = arg1 as UText;
            press.NameText = press.DataIndex.ToString() ;
        }

        private IList ScrollData()
        {
            return UIUtil.GetTestScrollData();
        }
        #endregion
    }
}