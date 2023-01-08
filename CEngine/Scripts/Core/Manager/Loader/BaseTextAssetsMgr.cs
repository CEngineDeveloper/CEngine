
using CYM.DLC;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM
{
    public class BaseTextAssetsMgr : BaseGFlowMgr, ILoader
    {
        public Dictionary<string, string> Data { get; protected set; } = new Dictionary<string, string>();
        public event Callback Callback_OnParseStart;
        public event Callback Callback_OnParseEnd;

        #region loader
        public IEnumerator Load()
        {
            Callback_OnParseStart?.Invoke();
            Data.Clear();
            foreach (var dlc in DLCManager.LoadedDLCItems.Values)
            {
                if (BuildConfig.Ins.IsEditorOrConfigMode)
                {
                    foreach (var file in dlc.GetAllTexts())
                    {
                        string fileName = Path.GetFileName(file);
                        Data.Add(fileName, File.ReadAllText(file));
                        BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load Text " + fileName;
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    var assetBundle = dlc.LoadRawBundle(SysConst.BN_Text);
                    if (assetBundle != null)
                    {
                        foreach (var textAssets in assetBundle.LoadAllAssets<TextAsset>())
                        {
                            Data.Add(textAssets.name, File.ReadAllText(textAssets.text));
                            BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load Text " + textAssets.name;
                        }
                    }
                }

            }
            Callback_OnParseEnd?.Invoke();
            yield break;
        }
        public string GetLoadInfo()
        {
            return "Load TextAssets";
        }
        #endregion

        #region get
        /// <summary>
        /// 获得text
        /// </summary>
        /// <returns></returns>
        public string GetText(string id)
        {
            if (!Data.ContainsKey(id))
                return null;
            return Data[id];
        }
        #endregion
    }

}