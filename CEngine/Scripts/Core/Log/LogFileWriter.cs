//------------------------------------------------------------------------------
// LogMono.cs
// Copyright 2022 2022/11/12 
// Created by CYM on 2022/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using System;
using System.IO;

namespace CYM
{
    public class LogFileWriter : MonoBehaviour
    {
        static FileStream fs;
        static StreamWriter sw;
        private void Awake()
        {
            //定义日志环境，这里是D盘创建一个GLRunlog（年月）文件夹
            string sFilePath = SysConst.Path_Log;//"d:\\" + "GLRunLog" + DateTime.Now.ToString("yyyyMM");
            //定义日志的名字，这里定义的日志名字为SQLLOG+日.log
            string sFileName = "LOG" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            //文件的绝对路径
            sFileName = sFilePath + "\\" + sFileName;
            //验证路径是否存在
            if (!Directory.Exists(sFilePath))
            {
                Directory.CreateDirectory(sFilePath);
            }

            if (File.Exists(sFileName))
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);
                sw.WriteLine($"BuildTime:{Version.BuildTime}");
                sw.WriteLine($"{Version.FullGameVersion}");
                sw.WriteLine($"{Util.AdvSystemInfo}");
                sw.WriteLine($"----------------------------------------------------------------");
            }
        }

        private void OnDestroy()
        {
            sw.Close();
            fs.Close();
        }

        public static void WriteLog(string strLog)
        {
            sw.WriteLine($"[{DateTime.Now.ToString("HH-mm-ss")}]{strLog}");
        }
    }
}