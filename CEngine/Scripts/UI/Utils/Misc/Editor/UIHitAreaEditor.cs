//------------------------------------------------------------------------------
// UIHitAreaEditor.cs
// Created by CYM on 2021/9/17
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace CYM.UI
{
    [CanEditMultipleObjects, CustomEditor(typeof(UIHitArea))]
    public class UIHitAreaEditor : Editor
    {
		public Vector3[] scaledWorldCorners
		{
			get
			{
				UIHitArea filter = this.target as UIHitArea;

				RectTransform rt = (RectTransform)filter.transform;
				Vector3[] corners = new Vector3[4];
				Rect scaledRect = filter.scaledRect;
				corners[0] = new Vector3(rt.rect.x + scaledRect.x, rt.rect.height + rt.rect.y + scaledRect.y, 0f);
				corners[1] = new Vector3(rt.rect.x + scaledRect.x, rt.rect.height + rt.rect.y + scaledRect.y + scaledRect.height, 0f);
				corners[2] = new Vector3(rt.rect.x + scaledRect.x + scaledRect.width, rt.rect.height + rt.rect.y + scaledRect.y + scaledRect.height, 0f);
				corners[3] = new Vector3(rt.rect.x + scaledRect.x + scaledRect.width, rt.rect.height + rt.rect.y + scaledRect.y, 0f);

				for (int i = 0; i < 4; i++)
				{
					corners[i] += new Vector3(rt.rect.width * rt.pivot.x, 0f, 0f);
					corners[i] += new Vector3(0f, (rt.rect.height * (1f - rt.pivot.y)) * -1f, 0f);
					corners[i] = rt.TransformPoint(corners[i]);
				}

				return corners;
			}
		}
		protected void OnSceneGUI()
		{
			Vector3[] worldCorners = this.scaledWorldCorners;

			Handles.color = Color.green;
			Handles.DrawLine(worldCorners[0], worldCorners[1]); // Left line
			Handles.DrawLine(worldCorners[1], worldCorners[2]); // Top line
			Handles.DrawLine(worldCorners[2], worldCorners[3]); // Right line
			Handles.DrawLine(worldCorners[3], worldCorners[0]); // Bottom line
		}
	}
}