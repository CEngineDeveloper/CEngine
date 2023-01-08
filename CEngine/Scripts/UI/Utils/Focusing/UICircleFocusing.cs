using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
	/// <summary>
	/// 圆形遮罩镂空引导
	/// </summary>
	public class UICircleFocusing : UIBaseFocusing
	{
        #region prop
		/// <summary>
		/// 镂空区域半径
		/// </summary>
		private float _radius;
		/// <summary>
		/// 当前高亮区域的半径
		/// </summary>
		private float _currentRadius;
		/// <summary>
		/// 收缩速度
		/// </summary>
		private float _shrinkVelocity = 0f;
		#endregion

		#region is
		public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (Target == null)
				return true;
			if (_currentRadius == 0f)
				return false;

			RectTransform rectTransform = Target.rectTransform;
			Vector2 localPositionPivotRelative;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPositionPivotRelative);
			bool ret = Vector2.Distance(localPositionPivotRelative, _center) > _currentRadius;
			return ret;
		}
        #endregion

        #region life
        protected override void Awake()
		{
			base.Awake();
			SetRadius(0);
		}
		protected override void Update()
		{
			base.Update();
			//从当前半径到目标半径差值显示收缩动画
			float value = Mathf.SmoothDamp(_currentRadius, _radius, ref _shrinkVelocity, _shrinkTime);
			if (!Mathf.Approximately(value, _currentRadius))
			{
				_currentRadius = value;
				_material.SetFloat("_Slider", _currentRadius);
			}
		}
		#endregion

		#region pub
		public void SetRadius(float val)
		{
			_currentRadius = val * 2;
			   _radius = val;
		}
		
        #endregion
    }

}