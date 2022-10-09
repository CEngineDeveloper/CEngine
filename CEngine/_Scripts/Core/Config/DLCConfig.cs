//------------------------------------------------------------------------------
// AssetBundleConfig.cs
// Copyright 2018 2018/5/18 
// Created by CYM on 2018/5/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using CYM.DLC;

namespace CYM
{
    public class DLCItemConfig
    {
        public string Name;
        public bool IsActive = true;
        public int Version = 0;
        public DLCItemConfig(string name)
        {
            Name = name;
        }
    }
    public sealed class DLCConfig : ScriptableObjectConfig<DLCConfig>
    {
        #region private inspector
        [SerializeField,ReadOnly, HideInInspector] 
        List<BuildRuleConfig> InnerBuildRule = new List<BuildRuleConfig>();
        [SerializeField, ReadOnly, HideInInspector]
        List<string> IgnoreConst = new List<string>();
        #endregion

        #region inspector
        //图片资源
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType BG = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Icon = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Head = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Texture = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Illustration = BuildRuleType.Directroy;
        //其他资源
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Audio = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Material = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Music = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Video = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Animator = BuildRuleType.Directroy;
        //Prefab资源
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Prefab = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType Perform = BuildRuleType.Directroy;
        [SerializeField, FoldoutGroup("Inner Rule")] BuildRuleType UI = BuildRuleType.Directroy;
        //其他
        [SerializeField, FoldoutGroup("Other")] List<BuildRuleConfig> BuildRule = new List<BuildRuleConfig>();
        [SerializeField, FoldoutGroup("Other")] List<DLCItemConfig> DLC = new List<DLCItemConfig>();
        #endregion

        #region dlc config
        //内部DLC
        public DLCItemConfig ConfigInternal { get; private set; }
        //默认DLC
        public DLCItemConfig ConfigNative { get; private set; }
        //扩展DLC 不包含 Native
        public List<DLCItemConfig> ConfigExtend { get; private set; } = new List<DLCItemConfig>();
        public List<DLCItemConfig> ConfigAll { get; private set; } = new List<DLCItemConfig>();
        #endregion

        #region editor dlc item
        public DLCItem EditorInternal { get; private set; }
        public DLCItem EditorNative { get; private set; }
        public List<DLCItem> EditorExtend { get; private set; } = new List<DLCItem>();
        public List<DLCItem> EditorAll { get; private set; } = new List<DLCItem>();
        public List<DLCItem> EditorInner { get; private set; } = new List<DLCItem>();
        #endregion

