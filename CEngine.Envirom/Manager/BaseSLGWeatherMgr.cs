//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2019 2019/9/4 
// Created by CYM on 2019/9/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Envirom
{
    [System.Serializable]
    [HideReferenceObjectPicker]
    public class SLGWeatherConfigData
    {
        public float Prob;
        public GameObject Prefab;

        public SLGWeatherConfigData()
        {        
        }
    }
    public class BaseSLGWeatherMgr : BaseGFlowMgr
    {
        class SLGWeatherData
        {
            public SLGWeatherData(int cd, GameObject perform)
            {
                CD = new CD(cd);
                Perform = perform;
            }
            public CD CD;
            public GameObject Perform;
        }
        #region prop
        protected virtual int StartCount => GameConfig.Ins.StartCount;
        protected virtual int TotalCount => GameConfig.Ins.TotalCount;
        protected virtual int CellCount => GameConfig.Ins.CellCount;
        protected virtual int Squar => GameConfig.Ins.Squar;
        int RealCount => CellCount - 1;
        Vector2[,] PosIndex;
        protected List<SLGWeatherConfigData> ConfigData => GameConfig.Ins.SLGWeather;
        Dictionary<string, SLGWeatherData> Data { get; set; } = new Dictionary<string, SLGWeatherData>();
        Timer UpdateTimer = new Timer(1.0f);
        Timer SpawnTimer = new Timer(10.0f);
        List<string> clearData = new List<string>();
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            Data.Clear();
            int step = Squar / CellCount;
            PosIndex = new Vector2[RealCount, RealCount];
            for (int i = 0; i < RealCount; ++i)
            {
                for (int j = 0; j < RealCount; ++j)
                {
                    PosIndex[i, j] = new Vector2((i + 1) * step, (j + 1) * step);
                }
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (BattleMgr.IsGameStartOver)
            {
                if (UpdateTimer.CheckOver())
                {
                    clearData.Clear();
                    foreach (var item in Data)
                    {
                        item.Value.CD.Update();
                        if (item.Value.CD.IsOver())
                        {
                            clearData.Add(item.Key);
                        }
                    }
                    foreach (var item in clearData)
                    {
                        Despwn(item);
                    }
                }
                if (SpawnTimer.CheckOver())
                {
                    RandWeather(0.5f);
                }
            }
        }
        #endregion

        #region set
        public void Spawn(Vector2 index)
        {
            if (Data.ContainsKey(index.ToString())) return;
            if (index.x >= RealCount || index.y >= RealCount) return;
            Vector2 pos = PosIndex[(int)index.x, (int)index.y];
            Vector3 realPos = new Vector3(pos.x, TerrainObj.Ins.SampleHeight(pos), pos.y);
            realPos.x += RandUtil.RandFloat(-50, 50);
            realPos.z += RandUtil.RandFloat(-50, 50);
            var weatherData = ConfigData.Rand();
            if (weatherData != null)
            {
                if (RandUtil.Rand(weatherData.Prob))
                {
                    var perform = BaseGlobal.PoolPerform.Spawn(weatherData.Prefab, realPos, Quaternion.identity);
                    Util.RaycastY(perform.transform, 0, SysConst.Layer_Terrain);
                    Data.Add(index.ToString(), new SLGWeatherData(RandUtil.RandInt(50, 400), perform));
                }
            }
        }
        public void Despwn(string key, bool isRemove = true)
        {
            if (!Data.ContainsKey(key)) return;
            BaseGlobal.PoolPerform.Despawn(Data[key].Perform);
            if (isRemove)
                Data.Remove(key);
        }
        void RandWeather(float prop = 0.2f)
        {
            if (Data.Count <= TotalCount)
            {
                if (RandUtil.Rand(prop))
                {
                    Spawn(new Vector2(RandUtil.RandInt(0, RealCount), RandUtil.RandInt(0, RealCount)));
                }
            }
        }
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            for (int i = 0; i < StartCount; ++i)
            {
                RandWeather(1.0f);
            }
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            foreach (var item in Data)
            {
                Despwn(item.Key, false);
            }
            Data.Clear();
        }
        #endregion
    }
}