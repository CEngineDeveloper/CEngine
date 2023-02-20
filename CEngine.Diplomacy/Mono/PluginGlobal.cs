//------------------------------------------------------------------------------
// BaseGlobal.cs
// Copyright 2022 2022/9/25 
// Created by CYM on 2022/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using CYM.Diplomacy;
namespace CYM
{
    public partial class BaseGlobal : BaseCoreMono
    {
        static PluginGlobal PluginDiplomacy = new PluginGlobal
        {
            OnInstall = (g) => {

            },

            OnPostAddComponet = (x) => {
                if (x is IRelationMgr) RelationMgr = x as IRelationMgr;
                else if (x is BaseTerrainGridMgr) TerrainGridMgr = x as BaseTerrainGridMgr;
                else if (x is IArticleMgr<TDBaseArticleData>) ArticleMgr = x as IArticleMgr<TDBaseArticleData>;
            }
        };
        public static IRelationMgr RelationMgr { get; protected set; }
        public static BaseTerrainGridMgr TerrainGridMgr { get; protected set; }
        public static IArticleMgr<TDBaseArticleData> ArticleMgr { get; protected set; }
    }
}