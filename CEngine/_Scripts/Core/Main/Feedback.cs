//------------------------------------------------------------------------------
// Feedback.cs
// Copyright 2021 2021/3/21 
// Created by CYM on 2021/3/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace CYM
{
    [HideMonoScript]
    public sealed class Feedback : MonoBehaviour
    {
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static private string SendTitle = "";
        static private string SendDesc = "";
        static private HashSet<string> sendedDesc = new HashSet<string>();
        #region set
        public static void SendError(string desc, string contactInfo, bool isSendAchieve = false)
        {
            string realDesc = getDesc("错误信息", desc, contactInfo,out string error);
            if (error == null || error.Length <= 0)
                return;
            string key = error.Substring(0,Mathf.Clamp(Mathf.Min(40, realDesc.Length-1),0,100));
            if (sendedDesc.Contains(key)) return;
            sendedDesc.Add(key);
            //简体中文地区自动发送邮件
            if (BaseLangMgr.LanguageType == LanguageType.Chinese)
            {
                string filePath = null;
                if (isSendAchieve)
                {
                    if (BaseGlobal.BattleMgr.IsInBattle)
                    {
                        BaseGlobal.DBMgr.SaveTemp();
                        filePath = BaseGlobal.DBMgr.GetTempSavePath();
                    }
                }
                CMail.Send(ErrorCatcher.GetTitle(), realDesc, filePath);
            }
        }
        public static void SendMail(string title, string desc, string contactInfo)
        {
            if (title.IsInv()) return;
            string realDesc = getDesc(title, desc, contactInfo,out string error);
            CMail.Send(title, realDesc, null);
        }
        #endregion

        #region get
        static string getDesc(string title, string desc, string contactInfo,out string error)
        {
            error = ErrorCatcher.GetErrorString();
            SendTitle = title;
            SendDesc = desc;
            string realDesc = string.Format(
                $"{MarkdownUtil.H2("Version")}:\n{BuildConfig?.FullVersion??"无版本"}\n" +
                $"{MarkdownUtil.H2("Contact")}:\n{contactInfo}\n" +
                $"{MarkdownUtil.H2("GMMode")}:\n{BaseGlobal.DiffMgr?.IsSettedGMMod()??false}\n" +
                $"{MarkdownUtil.H2("Name")}:\n{BaseGlobal.PlatSDKMgr?.GetName()??"无名氏"}\n" +
                $"{MarkdownUtil.H2("Desc")}:\n{SendDesc}\n" +
                $"{MarkdownUtil.H2("Error")}:\n{error}\n" +
                $"{MarkdownUtil.H2("SystemInfo")}:\n{Util.AdvSystemInfo}"
                );
            return realDesc;
        }
        #endregion
    }
}