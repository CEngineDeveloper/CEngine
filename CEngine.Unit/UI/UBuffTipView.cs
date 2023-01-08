//------------------------------------------------------------------------------
// UBuffTipView.cs
// Created by CYM on 2021/10/8
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.UI;
using CYM.Unit;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UBuffTipView : UTooltipView
    {
        [SerializeField]
        Text BuffTitle;
        [SerializeField]
        Text CurTime;
        [SerializeField]
        Text Desc;

        public void Show(IBuffGroup group)
        {
            ITDBuffData buff = group.IBuff;
            BuffTitle.text = buff.GetName();
            CurTime.text = GetStr("Text_Buff剩余时间", buff.PercentCDStr);
            Desc.text = buff.GetAdtDesc();
            Show(true);
        }
    }
}