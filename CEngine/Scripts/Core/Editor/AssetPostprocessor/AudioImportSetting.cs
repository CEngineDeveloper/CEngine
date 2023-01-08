//------------------------------------------------------------------------------
// AudioImportSetting.cs
// Copyright 2018 2018/3/23 
// Created by CYM on 2018/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEditor;

namespace CYM
{
    public class AudioImportSetting : AssetPostprocessor
    {
        ImportConfig Config => ImportConfig.Ins;

        void OnPreprocessAudio()
        {
            //特殊路径跳过
            if (assetImporter.assetPath.Contains("Plugins"))
                return;

            //自定义配置处理
            AudioImporter importer = (AudioImporter)assetImporter;
            bool isProcessed = false;
            if (!isProcessed) isProcessed = Check(Config.Audio);
            if (!isProcessed) isProcessed = Check(Config.Music);

            foreach (var item in Config.Audios)
            {
                if (!isProcessed) isProcessed = Check(item);
            }

            //默认处理
            if (!isProcessed)
            {
            }

            bool Check(AudioImportSettings item)
            {
                var path = assetImporter.assetPath;
                if (item.IsContainInDirectoryTag(path))
                {
                    importer.defaultSampleSettings = new AudioImporterSampleSettings
                    {
                        loadType = item.LoadType,
                        compressionFormat = item.CompressionFormat,
                        quality = item.Quality,

                    };
                    isProcessed = true;
                }

                return isProcessed;
            }
        }
    }
}