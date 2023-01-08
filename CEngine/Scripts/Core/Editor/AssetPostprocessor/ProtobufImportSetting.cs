//#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
namespace CYM
{
    public class ProtobufImportSetting : AssetPostprocessor
    {

        public static readonly string prefProtocEnable = "ProtobufUnity_Enable";
        public static readonly string prefProtocExecutable = "ProtobufUnity_ProtocExecutable";
        public static readonly string prefLogError = "ProtobufUnity_LogError";
        public static readonly string prefLogStandard = "ProtobufUnity_LogStandard";

        public static bool enabled
        {
            get
            {
                return EditorPrefs.GetBool(prefProtocEnable, true);
            }
            set
            {
                EditorPrefs.SetBool(prefProtocEnable, value);
            }
        }
        public static bool logError
        {
            get
            {
                return EditorPrefs.GetBool(prefLogError, true);
            }
            set
            {
                EditorPrefs.SetBool(prefLogError, value);
            }
        }

        public static bool logStandard
        {
            get
            {
                return EditorPrefs.GetBool(prefLogStandard, false);
            }
            set
            {
                EditorPrefs.SetBool(prefLogStandard, value);
            }
        }

        public static string excPath
        {
            get
            {
                return EditorPrefs.GetString(prefProtocExecutable, "");
            }
            set
            {
                EditorPrefs.SetString(prefProtocExecutable, value);
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (enabled == false)
            {
                return;
            }

            foreach (string str in importedAssets)
            {
                CompileProtobufAssetPath(str);
            }
            AssetDatabase.Refresh();
        }

        private static void CompileAllInProject()
        {
            if (logStandard)
            {
                UnityEngine.Debug.Log("Protobuf Unity : Compiling all .proto files in the project...");
            }
            string[] protoFiles = Directory.GetFiles(Application.dataPath, "*.proto", SearchOption.AllDirectories);
            foreach (string s in protoFiles)
            {
                if (logStandard)
                {
                    UnityEngine.Debug.Log("Protobuf Unity : Compiling " + s);
                }
                CompileProtobufSystemPath(s);
            }
            AssetDatabase.Refresh();
        }

        private static void CompileProtobufAssetPath(string assetPath)
        {
            string systemPath = Directory.GetParent(Application.dataPath) + Path.DirectorySeparatorChar.ToString() + assetPath;
            CompileProtobufSystemPath(systemPath);
        }

        private static void CompileProtobufSystemPath(string systemPath)
        {
            if (Path.GetExtension(systemPath) == ".proto")
            {
                string outputPath = Path.GetDirectoryName(systemPath);
                const string options = " --csharp_out {0} --proto_path {1}";
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "C:\\Windows\\System32\\protoc.exe", Arguments = string.Format("\"{0}\"", systemPath) + string.Format(options, outputPath, outputPath) };
                Process proc = new Process() { StartInfo = startInfo };
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (logStandard)
                {
                    if (output != "")
                    {
                        UnityEngine.Debug.Log("Protobuf Unity : " + output);
                    }
                    UnityEngine.Debug.Log("Protobuf Unity : Compiled " + Path.GetFileName(systemPath));
                }
                if (logError && error != "")
                {
                    UnityEngine.Debug.LogError("Protobuf Unity : " + error);
                }
            }
        }
    }
//#endif
}