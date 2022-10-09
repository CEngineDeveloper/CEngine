//------------------------------------------------------------------------------
// UIBaseFocusing.cs
// Copyright 2021 2021/5/6 
// Created by CYM on 2021/5/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine.UI;
using UnityEngine;
using CYM.UI;
namespace CYM
{
    public class UIBaseFocusing : MonoBehaviour, ICanvasRaycastFilter
    {
        #region prop
        /// <summary>
        /// 要高亮显示的目标
        /// </summary>
        public Image Target { get; private set; }
        /// <summary>
        /// 镂空区域圆心
        /// </summary>
        protected Vector4 _center { get; private set; }
        /// <summary>
        /// 遮罩材质
        /// </summary>
        protected Material _material { get; private set; }
        /// <summary>
        /// 动画收缩时间
        /// </summary>
        protected float _shrinkTime { get; private set; } = 0.2f;
        protected UControl _control { get; private set; }
        protected RectTransform ParentRectTrans { get; private set; }
        #endregion

        #region life
        protected virtual void Awake()
		{
            Target = GetComponent<Image>();
            _material = GameObject.Instantiate(Target.material);
            Target.material = _material;
            ParentRectTrans = transform.parent as RectTransform;
            SetCenter(Vector4.zero);
        }
		protected virtual void Update()
		{
            if (_control != null)
            {
                Vector3 pos = ParentRectTrans.InverseTransformPoint(_control.RectTrans.position);
                SetCenter(pos);
            }
		}
        #endregion

        #region is
        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return true;
		}
        #endregion

        #region set
        public void SetCenter(Vector2 pos)
        {
            _center = pos;
            _material.SetVector("_Center", _center);
        }
        public void SetCenter(UControl control)
        {
            _control = control;
        }
        #endregion
    }
}