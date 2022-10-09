//------------------------------------------------------------------------------
// UMinMap.cs
// Created by CYM on 2021/12/20
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UMinMapData : UData
    {
        public Callback<Vector3> OnClickOrDragMapPos;
    }
    [AddComponentMenu("UI/Control/UMinMap")]
    [HideMonoScript][RequireComponent(typeof(CanvasRenderer))]
    public class UMinMap : UPres<UMinMapData>
    {
        #region Inspector
        UILine LineRect;
        // 地图信息用于映射小地图位置和世界位置
        // 地图尺寸
        public Vector2 MapSize = new Vector2(1024, 1024);
        #endregion

        #region life
        protected override void Start()
        {
            base.Start();
            LineRect = new GameObject("LineRect").SafeAddComponet<UILine>();
            LineRect.ClearPoints(5);
            LineRect.transform.SetParent(RectTrans);
            LineRect.LineThickness = 2;
            LineRect.RelativeSize = true;
            LineRect.rectTransform.localScale = Vector2.one;
            LineRect.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            LineRect.rectTransform.anchorMin = new Vector2(0, 0);
            LineRect.rectTransform.anchorMax = new Vector2(1f, 1f);
            LineRect.rectTransform.sizeDelta = new Vector2(0,0);
            LineRect.rectTransform.anchoredPosition = Vector2.zero;
        }
        void FixedUpdate()
        {
            if (IsShow)
            {
                DrawCameraRect();
            }
        }
        #endregion

        #region Callback
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnClickOrDrag(eventData.position);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            OnClickOrDrag(eventData.position);
        }
        public void OnClickOrDrag(Vector3 pos)
        {
            var worldPos = ScreenPosToWorldPos(pos);
            Data?.OnClickOrDragMapPos?.Invoke(worldPos);
        }
        #endregion

        #region utile
        // 屏幕坐标到世界坐标
        // 屏幕坐标：往往是鼠标坐标
        // 世界坐标：鼠标点击小地图位置所对应的世界位置
        Vector3 ScreenPosToWorldPos(Vector3 pos)
        {
            Vector2 localPos = RectTrans.InverseTransformPoint(pos);
            Vector2 offset = localPos - RectTrans.rect.min;
            Vector2 normalized = new Vector2(offset.x / RectTrans.rect.size.x, offset.y / RectTrans.rect.size.y);
            Vector3 worldPos = NormalizedToWorld(normalized);
            return worldPos;
        }
        void DrawCameraRect()
        {
            if (LineRect == null)
                return;
            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 D;
            Vector2 E;

            A = CameraViewportToMapPoint(new Vector3(0, 0));
            B = CameraViewportToMapPoint(new Vector3(0, 1));
            C = CameraViewportToMapPoint(new Vector3(1, 1));
            D = CameraViewportToMapPoint(new Vector3(1, 0));
            E = CameraViewportToMapPoint(new Vector3(0.5f, 0.5f, 0));


            LineRect.SetPoint(0,A);
            LineRect.SetPoint(1,B);
            LineRect.SetPoint(2,C);
            LineRect.SetPoint(3,D);
            LineRect.SetPoint(4,A);
            LineRect.SetDirtyIfPointsChanged();
        }

        Vector2 CameraViewportToMapPoint(Vector3 viewportPoint)
        {
            if (BaseGlobal.MainCamera == null)
                return Vector2.zero;
            var ray = BaseGlobal.MainCamera.ViewportPointToRay(viewportPoint);
            float d = (0 - ray.origin.y) / ray.direction.y;
            Vector3 hit = ray.GetPoint(d);
            return WorldToNormalizedPoint(hit);
        }

        // 忽略y
        Vector2 WorldToNormalizedPoint(Vector3 world)
        {
            Vector2 r = new Vector2(world.x, world.z);
            return new Vector2(Mathf.Clamp01(r.x / MapSize.x), Mathf.Clamp01(r.y / MapSize.y));
        }

        Vector3 NormalizedToWorld(Vector2 normalized)
        {
            Vector2 pos = new Vector2(normalized.x * MapSize.x, normalized.y * MapSize.y);
            return new Vector3(pos.x, 0, pos.y);
        }
        #endregion
    }
}