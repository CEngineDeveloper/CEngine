//------------------------------------------------------------------------------
// ExtensionEnum.cs
// Created by CYM on 2021/9/13
// 填写类的描述...
//------------------------------------------------------------------------------

using System;

namespace CYM
{
    #region Attr
    // 消耗加城因子使用类型
    public enum UpFactionType
    {
        Percent,  // 百分比乘法 Val * InputVal * Faction + Add;
        PercentAdd,  // 百分比增加 Val * ((1 + InputVal) * Faction) + Add;
        LinerAdd, // 线性附加 Val + InputVal * Faction + Add;
        PowAdd,   // 指数附加 Pow(InputVal, Faction) + Add;
    }
    // 属性加成因子使用的类型
    public enum AttrFactionType
    {
        None,       //忽略加成因子
        DirectAdd,     //直接增加val+faction
        PercentAdd,    //百分比增加val*(1+faction)
        DirectMul,     //直接相乘val*faction
    }
    // 条件类型
    public enum ACCType
    {
        And,
        Or,
    }
    // 条件比较
    public enum ACCompareType
    {

        More,       // 大于
        MoreEqual,  // 大于等于  
        Less,       // 小于
        LessEqual,  // 小于等于
        Equal,      // 等于
        Have,       // 拥有
        NotHave,    // 没有
        Is,         // 是
        Not,        // 不是
    }
    // 数字类型
    public enum NumberType
    {
        Normal,     //正常显示,选择性2位小数
        D2,         //三位小数
        Percent,    //百分比 e.g. 10%,选择性1位小数
        KMG,        //单位显示 e.g. 1K/1M/1G,数字小于1K取整 
        Integer,    //取整
        Bool,       //布尔 0:false 1:true
    }
    // 属性操作类型
    public enum AttrOpType
    {
        DirectAdd = 0,  //直接累加
        PercentAdd,     //百分比累加
        Direct,         //直接赋值
        Percent,        //百分比赋值
    }
    // 属性类型
    public enum AttrType
    {
        Fixed,      // 固定值,比如最大的血量        
        Dynamic,    // 动态值,比如当前的血量
    }
    // 属性增益类型
    public enum AttrBuffType
    {
        Forward,
        Backward,
        Middle,
    }
    #endregion

    #region DB
    public enum GameNetMode
    {
        PVP,
        PVE,
    }
    public enum GamePlayStateType
    {
        NewGame,//新游戏
        LoadGame,//加载游戏
        Tutorial,//教程
    }
    #endregion

    #region Balance
    public enum BalanceActionType
    {
        Hire,
        Fire,
    }
    #endregion

    #region BGM
    public enum BGMType
    {
        MainMenu,
        Battle,
        Credits,
    }
    #endregion

    #region Buff
    //buff 合并类型,BuffGroupID的优先级高于所有类型
    public enum BuffMergeType
    {
        None, // 重置CD
        CD,   // 增加CD  
        Group,// 叠加
    }
    public enum RemoveBuffType
    {
        Once,//移除一层
        Group,//移除一组
    }
    #endregion

    #region Date Time
    public enum GameDateTimeType
    {
        AD,
        BC,
    }
    #endregion

    #region UI
    public enum Corner : int
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
    }
    public enum Anchoring
    {
        None,
        Corners,     //Normal
        LeftOrRight, //左右
        TopOrBottom, //上下
        Cant,//斜面
    }
    public enum Anchor
    {
        None = -1,
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
        Left,
        Right,
        Top,
        Bottom
    }
    #endregion

    #region Level
    public enum LevelLoadType
    {
        Scene, //直接加载场景
        Prefab,//加载Prefab
    }
    #endregion

    #region Loader
    public enum LoadEndType
    {
        Success = 0,
        Failed = 1,
    }
    #endregion

    #region UI
    public enum SaveOrLoad
    {
        Save,
        Load,
    }
    #endregion

    #region Misc
    public enum NodeType
    {
        None = -1,
        Footsteps = 0,
        Breast = 1,
        Spine = 2,
        LFoot = 3,
        RFoot = 4,
        Head = 5,
        LHand = 6,
        RHand = 7,

        //虚拟点位
        Top = 8,
        Center = 9,
        Pivot = 10,

        //可选骨骼,如果没有,则不设置
        Muzzle = 20,//发射口
    }
    public enum WindowType
    {
        Windowed,
        MaximizedWindow,
        Fullscreen,
        ExclusiveFullScreen,
    }
    public enum CameraHightType
    {
        Less,
        Low,
        Mid,
        More,
        Top,
        Most,
    }
    #endregion
}