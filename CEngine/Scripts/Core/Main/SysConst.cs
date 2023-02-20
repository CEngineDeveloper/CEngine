//**********************************************
// Class Name	: CYMConstans
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.IO;
using UnityEngine;

namespace CYM
{
    public partial class SysConst
    {
        #region member variable
        public const float LongPressDuration = 0.5f;
        public const int INT_Inv = int.MinValue;
        public const float FLOAT_Inv = float.NaN;
        public const long LONG_Inv = int.MinValue;
        public static readonly Vector3 VEC_GlobalPos = Vector3.one * 9999;
        public static readonly Vector3 VEC_FarawayPos = Vector3.one * 999999;
        public static readonly Vector3 VEC_MiniScale = Vector3.one * 0.00001f;
        public static readonly Vector3 VEC_WorldUICameraPos = new Vector3(9999, 9999, 0.0f);
        public static readonly Vector3 VEC_Inv = new Vector3(-9999, -9999, -9999);
        #endregion

        #region layer & tag
        public static readonly LayerData Layer_System = "System";
        public static readonly LayerData Layer_Default = "Default";
        public static readonly LayerData Layer_UI = "UI";
        public static readonly LayerData Layer_Water = "Water";
        public static readonly LayerData Layer_TransparentFX = "TransparentFX";
        public static readonly LayerData Layer_IgnoreRaycast = "Ignore Raycast";
        public static readonly LayerData Layer_Terrain = "Terrain";
        public static readonly string Tag_IgnorGUIBlock = "IgnorGUIBlock";
        #endregion

        #region bundle name
        public const string BN_Shared = "shared";
        public const string BN_Icon = "icon";
        public const string BN_Sprite = "sprite";
        public const string BN_Head = "head";
        public const string BN_Flag = "flag";
        public const string BN_Music = "music";
        public const string BN_Prefab = "prefab";
        public const string BN_Perform = "perform";
        public const string BN_Audio = "audio";
        public const string BN_Narration = "narration";
        public const string BN_Materials = "material";
        public const string BN_UI = "ui";
        public const string BN_BG = "bg";
        public const string BN_Texture = "texture";
        public const string BN_Illustration = "illustration";
        public const string BN_Scene = "scene";
        public const string BN_PhysicMaterial = "physicmaterial";
        public const string BN_Video = "video";
        public const string BN_AudioMixer = "audiomixer";
        public const string BN_Common = "common";
        public const string BN_Animator = "animator";
        public const string BN_System = "system";
        public const string BN_CSharp = "csharp";
        public const string BN_Excel = "excel";
        public const string BN_Language = "language";
        public const string BN_Lua = "lua";
        public const string BN_Text = "text";
        public const string BN_Config = "config";
        #endregion

        #region STR
        public const string STR_Line = "-----------------";
        public const string STR_DBTempSaveName = "TempSave";
        public const string STR_LuaTemplate = "Template";
        public const string STR_Infinite = "∞";
        public const string STR_None = "None";
        public const string STR_Inv = "";
        public const string STR_Unkown = "???";
        public const string STR_Indent = "<color=white>   *</color>";
        public const string STR_DoubbleIndent = "<color=white>    *</color>";
        public const string STR_IndentOr = "<color=white>   -</color>";
        public const string STR_Append = "  ";
        public const string STR_AppendStar = "  *";
        public const string STR_Space = " ";
        public const string STR_DoubbleSpace = "    ";
        public const string STR_Enter = "\n";
        public const string STR_Model = "Model";
        public const string STR_Desc_NoDesc = "Desc_NoDesc";
        public const string STR_Base = "Base";
        public const string STR_ABManifest = "AssetBundleManifest";
        public const string STR_NativeDLC = "Native";
        public const string STR_InternalDLC = "Internal";
        public const string STR_DLCManifest = "DLCManifest";
        public const string STR_DLCItem = "DLCItem";
        public const string STR_Custom = "Custom";
        public const string STR_AssetBundles = "AssetBundles";
        public const string STR_AssemblyEditor = "Assembly-CSharp-Editor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";        
        public const string STR_Assembly = "Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        public const string STR_AssemblyFirstpass = "Assembly-CSharp-firstpass, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        #endregion

        #region 语言表配置
        //注释前缀
        public const string Prefix_Lang_Notes = "#";
        //分类前缀
        public const string Prefix_Lang_Category = "@";
        //自动id
        public const string Prefix_Lang_AutoID = "*";
        //加载提示
        public const string Lang_Category_LoadTip = "LoadTip";
        #endregion