        #region runtime
        public HashSet<string> RuntimeAllDirectory { get; private set; } = new HashSet<string>();
        public List<BuildRuleConfig> RuntimeConfig { get; private set; } = new List<BuildRuleConfig>();
        public List<string> RuntimeCopyDirectory { get; private set; } = new List<string>();
        public HashSet<string> RuntimeIgnoreConstSet { get; private set; } = new HashSet<string>();
        public HashSet<string> RuntimeHashBuildRuleConfig { get; private set; } = new HashSet<string>();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            RecreateDLC();
        }
        public override void OnInited()
        {
            base.OnInited();
            RefreshDLC();
        }
        [Button(nameof(RecreateDLC))]
        public void RecreateDLC()
        {
            InnerBuildRule.Clear();
            RuntimeCopyDirectory.Clear();
            RuntimeHashBuildRuleConfig.Clear();
            IgnoreConst.Clear();
            //配置资源
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig("Config", BuildRuleType.Directroy, true));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig("Lua", BuildRuleType.Directroy, true));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig("Language", BuildRuleType.Directroy, true));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig("Excel", BuildRuleType.Directroy, true));
            //图片资源
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(BG), BG));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Icon), Icon));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Head), Head));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Texture), Texture));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Illustration), Illustration));
            //其他资源
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Audio), Audio));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Material), Material));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Music), Music));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Video), Video));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Animator), Animator));
            //Prefab资源
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Prefab), Prefab));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(Perform), Perform));
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig(nameof(UI), UI));
            //场景资源
            AddBuildConfig(InnerBuildRule, new BuildRuleConfig("Scene", BuildRuleType.File));
            //添加自定义规则
            foreach (var item in BuildRule)
            {
                AddBuildConfig(InnerBuildRule,item.Clone() as BuildRuleConfig);
            }
            //忽略的Const
            IgnoreConst.Add("CONFIG_DLCItem");
            IgnoreConst.Add("CONFIG_DLCManifest");
        }
        //刷新DLC
        [Button(nameof(RefreshDLC))]
        public void RefreshDLC()
        {
            RuntimeConfig.Clear();
            RuntimeAllDirectory.Clear();
            RuntimeCopyDirectory.Clear();
            RuntimeIgnoreConstSet.Clear();
            RuntimeHashBuildRuleConfig.Clear();

            ConfigExtend.Clear();
            ConfigAll.Clear();

            EditorExtend.Clear();
            EditorAll.Clear();
            EditorInner.Clear();

            foreach (var item in InnerBuildRule)
                AddBuildConfig(RuntimeConfig,item);
            foreach (var item in IgnoreConst)
                AddIgnoreConst(item);

            //加载内部dlc
            ConfigInternal = new DLCItemConfig(SysConst.STR_InternalDLC);
            EditorInternal = new DLCItem(ConfigInternal);

            //加载原生dlc
            ConfigNative = new DLCItemConfig(SysConst.STR_NativeDLC);
            EditorNative = new DLCItem(ConfigNative);

            //加载其他额外DLC
            foreach (var item in DLC)
            {
                var dlcItem = new DLCItemConfig(item.Name);
                ConfigExtend.Add(dlcItem);
                EditorExtend.Add(new DLCItem(dlcItem));
            }

            ConfigAll = new List<DLCItemConfig>(ConfigExtend);
            ConfigAll.Add(ConfigInternal);
            ConfigAll.Add(ConfigNative);

            EditorAll = new List<DLCItem>(EditorExtend);
            EditorAll.Add(EditorInternal);
            EditorAll.Add(EditorNative);

            EditorInner.Add(EditorInternal);
            EditorInner.Add(EditorNative);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                foreach (var item in RuntimeAllDirectory)
                {
                    FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Bundles, item));
                }

                foreach (var item in DLC)
                {
                    foreach (var dic in RuntimeAllDirectory)
                    {
                        FileUtil.EnsureDirectory(Path.Combine(SysConst.Path_Dlc, item.Name, dic));
                    }
                }
            }
#endif
        }
        void AddBuildConfig(List<BuildRuleConfig> runtimedata,BuildRuleConfig config)
        {
            if (RuntimeHashBuildRuleConfig.Contains(config.Name))
            {
                CLog.Error($"错误！发现重复的打包规则：{config.Name}");
                return;
            }
            RuntimeHashBuildRuleConfig.Add(config.Name);

            BuildRuleConfig data = config.Clone() as BuildRuleConfig;
            runtimedata.Add(data);
            if (data.IsCopyDirectory)
            {
                RuntimeCopyDirectory.Add(data.Name);
            }
            if (!RuntimeAllDirectory.Contains(data.Name))
                RuntimeAllDirectory.Add(data.Name);
        }
        void AddIgnoreConst(string name)
        {
            if (!RuntimeIgnoreConstSet.Contains(name))
                RuntimeIgnoreConstSet.Add(name);
        }
        public void RemoveDLC(string name)
        {
            DLC.RemoveAll(x => x.Name == name);
            RefreshDLC();
        }
        public void AddDLC(string name)
        {
            DLC.Add(new DLCItemConfig(name));
            RefreshDLC();
        }
        #endregion

        #region is
        public bool IsInIgnoreConst(string name)
        {
            return RuntimeIgnoreConstSet.Contains(name);
        }
        #endregion
    }
}