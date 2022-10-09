//------------------------------------------------------------------------------
// BaseLineRenderMgr.cs
// Copyright 2019 2019/4/20 
// Created by CYM on 2019/4/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DigitalRuby.FastLineRenderer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.Line
{
    public class BaseULineMgr : BaseMgr
    {
        #region prop
        FastLineRenderer LineRender;
        Color PathColor = Color.green;
        List<Vector3> PathVector = new List<Vector3>();
        #endregion

        #region life
        protected virtual float LineRadius => 0.025f;
        protected virtual float LineYOffset => 0.3f;
        protected virtual float LineEndCapScale => 5.0f;
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnEnable()
        {
            base.OnEnable();
            SelfBaseUnit.Callback_OnBeSelected += OnBeSelected;
            SelfBaseUnit.Callback_OnUnBeSelected += OnUnBeSelected;
            if (SelfBaseUnit.MoveMgr != null)
            {
                SelfBaseUnit.MoveMgr.Callback_OnMoveStart += OnMoveStart;
                SelfBaseUnit.MoveMgr.Callback_OnMoveEnd += OnMoveEnd;
                SelfBaseUnit.MoveMgr.Callback_OnMovingStep += OnMovingStep;
            }
        }
        public override void OnDisable()
        {
            ClearPath();
            SelfBaseUnit.Callback_OnBeSelected -= OnBeSelected;
            SelfBaseUnit.Callback_OnUnBeSelected -= OnUnBeSelected;
            if (SelfBaseUnit.MoveMgr != null)
            {
                SelfBaseUnit.MoveMgr.Callback_OnMoveStart -= OnMoveStart;
                SelfBaseUnit.MoveMgr.Callback_OnMoveEnd -= OnMoveEnd;
                SelfBaseUnit.MoveMgr.Callback_OnMovingStep -= OnMovingStep;
            }
            base.OnDisable();
        }

        public override void OnDeath()
        {
            base.OnDeath();
            ClearPath();
        }
        #endregion

        #region set
        // 一般上会在玩家选择的时候绘制
        public void ShowColor(Color color)
        {
            PathColor = color;
        }

        public void DrawPath(List<Vector3> newPath, Color col)
        {
            BaseGlobal.PathRenderMgr.ClearPath(ref LineRender);
            LineRender = BaseGlobal.PathRenderMgr.DrawPath(newPath, col, LineRadius, LineEndCapScale);
        }
        public void ClearPath()
        {
            IsForceDrawPath = false;
            BaseGlobal.PathRenderMgr.ClearPath(ref LineRender);
        }
        public void UpdateDrawPath()
        {
            if (SelfBaseUnit.MoveMgr == null)
                return;
            if (
                IsNeedDrawPath ||
                IsForceDrawPath)
            {
                DrawPath(PathVector, PathColor);
            }
            else
            {
                ClearPath();
            }
        }
        #endregion

        #region is
        public bool HasDrawPath => LineRender != null;
        public bool IsForceDrawPath { get; private set; } = false;
        public bool IsNeedDrawPath=> (IsForceDrawPath || BaseInputMgr.IsSelectUnit(SelfBaseUnit)) && PathVector.Count>0;
        #endregion

        #region Callback
        private void OnBeSelected(bool arg1)
        {
            UpdateDrawPath();
        }
        private void OnUnBeSelected()
        {
            ClearPath();
        }
        private void OnMoveStart()
        {
            if (SelfBaseUnit.AStarMoveMgr != null &&
                SelfBaseUnit.AStarMoveMgr.ABPath != null &&
                SelfBaseUnit.AStarMoveMgr.ABPath.vectorPath != null)
            {
                PathVector.Clear();
                PathVector.AddRange(SelfBaseUnit.AStarMoveMgr.ABPath.vectorPath);
                for (int i = 0; i < PathVector.Count; i++)
                {
                    Vector3 point = PathVector[i];
                    float terrainY = TerrainObj.Ins.SampleHeight(point) + LineYOffset;
                    float bakedPos = BaseSceneRoot.Ins.GetBakedColliderPos(point).y + LineYOffset;
                    if (terrainY < bakedPos)
                        terrainY = bakedPos;
                    // 避免地面小幅度起伏时先画在地面下面的问题
                    point.y = terrainY;
                    PathVector[i] = point;
                }
            }
            UpdateDrawPath();
        }
        private void OnMoveEnd()
        {
            ClearPath();
            PathVector.Clear();
        }
        protected void OnMovingStep()
        {
            if (PathVector.Count > 4)
                PathVector.RemoveAt(0);
            UpdateDrawPath();
        }
        
        #endregion
    }
}