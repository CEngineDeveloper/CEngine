//------------------------------------------------------------------------------
// UIHitArea.cs
// Created by CYM on 2021/9/17
// 填写类的描述...用于自定义放大控件的点击区域
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UIHitArea : MonoBehaviour
    {
        public float width=2;
        public float height=2;

        public Rect scaledRect
        {
            get
            {
                RectTransform rt = (RectTransform)this.transform;
                return new Rect(
                    ((rt.rect.x + ((rt.rect.width - (rt.rect.width * this.width)) / 2f))),
                    ((rt.rect.y + ((rt.rect.height - (rt.rect.height * this.height)) / 2f))),
                    ((rt.rect.width * this.width)),
                    ((rt.rect.height * this.height))
                );
            }
        }

        void Awake()
        {
            var rectTransform = transform as RectTransform;
            GameObject gobj = new GameObject("HitZone");
            gobj.hideFlags = HideFlags.HideInHierarchy;
            RectTransform hitzoneRectTransform = gobj.AddComponent<RectTransform>();
            hitzoneRectTransform.SetParent(transform);
            hitzoneRectTransform.localPosition = Vector3.zero;
            hitzoneRectTransform.localScale = Vector3.one;
            hitzoneRectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * width, rectTransform.sizeDelta.y * height);
            var image = gobj.AddComponent<RawImage>();
            image.color = new Color(1,1,1,0);
        }
    }
}