        #region Prefix
        //姓氏翻译
        public const string Prefix_Lang_Surname = "S_";
        //名字翻译
        public const string Prefix_Lang_Name = "N_";
        //事件选项提示
        public const string Prefix_Lang_OptHintTrans = "OptHint_";
        //城市前缀
        public const string Prefix_Castle = "Castle_";
        //提示前缀
        public const string Prefix_Tip = "Tip_";
        //翻译前缀
        public const string Prefix_Desc = "Desc_";
        //简介前缀
        public const string Prefix_Cont = "Cont_";
        //名字翻译前缀
        public const string Prefix_Name = "Name_";
        //场景前缀
        public const string Prefix_Battle = "Battle_";
        //存档前缀
        public const string Prefix_AutoSave = "AutoSave_";
        //快捷键
        public const string Prefix_Shortcut = "Shortcut_";
        //属性图标
        public const string Prefix_Attr = "Attr_";
        //星星图标
        public const string Prefix_Star = "Star_";
        #endregion

        #region Suffix
        //禁用图片后缀
        public const string Suffix_Disable = "_Dis";
        //选择图片的后缀
        public const string Suffix_Sel = "_Sel";
        //汉字旗帜后缀
        public const string Suffix_FlagHan = "_Han";
        //原型旗帜后缀
        public const string Suffix_FlagCircle = "_Circle";
        //方形旗帜后缀
        public const string Suffix_FlagSquare = "_Square";
        //提示描述后缀
        public const string Suffix_Tip = "_Tip";
        //简单地形
        public const string Suffix_Simple = "_Simple";
        //选项后缀,用于对话等系统
        public const string Suffix_Op = "_Op";
        //特效后缀
        public const string Suffix_Eff = "_eff";
        #endregion

        #region dir
        //texture文件夹
        public static readonly string Dir_Texture = "Texture";
        //temp文件夹
        public static readonly string Dir_Temp = "Temp";
        //lua文件夹
        public static readonly string Dir_Lua = "Lua";
        //语言文件包
        public const string Dir_Language = "Language";
        //配置文件
        public const string Dir_Config = "Config";
        //Const文件
        public const string Dir_Const = "Const";
        //ScriptTemplate
        public const string Dir_ScriptTemplate = "ScriptTemplate";
        //text assets文件夹
        public static readonly string Dir_TextAssets = "Text";
        //cs 文件夹
        public static readonly string Dir_CSharp = "CSharp";
        //excel 文件夹
        public static readonly string Dir_Excel = "Excel";
        //bundles
        public static readonly string Dir_Bundles = "_Bundles";
        //dlc目录
        public static readonly string Dir_Dlc = "_Dlc";
        //Art
        public static readonly string Dir_Art = "_Arts";
        //Art
        public static readonly string Dir_Res = "_Res";
        //ui
        public static readonly string Dir_UI = "UI";
        //Funcs
        public static readonly string Dir_Funcs = "_Funcs";
        //Tests
        public static readonly string Dir_Tests = "_Tests";
        //Gizmos
        public static readonly string Dir_Gizmos = "Gizmos";
        //CYMUni
        public static readonly string Dir_CEngine = "CEngine";
        //Resources
        public static readonly string Dir_Resources = "Resources";
        //Editor
        public static readonly string Dir_Editor = "Editor";
        //Plugin
        public static readonly string Dir_Plugins = "Plugins";
        #endregion

        #region format
        //场景Bundle的格式路径
        public readonly static string Format_BundleScenesPath = "/"+ Dir_Bundles + "/" + STR_NativeDLC + "/Scene/{0}.unity";
        public readonly static string Format_BundleSystemScenesPath = "/_Arts/Scene/{0}.unity";
        //全局配置资源的格式路径
        public readonly static string Format_ConfigAssetPath = "Assets/Resources/" + Dir_Config + "/{0}.asset";
        #endregion

        #region Regex
        //富文本,图标正则
        public readonly static string Regex_RichTextIcon = @"\[(.*?)\]";
        //富文本,动态参数正则
        public readonly static string Regex_RichTextDyn = @"\%(.*?)\%";
        #endregion

        #region logic path 
        //插件目录
        public static readonly string Path_Plugins = Path.Combine(Application.dataPath, Dir_Plugins);
        //美术资源文件夹
        public static readonly string Path_Arts = Path.Combine(Application.dataPath, Dir_Art);
        //美术资源UI文件夹
        public static readonly string Path_AtrUI = Path.Combine(Path_Arts, Dir_UI);
        //程序脚本文件夹
        public static readonly string Path_Funcs = Path.Combine(Application.dataPath, Dir_Funcs);
        //测试文件夹
        public static readonly string Path_Tests = Path.Combine(Application.dataPath, Dir_Tests);
        //Gizmos
        public static readonly string Path_Gizmos = Path.Combine(Application.dataPath, Dir_Gizmos);
        //CYMUni
        public static readonly string Path_CEngine = Path.Combine(Application.dataPath, Dir_Plugins, Dir_CEngine);
        //Resources
        public static readonly string Path_Resources = Path.Combine(Application.dataPath, Dir_Resources);
        //Resources
        public static readonly string Path_Editor = Path.Combine(Application.dataPath, Dir_Editor);
        //StreamingAssets
        public static readonly string Path_StreamingAssets = Application.streamingAssetsPath;
        public static readonly string Path_PersistentBundle = Path.Combine(Application.persistentDataPath,STR_AssetBundles);
        public static readonly string Path_Project = Application.dataPath;
        public static readonly string Path_Bundles = Path.Combine(Path_Project, Dir_Bundles);
        public static readonly string Path_Dlc = Path.Combine(Path_Project, Dir_Dlc);
        public static readonly string Path_TemplateConfig = Path.Combine(Path_CEngine, "_Res/Configs/TemplateFiles");
        #endregion

