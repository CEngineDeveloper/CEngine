//------------------------------------------------------------------------------
// Camera2DAutoSize.cs
// Copyright 2019 2019/12/9 
// Created by CYM on 2019/12/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM.UI
{
    public enum Camera2DScaleType
    {
        FixedWidth,
        FixedHeight,
    }
    public class UICamera2DAutoSize : MonoBehaviour
    {
        public Camera2DScaleType scaleMode = Camera2DScaleType.FixedWidth;
        Camera Camera;

        public float designWidth = 16f;
        public float designHeight = 9f;

        // Use this for initialization
        void Start()
        {
            Camera = GetComponent<Camera>();

        }

        private void Update()
        {
            float aspectRatio = Screen.width * 1.0f / Screen.height;
            float orthographicSize = 0;

            switch (scaleMode)
            {
                case Camera2DScaleType.FixedWidth:
                    orthographicSize = designWidth / (2 * aspectRatio);
                    break;
                case Camera2DScaleType.FixedHeight:
                    orthographicSize = designHeight / 2;
                    break;
            }

            Camera.orthographicSize = orthographicSize;
        }
    }
}