//------------------------------------------------------------------------------
// UICanvasMatch.cs
// Copyright 2021 2021/3/8 
// Created by CYM on 2021/3/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
namespace CYM.UI
{
    [HideMonoScript]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UICanvasMatch : MonoBehaviour 
    {
        #region Inspector
        CanvasScaler CanvasScaler;
        [SerializeField,ReadOnly]
        float Threshold = 0.5625f;        
        #endregion

        #region life
        private void Awake()
        {
            CanvasScaler = GetComponent<CanvasScaler>();
        }
        private void Start()
        {
            ReFit();
        }

        private void OnApplicationFocus(bool focus)
        {
            //ReFit();
        }

        private void Update()
        {
            RefreshRadio();
        }
        #endregion

        #region set
        public void ReFit()
        {
            if (CanvasScaler != null)
            {
                CanvasScaler.enabled = false;
                Util.Invoke(() => {
                    CanvasScaler.enabled = true;
                    Threshold = UIConfig.Ins.Height / UIConfig.Ins.Width;
                }, 0.1f);
            }
        }

        public void RefreshRadio()
        {
            if (CanvasScaler != null)
            {
                float radio = Screen.width / Screen.height;
                if (radio <= Threshold)
                    CanvasScaler.matchWidthOrHeight = 0.0f;
                else
                    CanvasScaler.matchWidthOrHeight = 1.0f;
            }
        }
        #endregion
    }
}