        #region 工程目录的相对路径
        public static readonly string RPath_Assets = "Assets/";
        public static readonly string RPath_CEngine = $"Assets/{Dir_Plugins}/{Dir_CEngine}/";
        public static readonly string RPath_Example = $"{RPath_CEngine}_Example/";
        public static readonly string RPath_Resources = "Assets/Resources/";
        public static readonly string RPath_CustomTempScript = Path.Combine(RPath_Resources, Dir_ScriptTemplate);
        public static readonly string RPath_Bundles = Path.Combine(RPath_Assets, Dir_Bundles);
        public static readonly string RPath_InternalBundle = Path.Combine(RPath_CEngine, Dir_Bundles);
        public static readonly string RPath_Dlc = Path.Combine(RPath_Assets, Dir_Dlc);
        public static readonly string RPath_TempCYMMonobehaviour = Path.Combine(RPath_CEngine, "_Res/Configs/TemplateFiles/CYMMonobehaviour.asset");
        public static readonly string RPath_TempMonobehaviour = Path.Combine(RPath_CEngine, "_Res/Configs/TemplateFiles/Monobehaviour.asset");
        public static readonly string RPath_TempCSharp = Path.Combine(RPath_CEngine, "_Res/Configs/TemplateFiles/CSharp.asset");
        #endregion

        #region Resource 路径
        public static readonly string Path_ResourcesTexture = Path.Combine(Path_Resources, Dir_Texture);
        public static readonly string Path_ResourcesText = Path.Combine(Path_Resources, Dir_TextAssets);
        public static readonly string Path_ResourcesTemp = Path.Combine(Path_Resources, Dir_Temp);
        public static readonly string Path_ResourcesConfig = Path.Combine(Path_Resources, Dir_Config);
        public static readonly string Path_ResourcesScriptTemplate = Path.Combine(Path_Resources, Dir_ScriptTemplate);
        public static readonly string Path_ResourcesConst = Path.Combine(Path_Resources, Dir_Const);
        #endregion

        #region 游戏目录
        //开发目录
        public static readonly string Path_Dev = Path.Combine(Application.persistentDataPath, "Dev");
        //本地截图
        public static readonly string Path_Log = Path.Combine(Application.persistentDataPath, "Log");
        //本地截图
        public static readonly string Path_Screenshot = Path.Combine(Application.persistentDataPath, "Screenshot");
        //本地存档
        public static readonly string Path_LocalDB = Path.Combine(Application.persistentDataPath, "Save");
        //本地存档
        public static readonly string Path_CloudDB = Path.Combine(Application.persistentDataPath, "CloudSave");
        //设置文件名称
        public static readonly string Path_Settings = Path.Combine(Application.persistentDataPath, "Settings.json");
        //快捷键
        public static readonly string Path_Shortcuts = Path.Combine(Application.persistentDataPath, "Shortcuts.json");
        //游戏Bin路径
        public readonly static string Path_Build = Path.GetFullPath(Path.Combine(Application.dataPath, "../Bin"));
        #endregion

        #region color
        public const string COL_Green = "#02FF07";
        public const string COL_Yellow = "yellow";
        public const string COL_Red = "#F51400";
        public const string COL_Grey = "grey";
        public static string CH_Green => string.Format("<color={0}>", COL_Green);
        public static string CH_Yellow => string.Format("<color={0}>", COL_Yellow);
        public static string CH_Red => string.Format("<color={0}>", COL_Red);
        public static string CH_Grey => string.Format("<color={0}>", COL_Grey);
        public static readonly Color Color_BuffPositive = UIUtil.FromHex("#136700FF");
        public static readonly Color Color_BuffNegative = UIUtil.FromHex("#670000FF");
        public static readonly Color Color_BuffAll = UIUtil.FromHex("#280067FF");
        #endregion

        #region ucd
        public const string UCD_NoBreakingSpace = "\u00A0";
        #endregion

        #region extention
        public static readonly string Extention_Save = ".dbs";
        public static readonly string Extention_AssetBundle = ".ab";
        #endregion

        #region file
        public static readonly string File_SteamAppID = "steam_appid.txt";
        public const string File_Download = "download.txt";
        #endregion

        #region ui const
        public const string kUILayerName = "UI";
        public const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        public const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        public const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        public const string kKnobPath = "UI/Skin/Knob.psd";
        public const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        public const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        public const string kMaskPath = "UI/Skin/UIMask.psd";
        #endregion

        #region scene
        public const string SCE_Start = "Start";
        public const string SCE_Preview = "Preview";
        public const string SCE_Test = "Test";
        #endregion
    }

}

