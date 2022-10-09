//**********************************************
// Class Name	: CYMBaseDynamicScript
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.DLC;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CYM
{
    public class BaseLuaMgr : BaseGFlowMgr, ILoader
    {
        #region callback val
        public event Callback Callback_OnParseEnd;
        public event Callback Callback_OnParseStart;
        #endregion

        #region property
        /// <summary>
        /// lua管理器列表类
        /// </summary>
        public static List<ITDConfig> TDConfigList { get; private set; } = new List<ITDConfig>();
        public static Dictionary<Type, ITDConfig> TDConfigDic { get; private set; } = new Dictionary<Type, ITDConfig>();
        public static Dictionary<string, ITDConfig> TDConfigDicStr { get; private set; } = new Dictionary<string, ITDConfig>();
        public static Script Lua { get; protected set; } = new Script();
        public static readonly string CoreLuaString = @"
                    --复制表格所有数据
                    CopyPairs = function(targetTable, sourceTable)
                      for key, val in pairs(sourceTable) do
		                    targetTable[key] = val;
                      end
                      return targetTable;
                    end

                    --获得一个新的表格
                    GetNewTable=function()
                        local t = { }
	                    return t;
                    end
        ";
        #endregion

        #region methon
        public object this[string key]
        {
            get { return Lua.Globals[key]; }
            set { Lua.Globals[key] = value; }
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
        }
        public void DoFileByFullPath(string path)
        {
            CLog.Debug("DoLua=>" + path);
            try
            {
                path = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(path));
                Lua.DoFile(path);
            }
            catch (Exception e)
            {
                CLog.Error("文件<" + path + ">编译错误" + e.ToString());
            }
        }
        public void DoFullDirection(string path)
        {
            string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                string newStr = item.Replace('\\', '/');
                DoFileByFullPath(newStr);
            }
        }
        public void DoDirection(string path)
        {
            CLog.Debug("DoDirection=>" + path);
            string[] fileNames = Directory.GetFiles(path, "*.txt");
            for (var i = 0; i < fileNames.Length; i++)
            {
                string fullPath = fileNames[i];
                DoFileByFullPath(fullPath);
            }
        }
        public virtual void DoString(string luaStr, string fileName = "", bool isLogFileName = true)
        {
            if (fileName != "" && isLogFileName)
            {
                CLog.Debug("DoString=>" + fileName);
            }
            try
            {
                Lua.DoString(luaStr);
            }
            catch (Exception e)
            {
                CLog.Error("文件<" + fileName + ">编译错误" + e.ToString() + "\n");
            }
        }
        public void DoString(string[] luaStrs)
        {
            for (int i = 0; i < luaStrs.Length; ++i)
            {
                DoString(luaStrs[i]);
            }
        }
        public static void AddGlobalAction(string key, Action<DynValue> action)
        {
            Lua.Globals[key] = action;
        }
        #endregion

        #region Life
        public override void OnEnable()
        {
            base.OnEnable();
            foreach (var item in TDConfigList)
            {
                BaseGlobal.LuaMgr.Callback_OnParseStart += item.OnLuaParseStart;
                BaseGlobal.LuaMgr.Callback_OnParseEnd += item.OnLuaParseEnd;
                BaseGlobal.ExcelMgr.Callback_OnParseStart += item.OnExcelParseStart;
                BaseGlobal.ExcelMgr.Callback_OnParseEnd += item.OnExcelParseEnd;
            }
            DoString(CoreLuaString);
            DoString(CustomCoreLuaString());
        }
        public override void OnDisable()
        {
            foreach (var item in TDConfigList)
            {
                BaseGlobal.LuaMgr.Callback_OnParseStart -= item.OnLuaParseStart;
                BaseGlobal.LuaMgr.Callback_OnParseEnd -= item.OnLuaParseEnd;
                BaseGlobal.ExcelMgr.Callback_OnParseStart -= item.OnExcelParseStart;
                BaseGlobal.ExcelMgr.Callback_OnParseEnd -= item.OnExcelParseEnd;
            }
            TDConfigList.Clear();
            TDConfigDic.Clear();
            TDConfigDicStr.Clear();
            base.OnDisable();
        }
        protected virtual string CustomCoreLuaString()
        {
            return "";
        }
        protected override void OnAllLoadEnd1()
        {
            base.OnAllLoadEnd1();
            foreach (var item in TDConfigList)
                item.OnAllLoadEnd1();
        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            foreach (var item in TDConfigList)
                item.OnAllLoadEnd2();
        }
        #endregion

        #region set

        #endregion

        #region Add & Get Config
        public static ITDConfig GetTDConfig(Type luaKey)
        {
            if (TDConfigDic.ContainsKey(luaKey))
                return TDConfigDic[luaKey];
            else
            {
                return null;
            }
        }
        public static ITDConfig GetTDConfig(string luaKey)
        {
            if (TDConfigDicStr.ContainsKey(luaKey))
                return TDConfigDicStr[luaKey];
            else
            {
                return null;
            }
        }
        public static void AddTDConfig(string tableKey, Type luaKey, ITDConfig tdConfig)
        {
            TDConfigList.Add(tdConfig);
            if (luaKey != null && !TDConfigDic.ContainsKey(luaKey))
            {
                TDConfigDic.Add(luaKey, tdConfig);
            }
            if (tableKey != null && !TDConfigDicStr.ContainsKey(tableKey))
            {
                TDConfigDicStr.Add(tableKey, tdConfig);
            }
        }
        #endregion

        #region Call func
        public static DynValue CallTableFunc(Table table, string funcName, params object[] args)
        {
            if (table == null)
            {
                CLog.Error("没有Table");
                return null;
            }
            var dyn = table.Get(funcName);
            if (dyn == null)
            {
                CLog.Error($"没有这个Func:{funcName}");
            }
            var func = dyn.Function;
            if (func == null)
            {
                CLog.Error($"没有这个Func:{funcName}");
            }
            return func.Call(args);
        }
        public static DynValue SafeCallTableFunc(Table table, string funcName, params object[] args)
        {
            if (table == null)
                return null;
            var dyn = table.Get(funcName);
            if (dyn == null)
            {
                return null;
            }
            var func = dyn.Function;
            if (func == null)
            {
                return null;
            }
            return func.Call(args);
        }
        #endregion

        #region Register
        public static void RegisterType<T>()
            where T : class
        {
            UserData.RegisterType<T>();
        }

        public static void RegisterObj<T>()
            where T : class, new()
        {
            UserData.RegisterType<T>();
            DynValue obj = null;
            obj = UserData.Create(new T());
            Lua.Globals[nameof(T)] = obj;
        }
        public static void RegisterStatic(Type type)
        {
            UserData.RegisterType(type);
            Lua.Globals[type.Name] = type;
        }
        public static void RegisterInstance<T>(string name,T data)
            where T : class
        {
            UserData.RegisterType<T>();
            Lua.Globals[name] = data;
        }
        #endregion

        #region Register Custom
        public static void RegisterFunc<T>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T>),
                v =>
                {
                    var function = v.Function;
                    return (Func<T>)(() => function.Call().ToObject<T>());
                }
            );
        }
        public static void RegisterFunc<T1, TResult>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, TResult>),
                v =>
                {
                    var function = v.Function;
                    return (Func<T1, TResult>)((T1 p1) => function.Call(p1).ToObject<TResult>());
                }
            );
        }
        public static void RegisterCallback<T>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Callback<T>),
                v =>
                {
                    var function = v.Function;
                    return (Callback<T>)(p => function.Call(p));
                }
            );
        }
        public static void RegisterCallback<T1,T2>()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Callback<T1,T2>),
                v =>
                {
                    var function = v.Function;
                    return (Callback<T1,T2>)((p1,p2) => function.Call(p1,p2));
                }
            );
        }
        public static void RegisterCallback()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Callback),
                v =>
                {
                    var function = v.Function;
                    return (Callback)(() => function.Call());
                }
            );
        }
        #endregion

        #region get
        public static Table GetTable(string name)
        {
            var dyn = Lua.Globals.Get(name);
            if (dyn == null)
            {
                return null;
            }
            var table = dyn.Table;
            if (table == null)
            {
                return null;
            }
            return table;
        }
        public static string GetTableStr(Table table,string key)
        {
            if (table == null)
                return "";
            var dyn = table.Get(key);
            if (dyn == null || table == null)
            {
                return "";
            }
            return dyn.String;
        }
        public static int GetTableInt(Table table, string key)
        {
            if (table == null)
                return 0;
            var dyn = table.Get(key);
            if (dyn == null || table == null)
            {
                return 0;
            }
            return (int)dyn.Number;
        }
        public static float GetTableFloat(Table table, string key)
        {
            if (table == null)
                return 0;
            var dyn = table.Get(key);
            if (dyn == null || table == null)
            {
                return 0;
            }
            return (float)dyn.Number;
        }
        #endregion

        #region loader
        public IEnumerator Load()
        {
            Callback_OnParseStart?.Invoke();
            SafeDic<int, List<string>> ExcuteStrDic = new SafeDic<int, List<string>>();
            //加载DLC Lua
            foreach (var dlc in DLCManager.LoadedDLCItems.Values)
            {
                if (BuildConfig.Ins.IsEditorOrConfigMode)
                {
                    var files = dlc.GetAllLuas();
                    for (int i = 0; i < files.Length; ++i)
                    {
                        DoString(File.ReadAllText(files[i]), files[i]);
                        BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load Lua " + files[i];
                    }
                }
                else
                {
                    var assetBundle = dlc.LoadRawBundle(SysConst.BN_Lua);
                    if (assetBundle != null)
                    {
                        foreach (var lua in assetBundle.LoadAllAssets<TextAsset>())
                        {
                            DoString(lua.text, lua.name);
                            BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load Lua " + lua.name;
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            Callback_OnParseEnd?.Invoke();
            yield break;
        }
        public string GetLoadInfo()
        {
            return "Load Lua";
        }
        #endregion
    }

}

