using System;

namespace CYM
{
    #region enum
    public enum GameDiffType
    {
        Simple,//简单
        Ordinary,//普通
        Difficulty,//困难
        Extremely,//变态
    }
    public enum GamePropType
    {
        Low,//简单
        Middle,//普通
        Hight,//困难
    }
    public enum Direct : int
    {
        Right = -1,
        Left = 1,
        Up = 2,
        Down = -2,
    }
    /// <summary>
    /// The easing type
    /// </summary>
    public enum TweenType
    {
        immediate,
        linear,
        spring,
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce,
        easeInBack,
        easeOutBack,
        easeInOutBack,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic
    }
    #endregion

    #region Builder
    public enum VersionTag
    {
        Preview,//预览版本
        Beta,//贝塔版本
        Release,//发行版本
    }

    public enum Platform
    {
        Windows64,
        Android,
        IOS,
    }
    /// <summary>
    /// 发布类型
    /// </summary>
    public enum BuildType
    {
        Develop,//内部开发版本
        Public,//上传发行版本
    }
    #endregion

    #region TD Config
    public enum CloneType
    {
        Memberwise, //浅层拷贝,拷贝所有值字段
        Deep,       //拷贝所有值字段,包括用户自定义的深层拷贝
    }
    #endregion

    #region Mgr
    public enum MgrType
    {
        Global,
        Unit,
        All,
    }
    #endregion

    #region Mono
    // mono 的类型
    public enum MonoType
    {
        None = 0,
        Unit = 1,
        Global = 2,
        View = 4,
        Normal = 8,
    }
    #endregion

    #region misc

    public enum SpriteDirRoot
    {
        Bundle,
        Art,
    }

    public enum LogoType
    {
        Image,
        Video,
    }
    public enum FontType
    {
        None = -1,
        Normal = 0,
        Title = 10,
        Dynamic = 100,
    }
    #endregion

    #region Lang
    [Serializable]
    public enum LanguageType : int
    {
        Chinese = 0,
        Traditional = 1,
        English = 2,
        Japanese = 3,
        Spanish = 4,
        Classical = 5,
    }
    #endregion
}
