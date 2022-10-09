//------------------------------------------------------------------------------
// UPosMap.cs
// Created by CYM on 2021/11/6
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using DG.Tweening;
using CYM.Pool;
namespace CYM.UI
{
    public class UPosMapData : UData
    {
        public Func<TextAsset> MapPosAssets = () => null;
        public Func<string> MapPosPath = () => null;
        public Func<float> MapNormal =()=> 1024f;
        public Func<Vector2> MapSize = ()=> new Vector2(462f,462f);
        public Func<List<string>> Castles;
        public Func<string> Capital = null;
    }
    [AddComponentMenu("UI/Control/UPosMap")]
    [HideMonoScript]
    public class UPosMap : UPres<UPosMapData>
    {
        #region Inspector
        [SerializeField]
        RectTransform ICapital;
        [SerializeField]
        RectTransform ICastle;
        #endregion

        #region prop
        GOPool CastleItemPool;
        Tween AnchorTweenScale;
        Tween TweenPos;
        PositionData PosData  = null;
        #endregion

        #region life
        public override void Init(UPosMapData data)
        {
            base.Init(data);
            if (PosData == null)
            {
                string mapPath = data.MapPosPath.Invoke();
                TextAsset textAsset = data.MapPosAssets.Invoke();
                float normal = data.MapNormal.Invoke();
                Vector2 size = data.MapSize.Invoke();
                if (mapPath != null)
                {
                    PosData = new PositionData(mapPath, normal, size.x, size.y);
                }
                else if (textAsset != null)
                {
                    PosData = new PositionData(textAsset, normal, size.x, size.y);
                }
            }
        }
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            CastleItemPool = new GOPool(ICastle.gameObject, ICastle.parent);
            ICastle.gameObject.SetActive(false);
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data == null)
            {
                CLog.Error("错误!UPosMap Data == null");
                return;
            }
            if (PosData == null)
            {
                CLog.Error("错误!UPosMap PosData == null");
                return;
            }
            if (Data.Capital == null)
            {
                CLog.Error("错误!UPosMap Data.Capital == null");
                return;
            }
            if (Data.Castles == null)
            {
                CLog.Error("错误!CastleID == null");
                return;
            }
            if (Data != null && 
                PosData != null && 
                Data.Capital!=null &&
                Data.Castles!=null)
            {
                if (AnchorTweenScale != null) AnchorTweenScale.Kill();
                if (TweenPos != null) TweenPos.Kill();
                string capital = Data.Capital?.Invoke();
                //ICapital.transform.localScale = Vector3.one * 0.01f;
                //AnchorTweenScale = ICapital.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
                ICapital.anchoredPosition = PosData.GetCastleMinMapPos(capital?.TrimStart(SysConst.Prefix_Castle));
                //TweenPos = DOTween.To(() => ICapital.anchoredPosition, (x) => ICapital.anchoredPosition = x, PosData.GetCastleMinMapPos(capital?.TrimStart(Const.Prefix_Castle)), 0.2f);

                CastleItemPool.DespawnAll();
                var castleDatas = Data.Castles.Invoke();
                for (int i = 0; i < castleDatas.Count; ++i)
                {
                    if (i == 0) continue;
                    var castleID = castleDatas[i];
                    var castle = CastleItemPool.Spawn();
                    RectTransform rect = castle.transform as RectTransform;
                    //rect.DOShakeScale(0.2f);
                    rect.anchoredPosition = PosData.GetCastleMinMapPos(castleID.TrimStart(SysConst.Prefix_Castle));
                }
                ICapital.SetAsLastSibling();
            }
        }
        #endregion
    }
}