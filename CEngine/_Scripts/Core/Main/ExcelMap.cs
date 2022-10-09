//------------------------------------------------------------------------------
// ExcelMap.cs
// Copyright 2021 2021/1/25 
// Created by CYM on 2021/1/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using CYM.Excel;
using System;

/************************************************************就像这样***************************************************************************************
轴	0	        1	        2	        3	        4	        5	        6	        7	        8	        9	        10	        11	        12	       
5	000|201|1	101|300|3	101|000|0	101|000|0	101|000|0	101|341|1	101|000|0	000|206|3				000|220|1	000|220|2																	
4	000|202|1	101|300|3	101|321|0	101|000|0	101|301|0	101|361|0	101|000|0	000|206|3	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
3	000|201|1	101|000|0	101|000|0	101|000|0	101|000|0	101|000|0	101|000|0	000|206|3	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
2	000|202|1	101|000|0	101|000|0	101|441|0	101|441|2	101|000|0	101|000|0	000|206|3	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
1	000|201|1	101|000|0	101|000|0	101|000|0	101|000|0	101|000|0	101|000|0	000|206|3	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
0	000|203|1	000|206|0	000|206|0	000|000|0	000|209|0	000|206|0	000|206|0	000|207|2	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
-1																														
-2																														
-3				000|210|2																										
-4																														
-5	000|215|1	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|215|1	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
-6	000|212|1	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|212|3	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
-7	000|213|1	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|213|3	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
-8	000|214|1	000|212|0	000|213|0	000|212|0	000|000|0	000|211|2	000|000|0	000|214|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
-9	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0	000|000|0																	
-10	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
-11	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
-12	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
-13	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
-14	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0																	
-15	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0	000|223|0	000|224|0																	
************************************************************************************************************************************************************/

namespace CYM
{
    public class BaseMapAreaConfig
    {
        public int X;
        public int Z;
    }
    //excel坐标图
    public sealed class ExcelMap<T> where T : BaseMapAreaConfig, new()
    {
        #region prop
        const string KeyZAxie = "Z轴";
        const char ParamSplit = '|';
        Func<string[], bool> IsValid;
        Callback<string[], T> Convert;
        char Split = '|';
        #endregion

        //地图配置信息
        public Dictionary<string, List<List<T>>> Datas { get; private set; } = new Dictionary<string, List<List<T>>>();
        public Dictionary<string, Dictionary<int, T>> MapDatas { get; private set; } = new Dictionary<string, Dictionary<int, T>>();
        public Dictionary<string, Dictionary<string, List<float>>> Params { get; private set; } = new Dictionary<string, Dictionary<string, List<float>>>();
        readonly List<float> EmptyList = new List<float>();

        #region life
        public ExcelMap(char split, Func<string[], bool> isValid, Callback<string[], T> convert)
        {
            Split = split;
            IsValid = isValid;
            Convert = convert;
        }
        #endregion

        #region is
        public bool IsHave(string id)
        {
            if (!Datas.ContainsKey(id))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region get
        public T Get(string id, int mapKey)
        {
            if (!MapDatas.ContainsKey(id))
            {
                return default;
            }
            if (!MapDatas[id].ContainsKey(mapKey))
                return default;
            return MapDatas[id][mapKey];
        }
        public T Get(string id, float x, float z)
        {
            var key = GetMapKey(x, z);
            if (!MapDatas.ContainsKey(id))
            {
                return default;
            }
            if (!MapDatas[id].ContainsKey(key))
                return default;
            return MapDatas[id][key];
        }

        //获得地图信息
        public List<List<T>> GetList(string id)
        {
            if (!Datas.ContainsKey(id))
            {
                return default;
            }
            return Datas[id];
        }
        //获得地图全局参数
        public List<float> GetParams(string mapId, string id)
        {
            if (!Params.ContainsKey(mapId))
                return EmptyList;
            var param = Params[mapId];
            if (!param.ContainsKey(id))
                return EmptyList;
            return param[id];
        }
        public int GetMapKey(float x, float z)
        {
            return (Mathf.RoundToInt(x) * 100) + Mathf.RoundToInt(z);
        }
        #endregion

        #region read
        public void Read(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            Datas.Clear();
            MapDatas.Clear();
            Params.Clear();
            WorkBook workBook = BaseExcelMgr.ReadWorkbook(buffer);
            if (workBook != null && workBook.Count > 0)
            {
                foreach (var sheet in workBook)
                {
                    try
                    {
                        if (sheet.Name.StartsWith(SysConst.Prefix_Lang_Notes))
                            continue;

                        string mapKey = sheet.Name;
                        int paredRowCount = 0;
                        int paredColCount = 0;
                        int realRowIndex = 0;
                        int realColIndex = 0;
                        int zAxieRow = 0;
                        bool isHaveZAxie = false;

                        //添加Map参数列表
                        if (!Params.ContainsKey(mapKey))
                            Params.Add(mapKey, new Dictionary<string, List<float>>());

                        var sheetMap = new List<List<T>>();
                        var dicMap = new Dictionary<int, T>();
                        foreach (var row in sheet)
                        {
                            realRowIndex++;
                            if (row.Count > 0)
                            {
                                var key = row[0].Text;
                                if (!isHaveZAxie)
                                {
                                    if (key == KeyZAxie)
                                    {
                                        zAxieRow = realRowIndex - 1;
                                        isHaveZAxie = true;
                                    }
                                    else if (row.Count > 1)
                                    {
                                        var val = row[1].Text.Split(ParamSplit);
                                        List<float> value = new List<float>();
                                        foreach (var item in val)
                                        {
                                            value.Add(float.Parse(item));
                                        }
                                        Params[mapKey].Add(key, value);
                                    }
                                    continue;
                                }
                            }

                            paredColCount = 0;
                            realColIndex = 0;

                            //读取Map
                            List<T> mapRow = new List<T>();
                            foreach (var col in row)
                            {
                                realColIndex++;
                                if (paredColCount == 0)
                                {
                                    paredColCount++;
                                    continue;
                                }
                                if (col.IsString && !col.String.IsInv())
                                {
                                    string[] vals = col.String.Split(Split);
                                    if (!IsValid(vals))
                                    {
                                        CLog.Error("策划配置格式错误，地图文件");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (sheet.Rows.Count <= 0)
                                                continue;
                                            T data = new T();
                                            Convert(vals, data);
                                            data.X = sheet.Rows[zAxieRow].Cells[paredColCount];  //横向
                                            data.Z = sheet.Rows[zAxieRow + paredRowCount + 1].Cells[0];  //竖向
                                            mapRow.Add(data);
                                            dicMap.Add(GetMapKey(data.X, data.Z), data);
                                        }
                                        catch (Exception e)
                                        {
                                            CLog.Error(string.Format($"{e.Message},rowCount:{paredRowCount},colCount:{paredColCount},sheet:{sheet.Name}"));
                                        }
                                    }
                                }
                                else
                                {
                                    mapRow.Add(new T());
                                }
                                paredColCount++;
                            }
                            sheetMap.Add(mapRow);
                            paredRowCount++;
                        }
                        Datas.Add(sheet.Name, sheetMap);
                        MapDatas.Add(sheet.Name, dicMap);
                    }
                    catch (Exception e)
                    {
                        CLog.Error($"{sheet.Name},出错：" + e.ToString());
                    }
                }
            }
        }
        #endregion
    }
}