//------------------------------------------------------------------------------
// TextureImportSetting .cs
// Copyright 2018 2018/3/3 
// Created by CYM on 2018/3/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace CYM
{
    public class TextureImportSetting : AssetPostprocessor
    {
        ImportConfig Config => ImportConfig.Ins;

        /// <summary>
        /// 图片导入之前调用，可设置图片的格式、Tag……
        /// </summary>
        void OnPreprocessTexture()
        {
            //特殊路径跳过
            if (!assetImporter.assetPath.Contains(SysConst.Dir_Bundles) &&
                !assetImporter.assetPath.Contains(SysConst.Dir_Res))
            {
                if (assetImporter.assetPath.Contains("Plugins"))
                    return;
                if (assetImporter.assetPath.Contains("Resources"))
                    return;
                if (assetImporter.assetPath.Contains("_NC"))
                    return;
            }
            //自定义配置处理
            TextureImporter importer = (TextureImporter)assetImporter;
            bool isProcessed = false;
            if(!isProcessed) isProcessed = Check(Config.UI);
            if (!isProcessed) isProcessed = Check(Config.Sprite);
            if (!isProcessed) isProcessed = Check(Config.Icon);
            if (!isProcessed) isProcessed = Check(Config.Head);
            if (!isProcessed) isProcessed = Check(Config.Illustration);
            if (!isProcessed) isProcessed = Check(Config.BG);
            if (!isProcessed) isProcessed = Check(Config.ResUI);

            foreach (var item in Config.Textures)
            {
                if (!isProcessed) isProcessed = Check(item);
            }

            //默认处理
            if (!isProcessed)
            {
                importer.crunchedCompression = true;
                importer.compressionQuality = 100;
            }

            bool Check(TextureImportSettings item)
            {
                var path = assetImporter.assetPath;
                if (item.IsContainInDirectoryTag(path))
                {
                    importer.crunchedCompression = item.CrunchedCompression;
                    importer.filterMode = item.FilterMode;
                    importer.textureCompression = item.TextureCompression;
                    importer.compressionQuality = item.CompressionQuality;
                    importer.isReadable = false;
                    importer.spritePackingTag = item.PackingTag;
                    importer.textureType = item.TextureImporterType;
                    isProcessed = true;
                }
                return isProcessed;
            }
        }

        /// <summary>
        /// 图片已经被压缩、保存到指定目录下之后调用
        /// </summary>
        /// <param name="texture"></param>
        void OnPostprocessTexure(Texture2D texture)
        {

        }

        /// <summary>
        /// 所有资源被导入、删除、移动完成之后调用
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {

            }
            foreach (string str in deletedAssets)
            {

            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
    
            }
        }
    }
}