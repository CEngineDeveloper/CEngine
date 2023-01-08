using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
	/// <summary>
	/// 矩形引导组件
	/// </summary>
	public class UIRectFocusing : UIBaseFocusing
	{
        #region prop
		/// <summary>
		/// 最终的偏移值X
		/// </summary>
		private float _targetOffsetX = 0f;
		/// <summary>
		/// 最终的偏移值Y
		/// </summary>
		private float _targetOffsetY = 0f;
		/// <summary>
		/// 当前的偏移值X
		/// </summary>
		private float _currentOffsetX = 0f;
		/// <summary>
		/// 当前的偏移值Y
		/// </summary>
		private float _currentOffsetY = 0f;
		/// <summary>
		/// 动画时间
		/// </summary>
		private float _shrinkVelocityX = 0f;
		private float _shrinkVelocityY = 0f;
		float rectX => _currentOffsetX * 2;
		float rectY => _currentOffsetY * 2;
		Rect scaledRect => new Rect(new Vector2(_center.x- rectX/2, _center.y- rectY/2), new Vector2(rectX, rectY));
		#endregion

		#region is
		public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (Target == null)
				return true;
			Vector2 localPositionPivotRelative;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Target.rectTransform, sp, eventCamera, out localPositionPivotRelative);
			bool ret = !scaledRect.Contains(localPositionPivotRelative);
			return ret;
		}
        #endregion

        #region life
        protected override void Awake()
		{
			base.Awake();
			SetRect(0, 0);
		}
		protected override void Update()
		{
			base.Update();
			//从当前偏移值到目标偏移值差值显示收缩动画
			float valueX = Mathf.SmoothDamp(_currentOffsetX, _targetOffsetX, ref _shrinkVelocityX, _shrinkTime);
			float valueY = Mathf.SmoothDamp(_currentOffsetY, _targetOffsetY, ref _shrinkVelocityY, _shrinkTime);
			if (!Mathf.Approximately(valueX, _currentOffsetX))
			{
				_currentOffsetX = valueX;
				_material.SetFloat("_SliderX", _currentOffsetX);
			}

			if (!Mathf.Approximately(valueY, _currentOffsetY))
			{
				_currentOffsetY = valueY;
				_material.SetFloat("_SliderY", _currentOffsetY);
			}
		}
		#endregion

		#region pub
		public void SetRect(float x,float y)
		{
			_currentOffsetX = x*2;
			_currentOffsetY = y*2;
			_targetOffsetX = x;
			_targetOffsetY = y;
		}
		#endregion
	}
}