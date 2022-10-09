using Invoke;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace CYM
{
    public enum ScreenEdgeType
    {
        None,
        Left,
        Right,
        Top,
        Bot,
    }
    public partial class Util
    {
        #region static
        static DateTime DateTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        static TimeSpan TimeSpan = new TimeSpan();
        #endregion

        #region info
        public static string SimpleSystemInfo
        {
            get
            {
                string info =
                       "OS:" + SystemInfo.operatingSystem +
                       "\nProcessor:" + SystemInfo.processorType +
                       "\nMemory:" + SystemInfo.systemMemorySize +
                       "\nGraphics API:" + SystemInfo.graphicsDeviceType +
                       "\nGraphics Processor:" + SystemInfo.graphicsDeviceName +
                       "\nGraphics Memory:" + SystemInfo.graphicsMemorySize +
                       "\nGraphics Vendor:" + SystemInfo.graphicsDeviceVendor;
                return info;
            }
        }
        // 基本系统信息
        public static string BaseSystemInfo
        {
            get
            {
                string systemInfo =
                "DeviceModel：" + SystemInfo.deviceModel +
                "\nDeviceName：" + SystemInfo.deviceName +
                "\nDeviceType：" + SystemInfo.deviceType +
                "\nGraphicsDeviceName：" + SystemInfo.graphicsDeviceName +
                "\nGraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion +
                "\nGraphicsMemorySize（M）：" + SystemInfo.graphicsMemorySize +
                "\nGraphicsShaderLevel：" + SystemInfo.graphicsShaderLevel +
                "\nMaxTextureSize：" + SystemInfo.maxTextureSize +
                "\nOperatingSystem：" + SystemInfo.operatingSystem +
                "\nProcessorCount：" + SystemInfo.processorCount +
                "\nProcessorType：" + SystemInfo.processorType +
                "\nSystemMemorySize：" + SystemInfo.systemMemorySize;

                return systemInfo;
            }
        }
        // 高级系统信息
        public static string AdvSystemInfo
        {
            get
            {
                string systemInfo =
                "DeviceModel：" + SystemInfo.deviceModel +
                "\nDeviceName：" + SystemInfo.deviceName +
                "\nDeviceType：" + SystemInfo.deviceType +
                "\nDeviceUniqueIdentifier：" + SystemInfo.deviceUniqueIdentifier +
                "\nGraphicsDeviceID：" + SystemInfo.graphicsDeviceID +
                "\nGraphicsDeviceName：" + SystemInfo.graphicsDeviceName +
                "\nGraphicsDeviceVendor：" + SystemInfo.graphicsDeviceVendor +
                "\nGraphicsDeviceVendorID:" + SystemInfo.graphicsDeviceVendorID +
                "\nGraphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion +
                "\nGraphicsMemorySize（M）：" + SystemInfo.graphicsMemorySize +
                "\nGraphicsShaderLevel：" + SystemInfo.graphicsShaderLevel +
                "\nMaxTextureSize：" + SystemInfo.maxTextureSize +
                "\nNpotSupport：" + SystemInfo.npotSupport +
                "\nOperatingSystem：" + SystemInfo.operatingSystem +
                "\nProcessorCount：" + SystemInfo.processorCount +
                "\nProcessorType：" + SystemInfo.processorType +
                "\nSupportedRenderTargetCount：" + SystemInfo.supportedRenderTargetCount +
                "\nSupports3DTextures：" + SystemInfo.supports3DTextures +
                "\nSupportsAccelerometer：" + SystemInfo.supportsAccelerometer +
                "\nSupportsComputeShaders：" + SystemInfo.supportsComputeShaders +
                "\nSupportsGyroscope：" + SystemInfo.supportsGyroscope +
                "\nSupportsInstancing：" + SystemInfo.supportsInstancing +
                "\nSupportsLocationService：" + SystemInfo.supportsLocationService +
                "\nSupportsShadows：" + SystemInfo.supportsShadows +
                "\nSupportsSparseTextures：" + SystemInfo.supportsSparseTextures +
                "\nSupportsVibration：" + SystemInfo.supportsVibration +
                "\nSystemMemorySize：" + SystemInfo.systemMemorySize;

                return systemInfo;
            }
        }
        #endregion

        #region get ray cast pos
        /// <summary>
        /// 获得Y轴坐标
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Vector3 GetRaycastY(Transform trans, float yoffset, LayerData layer)
        {
            return GetRaycastY(trans, yoffset, (LayerMask)layer);
        }
        /// <summary>
        /// 获得Y轴坐标
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static Vector3 GetRaycastY(Transform trans, float yoffset, LayerMask mask)
        {
            RaycastHit hitInfo;
            Vector3 opos = trans.position + Vector3.up * 99999.0f;
            Physics.Raycast(new Ray(opos, trans.position - opos), out hitInfo, float.MaxValue, mask);
            return new Vector3(hitInfo.point.x, yoffset + hitInfo.point.y, hitInfo.point.z);
        }
        public static Vector3 GetRaycastY(Vector3 pos, float yoffset, LayerMask mask)
        {
            RaycastHit hitInfo;
            Vector3 opos = pos + Vector3.up * 99999.0f;
            Physics.Raycast(new Ray(opos, pos - opos), out hitInfo, float.MaxValue, mask);
            return new Vector3(hitInfo.point.x, yoffset + hitInfo.point.y, hitInfo.point.z);
        }
        #endregion

        #region Ray cast
        public static GameObject MousePick(int mask)
        {
            if (Camera.main == null)
                return null;
            GameObject ret = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, mask))
            {
                ret = hitInfo.collider.gameObject;
            }
            return ret;
        }
        public static GameObject ScreenPick(Vector2 screenPos, int mask)
        {
            if (Camera.main == null)
                return null;
            GameObject ret = null;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, mask))
            {
                ret = hitInfo.collider.gameObject;
            }
            return ret;
        }
        public static void FollowToMousePos(GameObject go, float fixedHeight = 0.5f)
        {
            if (Camera.main == null)
                return;
            if (go == null)
                return;
            Vector3 dragItemScreenSpace = Camera.main.WorldToScreenPoint(go.transform.position);
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragItemScreenSpace.z);
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace);
            currentPosition = currentPosition.SetY(fixedHeight);
            go.transform.position = currentPosition;
        }
        public static void FollowToScreenPos(GameObject go, Vector2 screenPos, float fixedHeight = 0.5f)
        {
            if (Camera.main == null)
                return;
            if (go == null)
                return;
            Vector3 dragItemScreenSpace = Camera.main.WorldToScreenPoint(go.transform.position);
            Vector3 currentScreenSpace = new Vector3(screenPos.x, screenPos.y, dragItemScreenSpace.z);
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace);
            currentPosition = currentPosition.SetY(fixedHeight);
            go.transform.position = currentPosition;
        }
        /// <summary>
        /// 检测Y轴碰撞体,防止下沉
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="mask"></param>
        public static void RaycastY(Transform trans, float yoffset, LayerData layer)
        {
            trans.position = GetRaycastY(trans, yoffset, layer);
        }
        /// <summary>
        /// 检测Y轴碰撞体,防止下沉
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="layer"></param>
        public static void RaycastY(Transform trans, float yoffset, LayerMask mask)
        {
            trans.position = GetRaycastY(trans, yoffset, mask);
        }
        public static bool RayCast(out RaycastHit hit, Vector2 pos, LayerMask layer)
        {
            hit = new RaycastHit();
            if (Camera.main == null)
                return false;
            return Physics.Raycast(new Ray(new Vector3(pos.x, int.MaxValue * 0.5f, pos.y), -Vector3.up), out hit, int.MaxValue, layer, QueryTriggerInteraction.Collide);
        }
        public static bool ScreenRayCast(out RaycastHit hit, Vector2 screenPos, LayerMask layer)
        {
            hit = new RaycastHit();
            if (Camera.main == null)
                return false;
            return Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out hit, 9999, layer, QueryTriggerInteraction.Collide);
        }
        public static bool ScreenCenterRayCast(out RaycastHit hit, LayerMask layer)
        {
            return ScreenRayCast(out hit, new Vector2(Screen.width / 2, Screen.height / 2), layer);
        }
        public static bool OverlapPoint2D(out Collider2D collider, Vector2 screenPos, LayerMask layer)
        {
            collider = null;
            if (Camera.main == null)
                return false;
            collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(screenPos), layer, -9999, 9999);
            return collider != null;
        }
        public static bool OverlapPointAll2D(out Collider2D[] collider, Vector2 screenPos, LayerMask layer)
        {
            collider = null;
            if (Camera.main == null)
                return false;
            collider = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(screenPos), layer, -9999, 9999);
            return collider != null;
        }
        public static (Collider col, Collider2D col2D) PickCollider(Vector2 screenPos, LayerMask layer)
        {
            if (ScreenRayCast(out RaycastHit hitInfo, screenPos, layer))
            {
                return (hitInfo.collider, null);
            }
            else if (OverlapPoint2D(out Collider2D collider2D, screenPos, layer))
            {
                return (null, collider2D);
            }
            return (null, null);
        }
        public static Component PickColliderCom(Vector2 screenPos, LayerMask layer)
        {
            var picked = PickCollider(screenPos, layer);
            if (picked.col != null)
                return picked.col;
            else if (picked.col2D != null)
                return picked.col2D;
            return null;
        }
        public static (Collider[] cols, Collider2D[] cols2D) OverlapSphere(Vector3 pos, float radius, LayerMask layer)
        {
            var cols = Physics.OverlapSphere(pos, radius, layer);
            if (cols != null && cols.Length > 0)
            {
                return (cols, null);
            }
            else
            {
                var cols2D = Physics2D.OverlapCircleAll(pos, radius, layer, -9999, 9999);
                if (cols2D != null && cols2D.Length > 0)
                {
                    return (null, cols2D);
                }
            }
            return (null, null);
        }
        public static Component[] OverlapSphereCom(Vector3 pos, float radius, LayerMask layer)
        {
            var picked = OverlapSphere(pos, radius, layer);
            if (picked.cols != null)
                return picked.cols;
            else if (picked.cols2D != null)
                return picked.cols2D;
            return null;
        }
        public static ScreenEdgeType ScreenEdge(GameObject go, float width)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(go.transform.position);
            if (screenPos.y >= Screen.height - width)
            {
                return ScreenEdgeType.Top;
            }
            if (screenPos.y <= width)
            {
                return ScreenEdgeType.Bot;
            }
            if (screenPos.x <= width)
            {
                return ScreenEdgeType.Left;
            }
            if (screenPos.x >= Screen.width - width)
            {
                return ScreenEdgeType.Right;
            }
            return ScreenEdgeType.None;
        }
        public static bool KeepInScreenEdge(GameObject go, float screenEdgeWidth, float moveStep)
        {
            bool ret = false;
            var curTrans = go.transform;
            var screenEdge = ScreenEdge(go, screenEdgeWidth);
            var sourcePos = curTrans.position;
            if (screenEdge == ScreenEdgeType.Top)
            {
                curTrans.position = curTrans.position.SetXZ(sourcePos.x + moveStep, sourcePos.z - moveStep);
                ret = true;
            }
            else if (screenEdge == ScreenEdgeType.Bot)
            {
                curTrans.position = curTrans.position.SetXZ(sourcePos.x - moveStep, sourcePos.z + moveStep);
                ret = true;
            }
            else if (screenEdge == ScreenEdgeType.Left)
            {
                curTrans.position = curTrans.position.SetXZ(sourcePos.x + moveStep, sourcePos.z + moveStep);
                ret = true;
            }
            else if (screenEdge == ScreenEdgeType.Right)
            {
                curTrans.position = curTrans.position.SetXZ(sourcePos.x - moveStep, sourcePos.z - moveStep);
                ret = true;
            }
            return ret;
        }
        #endregion

        #region pos
        public static Vector3 MousePos()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        }
        #endregion

        #region mail
        public static void Send(string mailTo, string subject, string body)
        {
            Application.OpenURL(string.Format("mailto:{0}?subject={1}&body={2}", mailTo, Uri.EscapeDataString(subject), Uri.EscapeDataString(body)));
        }
        #endregion

        #region other
        public static bool IsArrayValid<T>(IList<T> data)
        {
            if (data == null) return false;
            if (data.Count <= 0)
                return false;
            return true;
        }
        public static void CopyTextToClipboard(string str)
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = str;
            textEditor.OnFocus();
            textEditor.Copy();
        }
        public static T Cast<T>(object obj) where T : class
        {
            return obj as T;
        }

        public static List<Type> ReflectionSubClass(Type type)
        {
            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (item.IsDerivedFromOpenGenericType(type))
                    {
                        if (item.IsClass && !item.IsAbstract)
                        {
                            types.Add(item);
                        }
                    }
                }
            }
            return types;
        }
        #endregion

        #region screen pos
        public static float PosScreenX(Vector3 WorldPos)
        {
            if (Camera.main == null)
                return 0;
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            float x = Pos.x / Screen.width;
            return x;
        }
        // 转换屏幕坐标Y
        public static float PosScreenY(Vector3 WorldPos)
        {
            if (Camera.main == null)
                return 0;
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            float y = Pos.y / Screen.height;
            return y;
        }
        // 世界坐标转换到屏幕坐标
        public static Vector2 PosScreen(Vector3 WorldPos)
        {
            if (Camera.main == null)
                return Vector2.zero;
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            Vector2 retPos = new Vector2(Pos.x, Pos.y);
            return retPos;
        }
        public static Vector2 ScreenToWorld(Vector2 screenPos)
        {
            if (Camera.main == null)
                return Vector2.zero;
            var Pos = Camera.main.ScreenToWorldPoint(screenPos, Camera.MonoOrStereoscopicEye.Mono);
            return Pos;
        }
        #endregion

        #region for
        public static void For(int count, Callback<int> action)
        {
            for (int i = 0; i < count; ++i)
            {
                action?.Invoke(i);
            }
        }
        public static void Foreach<T>(IEnumerable<T> data, Callback<int, T> action)
        {
            int index = 0;
            foreach (var item in data)
            {
                action?.Invoke(index, item);
                index++;
            }
        }
        #endregion

        #region time
        public static long GetTimestamp()
        {
            return DateTime.Now.Ticks;
        }
        public static TimeSpan GetTimespan(long startTicks)
        {
            var ret = DateTime.Now.Ticks - startTicks;
            if (ret < 0)
                ret = 0;
            return TimeSpan.FromTicks(ret);
        }
        #endregion

        #region other
        public static byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }
        #endregion

        #region get
        public static string GetStr(string key, params object[] ps)
        {
            return BaseLangMgr.Get(key, ps);
        }
        public static string Joint(string key,params object[] ps)
        {
            return BaseLangMgr.Joint(key,ps);
        }
        public static BaseUnit GetPlayer(params BaseUnit[] units)
        {
            foreach (var item in units)
            {
                if (item.IsPlayer())
                    return item;
            }
            return null;
        }
        public static Transform GetTrans(Vector3 pos)
        {
            BaseGlobal.TempTrans.position = pos;
            return BaseGlobal.TempTrans;
        }
        #endregion

        #region other
        public static T CreateGlobalObj<T>(string name) where T : MonoBehaviour
        {
            if (BaseGlobal.Ins == null)
            {
                CLog.Error("CreateGlobalObj:错误!Global还未初始化!!!");
                return null;
            }
            var go = new GameObject(name);
            go.transform.SetParent(BaseGlobal.Ins.Trans);
            T instance = go.AddComponent<T>();
            return instance;
        }
        public static T CreateGlobalResourceObj<T>(string name) where T : MonoBehaviour
        {
            if (Starter.Ins == null)
            {
                CLog.Error("CreateGlobalObj:错误!Global还未初始化!!!");
                return null;
            }
            var prefab = Resources.Load(name);
            if (prefab == null)
            {
                CLog.Error("CreateGlobalObj:错误:"+ name);
                return null;
            }
            var go = GameObject.Instantiate(prefab) as GameObject;
            go.transform.SetParent(Starter.Ins.transform);
            T instance = go.GetComponent<T>();
            return instance;
        }
        public static GameObject CreateGlobalResourceGameObject(string name)
        {
            if (Starter.Ins == null)
            {
                CLog.Error("CreateGlobalResourceGameObject:错误!Global还未初始化!!!");
                return null;
            }
            var prefab = Resources.Load(name);
            if (prefab == null)
            {
                CLog.Error("CreateGlobalResourceGameObject:错误:" + name);
                return null;
            }
            var go = GameObject.Instantiate(prefab) as GameObject;
            go.transform.SetParent(Starter.Ins.transform);
            return go;
        }
        // 计算平均位置
        public static Vector3 AveragePos<T>(List<T> units) where T : BaseUnit
        {
            if (units == null || units.Count == 0)
                return SysConst.VEC_FarawayPos;
            Vector3 sum = new Vector3();
            foreach (var unit in units)
            {
                sum += unit.Pos;
            }
            return sum / units.Count;
        }
        #endregion

        #region is
        //检测组件和子组件是否有冲突
        public static bool IsFit(BaseMgr main, BaseMgr sub)
        {
            if (main.MgrType == MgrType.All || sub.MgrType == MgrType.All)
                return true;
            return main.MgrType == sub.MgrType;
        }
        //检测组件和Mono是否有冲突
        public static bool IsFit(BaseCoreMono main, BaseMgr com)
        {
            if (main.MonoType == MonoType.None ||
                main.MonoType == MonoType.Normal)
                return false;

            if (com.MgrType == MgrType.All)
                return true;

            if (main.MonoType == MonoType.Global ||
                main.MonoType == MonoType.View)
            {
                if (com.MgrType == MgrType.Global)
                    return true;
                return false;
            }

            if (main.MonoType == MonoType.Unit)
            {
                if (com.MgrType == MgrType.Unit)
                    return true;
                return false;
            }

            return false;
        }
        public static bool IsAnyPlayer(params BaseUnit[] unit)
        {
            foreach (var item in unit)
            {
                if (item == null)
                    continue;
                if (item.IsPlayer())
                    return true;
            }
            return false;
        }
        public static bool IsAllPlayer(params BaseUnit[] unit)
        {
            foreach (var item in unit)
            {
                if (item == null)
                    continue;
                if (item.IsPlayer())
                    return false;
            }
            return true;
        }
        #endregion

        #region invoke
        public static IJob Invoke(Action action, float delay=0.5f)
        {
            return SuperInvoke.Run(action, delay);
        }
        #endregion

        #region ray cast
        public static bool ScreenRayCast(out RaycastHit hit, LayerMask layer)
        {
            return ScreenRayCast(out hit, BaseInputMgr.ScreenPos, layer);
        }
        public static (Collider col, Collider2D col2D) PickCollider(LayerMask layer)
        {
            return PickCollider(BaseInputMgr.ScreenPos, layer);
        }
        public static Component PickColliderCom(LayerMask layer)
        {
            return PickColliderCom(BaseInputMgr.ScreenPos, layer);
        }
        public static Vector3 GetHitTerrinPoint()
        {
            Util.ScreenRayCast(out RaycastHit hit,BaseInputMgr.ScreenPos, (LayerMask)SysConst.Layer_Terrain);
            return hit.point;
        }
        #endregion

        #region misc
        public static string GetPlatformName()
        {
#if UNITY_STANDALONE_OSX
			return "OSX";
#elif UNITY_STANDALONE_WIN
			return "WIN";
#elif UNITY_STANDALONE_LINUX
			return "LINUX";
#elif UNITY_STANDALONE
			return "STANDALONE";
#elif UNITY_WII
			return "WII";
#elif UNITY_IOS
			return "IOS";
#elif UNITY_IPHONE
			return "IPHONE";
#elif UNITY_ANDROID
            return "ANDROID";
#elif UNITY_PS3
			return "PS3";
#elif UNITY_PS4
			return "PS4";
#elif UNITY_SAMSUNGTV
			return "SAMSUNGTV";
#elif UNITY_XBOX360
			return "XBOX360";
#elif UNITY_XBOXONE
			return "XBOXONE";
#elif UNITY_TIZEN
			return "TIZEN";
#elif UNITY_TVOS
			return "TVOS";
#elif UNITY_WP_8_1
			return "WP_8_1";
#elif UNITY_WSA_10_0
			return "WSA_10_0";
#elif UNITY_WSA_8_1
			return "WSA_8_1";
#elif UNITY_WSA
			return "WSA";
#elif UNITY_WINRT_10_0
			return "WINRT_10_0";
#elif UNITY_WINRT_8_1
			return "WINRT_8_1";
#elif UNITY_WINRT
			return "WINRT";
#elif UNITY_WEBGL
			return "WEBGL";
#else
            return "UNKNOWNHW";
#endif
        }
        public static void UnifyChildName(GameObject GO)
        {
            if (GO == null)
                return;
            for (int i = 0; i < GO.transform.childCount; ++i)
            {
                if (GO.transform.GetChild(i).GetComponent<LayoutElement>() != null)
                    continue;
                GO.transform.GetChild(i).name = "Item" + i;
            }
        }
        public static T GetSetting<T>(GameObject go) where T : PostProcessEffectSettings
        {
            var postProcessVolume = go.GetComponentInChildren<PostProcessVolume>();
            if (postProcessVolume == null)
                return null;
            T ret = default;
            if (postProcessVolume && postProcessVolume.profile)
                postProcessVolume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region mail
        void OpenEmail(string toEmail, string emailSubject, string emailBody)
        {
            emailSubject = System.Uri.EscapeUriString(emailSubject);
            emailBody = System.Uri.EscapeUriString(emailSubject);
            Application.OpenURL("mailto:" + toEmail + "?subject=" + emailSubject + "&body=" + emailBody);
        }
        #endregion

        #region DB
        public static void CopyToDB<TConfig, TData>(TConfig config, TData data, string tdid, long rtid) where TData : DBBase
        {
            CopyToDB(config, data);
            data.ID = rtid;
            data.TDID = tdid;
        }
        // 将配置数据复制到DB里,通过反射,减少操作,字段必须包含AttrCopyData特性
        // Config对象只能使用属性来映射
        // DBData对象可以使用属性或者字段来映射
        public static void CopyToDB<TConfig, TData>(TConfig config, TData data)
        {
            if (config == null || data == null) return;
            var cType = config.GetType();
            var dType = data.GetType();
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            //设置属性
            foreach (var dAttr in dType.GetMembers(flag))
            {
                var cAttr = cType.GetProperty(dAttr.Name, flag);
                if (cAttr != null)
                {
                    if (cAttr.GetReturnType().Name != dAttr.GetReturnType().Name)
                    {
                        CLog.Error("CopyToDB错误!名称相同但是类型不同,{0},{1}--{2}", dAttr.Name, data.ToString(), config.ToString());
                        continue;
                    }
                    dAttr.SetMemberValue(data, cAttr.GetValue(config));
                }
            }
        }
        // Config对象只能使用属性来映射
        // DBData对象可以使用属性或者字段来映射
        public static void CopyToTD<TData, TConfig>(TData data, TConfig config)
        {
            if (config == null || data == null) return;
            var cType = config.GetType();
            var dType = data.GetType();
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            //设置属性
            foreach (var dAttr in dType.GetMembers(flag))
            {
                var cAttr = cType.GetProperty(dAttr.Name, flag);
                if (cAttr != null)
                {
                    if (cAttr.GetReturnType().Name != dAttr.GetReturnType().Name)
                    {
                        CLog.Error("CopyToTD错误!名称相同但是类型不同,{0},{1}--{2}", dAttr.Name, data.ToString(), config.ToString());
                        continue;
                    }
                    cAttr.SetValue(config, dAttr.GetMemberValue(data));
                }
            }
        }

        public static void CopyToData(TDBaseData config, DBBase data)
        {
            data.CustomName = config.CustomName;
            data.ID = config.ID;
            data.TDID = config.TDID;
        }
        public static void CopyToConfig(DBBase data, TDBaseData config)
        {
            config.CustomName = data.CustomName;
            config.ID = data.ID;
            config.TDID = data.TDID;
        }
        #endregion
    }

}