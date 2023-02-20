using CYM.DLC;
using HybridCLR;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    [ExecuteInEditMode]
    [HideMonoScript]
    public class Starter : MonoBehaviour
    {
        #region assembly
        public static Assembly Assembly { get; private set; }
        public static Assembly AssemblyFirstpass { get; private set; }
        #endregion

        #region prop
        public static Starter Ins { get; private set; }
        public static UnityEngine.Object ConsoleObj { get; private set; }
        public static UnityEngine.Object GlobalObj { get; private set; }
        static StarterUI StarterUI = null;
        #endregion

        #region Inspector
        [SerializeField]
        [ValueDropdown("GetGlobalTypeList")]
        [DisableInPlayMode]
        string GlobalType;
        #endregion

        #region life
        async void Awake()
        {
            Ins = this;
            if (Application.isPlaying)
            {
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
        [DisableInPlayMode]
        void EnsureComponet()
        {
            if (Application.isPlaying)
                return;
            transform.hideFlags = HideFlags.NotEditable;
            transform.position = SysConst.VEC_GlobalPos;
            ConsoleObj = AddScript("Console");
            //SetupComponet<Logview>();
            SetupComponet<FPSCounter>();
        }
#endif

        #region set
        void SetupComponet<T>() where T : MonoBehaviour
        {
            if (gameObject.GetComponent<T>() == null)
                gameObject.AddComponent<T>();
        }
        UnityEngine.Object AddScript(string name)
        {
            if (name.IsInv())
            {
                CLog.Error("����,AddScript,name���Ϸ�");
                return null;
            }
            string fullName = "none";
            UnityEngine.Object ret = null;
            EnsureAssembly();
            //�����߼��㻷��
            fullName = BuildConfig.Ins.NameSpace + "." + name;
            Type type = Assembly.GetType(fullName);
            //���ؽ̳̻���
            if (type == null)
            {
                fullName = nameof(CYM)+"."+ nameof(Example) + "." + name;
                type = AssemblyFirstpass.GetType(fullName);
            }
            //������
            if (type == null)
            {
                Debug.LogError($"�ش���󣡣�{name} ������û�ж���");
                return null;
            }
            ret = gameObject.GetComponent(type);
            if (ret == null)
                ret = gameObject.AddComponent(type);
            return ret;
        }
        void LoadHotFixScript()
        {
            //�������
            if (!Application.isEditor &&
                BuildConfig.Ins.IsHotFix)
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
            EnsureAssembly();
            GlobalObj = AddScript(GlobalType);
            ConsoleObj = AddScript("Console");
            Debug.Log("�ȸ���ʼ�����");
        }
        void EnsureAssembly()
        {
            if (Assembly == null)
            {
                Assembly = Assembly.Load(SysConst.STR_Assembly);
            }
            if (AssemblyFirstpass == null)
            {
                AssemblyFirstpass = Assembly.Load(SysConst.STR_AssemblyFirstpass);
            }
        }
        #endregion

        #region get
        string[] GetGlobalTypeList()
        {
            EnsureAssembly();
            var ret1 = Assembly.GetTypes()
                .Where(x=>!x.IsGenericTypeDefinition)
                .Where(x=>x.IsSubclassOf(typeof(BaseGlobal)));
            var ret2 = AssemblyFirstpass.GetTypes()
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => x.IsSubclassOf(typeof(BaseGlobal)));
            ret1 = ret1.Concat(ret2);
            return ret1.Select(x=>x.Name).ToArray();
        }
        #endregion
    }
}