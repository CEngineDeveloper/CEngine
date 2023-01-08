//------------------------------------------------------------------------------
// BaseExcelMgr.cs
// Copyright 2019 2019/1/30 
// Created by CYM on 2019/1/30
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.DLC;
using CYM.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace CYM
{
    public class BaseExcelMgr : BaseGFlowMgr, ILoader
    {
        #region prop
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //默认从第二行读取配置字段
        protected virtual int StartRowCount => 2;
        #endregion

        #region Callback val
        public event Callback Callback_OnParseEnd;
        public event Callback Callback_OnParseStart;
        #endregion

        #region static excel
        public static WorkBook ReadWorkbook(byte[] buffer)
        {
            try
            {
                var book = new WorkBook(buffer);
                return book;
            }
            catch (Exception e)
            {
                CLog.Error(e.ToString());
                return null;
            }
            finally
            {
            }
        }
        public static WorkBook ReadWorkbook(string path)
        {
            try
            {
                var book = new WorkBook(path);
                return book;
            }
            catch (Exception e)
            {
                CLog.Error(e.ToString());
                return null;
            }
            finally
            {
            }
        }
        #endregion

        #region loader
        public IEnumerator Load()
        {
            Callback_OnParseStart?.Invoke();

            //加载DLC Excel
            foreach (var dlc in DLCManager.LoadedDLCItems.Values)
            {
                if (BuildConfig.Ins.IsEditorOrConfigMode)
                {
                    string[] fileList = dlc.GetAllExcel();
                    foreach (var item in fileList)
                    {
                        LoadExcelData(File.ReadAllBytes(item), Path.GetFileNameWithoutExtension(item));
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    var assetBundle = dlc.LoadRawBundle(SysConst.BN_Excel);
                    if (assetBundle != null)
                    {
                        foreach (var txt in assetBundle.LoadAllAssets<TextAsset>())
                        {
                            LoadExcelData(txt.bytes, txt.name);
                            yield return new WaitForEndOfFrame();
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
            return "Load Excel";
        }

        private void LoadExcelData(byte[] buffer,string tableName)
        {
            var luaMgr = BaseLuaMgr.GetTDConfig(tableName);
            if (luaMgr == null)
                return;
            stopwatch.Start();
            WorkBook book = new WorkBook(buffer);
            if (book == null || book.Count<=0)
            {
                CLog.Error("错误！无法读取Excel："+ tableName);
            }
            foreach (var item in book)
            {
                if(!item.Name.StartsWith(SysConst.Prefix_Lang_Notes))
                    OnConvert(item, tableName);
            }
            stopwatch.Stop();
            CLog.Info($"加载数据表：{tableName},Time：{stopwatch.Elapsed.TotalSeconds}");
        }
        protected virtual void OnConvert(WorkSheet sheet,string tableName)
        {
            int excludeRow = StartRowCount;
            var luaMgr = BaseLuaMgr.GetTDConfig(tableName);
            if (luaMgr!=null)
            {
                IEnumerable<object> data;
                if (luaMgr.TableMapper != null)
                {
                    var map = luaMgr.TableMapper.Exclude(excludeRow);
                    map.SafeMode = true;
                    data = sheet.Convert(map);
                }
                else
                {
                    data = sheet.Convert(luaMgr.DataType, excludeRow, true) ;
                }
                luaMgr.AddAlterRangeFromObj(data);
            }
        }
        #endregion
    }
}