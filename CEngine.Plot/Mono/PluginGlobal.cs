//------------------------------------------------------------------------------
// PluginGlobal.cs
// Copyright 2022 2022/11/8 
// Created by CYM on 2022/11/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Plot;

namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        static PluginGlobal PluginPlot = new PluginGlobal
        {
            OnPostAddComponet = (x) => 
            {
                if (x is INarrationMgr<TDBaseNarrationData> plotMgr)
                {
                    NarrationMgr = plotMgr;
                }
            }
        };
        public static INarrationMgr<TDBaseNarrationData> NarrationMgr { get; protected set; }
        public static IStoryMgr<TDBaseStoryData> StoryMgr { get; protected set; }
        public static ITalkMgr TalkMgr { get; protected set; }

    }
}