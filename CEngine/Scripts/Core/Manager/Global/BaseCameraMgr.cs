using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace CYM
{
    public class BaseCameraMgr : BaseGFlowMgr, ICameraMgr
    {
        #region wrap
        public override Vector3 Pos => MainCameraTrans.position;
        public override Vector3 LocalPos => MainCameraTrans.localPosition;
        #endregion

        #region Interface
        public float LastScrollVal { get; private set; } = 0;
        public virtual float ScrollVal => 0;
        public float SafeScrollVal => Mathf.Clamp(ScrollVal,0.1f,1.0f);
        public float GetCustomScrollVal(float maxVal) => Mathf.Clamp(ScrollVal / maxVal, 0, 1.0f);
        public float GetCustomHightPercent(float maxVal) => Mathf.Clamp(HightPercent / maxVal, 0, 1.0f);
        public virtual float HightPercent => 0;
        public float CameraHight { get; private set; } = 1;
        public PostProcessVolume PostProcessVolume { get; private set; }
        public Camera MainCamera { get; private set; }
        public Transform MainCameraTrans { get; private set; }
        #endregion

        #region is height
        public bool IsInScroll { get; private set; }
        public virtual bool IsMoving => true;
        protected virtual float LessHight => 0.1f;
        protected virtual float LowHight => 0.3f;
        protected virtual float MidHight => 0.5f;
        protected virtual float MoreHight => 0.7f;
        protected virtual float TopHight => 0.8f;
        protected virtual float MostHight => 0.9f;
        protected virtual float EndHight => 0.999f;
        public bool IsLessHight => LastScrollVal >= LessHight;
        public bool IsLowHight => LastScrollVal >= LowHight;
        public bool IsMidHight => LastScrollVal >= MidHight;
        public bool IsMoreHight => LastScrollVal >= MoreHight;
        public bool IsTopHight => LastScrollVal >= TopHight;
        public bool IsMostHight => LastScrollVal >= MostHight;
        public bool IsEndHight => LastScrollVal >= EndHight;
        #endregion

        #region Callback
        public event Callback<Camera> Callback_OnFetchCamera;
        public event Callback<CameraHightType,bool> Callback_OnHightChanged;
        public event Callback<bool> Callback_OnIsLessHight;
        public event Callback<bool> Callback_OnIsLowHight;
        public event Callback<bool> Callback_OnIsMidHight;
        public event Callback<bool> Callback_OnIsMoreHight;
        public event Callback<bool> Callback_OnIsTopHight;
        public event Callback<bool> Callback_OnIsMostHight;
        #endregion

        #region life
        protected override string ResourcePrefabKey => "";
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
            Enable(false);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (MainCameraTrans)
            {
                CameraHight = MainCameraTrans.position.y;
            }
            bool preIsLessHight = IsLessHight;
            bool preIsLowHight = IsLowHight;
            bool preIsMidHight = IsMidHight;
            bool preIsMoreHight = IsMoreHight;
            bool preIsTopHight = IsTopHight;
            bool preIsMostHight = IsMostHight;

            if (LastScrollVal != ScrollVal)
            {
                IsInScroll = true;
                LastScrollVal = ScrollVal;
                if (IsLessHight != preIsLessHight)
                {
                    Callback_OnIsLessHight?.Invoke(IsLessHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.Less, IsLessHight);
                }
                if (IsLowHight != preIsLowHight)
                {
                    Callback_OnIsLowHight?.Invoke(IsLowHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.Low, IsLowHight);
                }
                if (IsMidHight != preIsMidHight)
                {
                    Callback_OnIsMidHight?.Invoke(IsMidHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.Mid, IsMidHight);
                }
                if (IsMoreHight != preIsMoreHight)
                {
                    Callback_OnIsMoreHight?.Invoke(IsMoreHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.More, IsMoreHight);
                }
                if (IsTopHight != preIsTopHight)
                {
                    Callback_OnIsTopHight?.Invoke(IsTopHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.Top, IsTopHight);
                }
                if (IsMostHight != preIsMostHight)
                {
                    Callback_OnIsMostHight?.Invoke(IsMostHight);
                    Callback_OnHightChanged?.Invoke(CameraHightType.Most, IsMostHight);
                }
            }
            else {
                IsInScroll = false;
            }
        }
        //重场景中重新获得Camera
        public virtual void FetchCamera()
        {
            if (ResourcePrefabKey.IsInv())
            {
                ResourceObj = CameraObj.GO;
            }
            if (ResourceObj != null)
            {
                MainCamera = ResourceObj.GetComponent<Camera>();
                MainCameraTrans = MainCamera.transform;
                PostProcessVolume = MainCamera.GetComponentInChildren<PostProcessVolume>();
                Callback_OnFetchCamera?.Invoke(MainCamera);
            }
            else
            {
                CLog.Error($"没有设置Camera,请设置{nameof(ResourcePrefabKey)}或者{nameof(CameraObj)}");
            }
        }
        #endregion

        #region get
        public Vector3 CameraPos
        {
            get
            {
                if (MainCameraTrans == null)
                    return Vector3.zero;
                return MainCameraTrans.position;
            }
        }
        public T GetPostSetting<T>() where T : PostProcessEffectSettings
        {
            T ret;
            if (PostProcessVolume == null)
                return null;
            PostProcessVolume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region set
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (MainCamera != null)
            {
                MainCamera.enabled = b;
            }
        }
        public void EnableSkyBox(bool b)
        {
            if (MainCamera != null)
            {
                if (b) MainCamera.clearFlags = CameraClearFlags.Skybox;
                else MainCamera.clearFlags = CameraClearFlags.SolidColor;
            }
        }
        #endregion

        #region jump
        int CurJumpIndex = 0;
        IList LastJumpList;
        public virtual void SetPos(Vector3 pos)
        {
            //pos = pos.SetZ(-2);
            MainCameraTrans.transform.position = pos;
            Jump(pos);
        }
        public virtual void Jump(Vector3 pos, float? heightPercent = null)
        {
            throw new NotImplementedException();
        }
        public void Jump(Transform target, float? heightPercent = null)
        {
            if (target == null) return;
            Jump(target.position, heightPercent);
        }
        public  void Jump(BaseUnit target, float? heightPercent = null)
        {
            if (target == null) return;
            Jump(target.Trans.position, heightPercent);
        }
        public void Jump<TUnit>(List<TUnit> list, float? heightPercent = null, Func<TUnit, Transform> customTrans = null) where TUnit : BaseUnit
        {
            if (list.Count <= 0) return;
            if (LastJumpList != list) CurJumpIndex = 0;
            LastJumpList = list;
            if (list.Count <= CurJumpIndex) CurJumpIndex = 0;
            if (customTrans == null)
            {
                Jump(list[CurJumpIndex], heightPercent);
            }
            else
            {
                Jump(customTrans(list[CurJumpIndex]).position, heightPercent);
            }
            CurJumpIndex++;
        }
        #endregion

        #region Callback
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            FetchCamera();
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            Enable(true);
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            Enable(false);
        }
        #endregion
    }

}