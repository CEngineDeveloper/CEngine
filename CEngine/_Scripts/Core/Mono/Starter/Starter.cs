using CYM.DLC;
using HybridCLR;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    [HideMonoScript]
    public class Starter : MonoBehaviour
    {
        #region assembly
        public static Assembly Assembly { get; private set; }
        public static Assembly AssemblyFirstpass { get; private set; }
        #endregion

        #region prop
        public static Starter Ins { get; private set; }
        StarterUI StarterUI;
        #endregion

        #region life
        async void Awake()
        {
            Ins = this;
            StarterUI = Util.CreateGlobalResourceObj<StarterUI>("BaseStarterPlayer");
            DontDestroyOnLoad(this);
            SetupComponet<ErrorCatcher>();
            SetupComponet<DLCDownloader>();
            SetupComponet<DLCManager>();
            //������Դ
            await DLCDownloader.StartDownload();
            //�����ȸ�����
            LoadHotFixScript();
            //��ʼ��Console
            SysConsole.Initialize();
        }
        #endregion

#if UNITY_EDITOR
        private void Reset()
        {
            EnsureComponet();
        }
        private void OnValidate()
        {
            EnsureComponet();
        }
        [Button]
        void EnsureComponet()
        {
            if (Application.isPlaying)
                return;
            transform.hideFlags = HideFlags.NotEditable;
            transform.position = SysConst.VEC_GlobalPos;
            AddScript("Console");
            SetupComponet<Logview>();
            SetupComponet<FPSCounter>();
        }
#endif

        #region set
        void SetupComponet<T>() where T : MonoBehaviour
        {
            if (gameObject.GetComponent<T>() == null)
                gameObject.AddComponent<T>();
        }
        void AddScript(string name)
        {
            if (Assembly == null)
            {
                Assembly = Assembly.Load(SysConst.STR_Assembly);
            }
            name = BuildConfig.Ins.NameSpace + "." + name;
            Type type = Assembly.GetType(name);
            if (type == null)
            {
                Debug.LogError($"�ش���󣡣�{name} ������û�ж���");
                return;
            }
            if (gameObject.GetComponent(type) == null)
                gameObject.AddComponent(type);
        }
        void LoadHotFixScript()
        {
            //�������
            if (!Application.isEditor)
            {
                ///�����ȸ�����
                AssetBundle hotUpdateDllAb = DLCDownloader.GetAssetBundle("hotupdatedlls");
                TextAsset dllBytes = hotUpdateDllAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes");
                Assembly = Assembly.Load(dllBytes.bytes);
                /// ���Լ�������aot assembly�Ķ�Ӧ��dll����Ҫ��dll������unity build���������ɵĲü����dllһ�£�������ֱ��ʹ��ԭʼdll��
                /// ������BuildProcessors������˴�����룬��Щ�ü����dll�ڴ��ʱ�Զ������Ƶ� {��ĿĿ¼}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} Ŀ¼��
                /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
                /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش��� 
                AssetBundle dllAB = DLCDownloader.GetAssetBundle("aotdlls");
                foreach (var aotDllName in dllAB.GetAllAssetNames())
                {
                    byte[] aotdllBytes = dllAB.LoadAsset<TextAsset>(aotDllName).bytes;
                    /// ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
                    LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(aotdllBytes);
                    Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
                }
            }
            //�༭������
            else
            {
                Assembly = Assembly.Load(SysConst.STR_Assembly);
            }
            AssemblyFirstpass = Assembly.Load(SysConst.STR_AssemblyFirstpass);
            AddScript("Global");
            AddScript("Console");
            Debug.Log("�ȸ���ʼ�����");
        }
        #endregion
    }
}