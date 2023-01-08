//------------------------------------------------------------------------------
// StarterUI.cs
// Copyright 2022 2022/10/6 
// Created by CYM on 2022/10/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    [HideMonoScript]
    public class StarterUI : MonoBehaviour 
    {
        [SerializeField]
        CanvasScaler CanvasScaler;
        [SerializeField]
        Image Logo;
        [SerializeField]
        Image Fill;

        public static StarterUI Ins { get; private set; }
        UIConfig UIConfig => UIConfig.Ins;

        #region set
        private void Awake()
        {
            Ins = this;
            CanvasScaler.referenceResolution = new Vector2(UIConfig.Width, UIConfig.Height);
            Logo.SetActive(UIConfig.StartLogo!=null);
            if (Logo.IsActiveSelf())
            {
                Logo.sprite = UIConfig.StartLogo;
                Logo.SetNativeSize();
            }
        }
        private void Update()
        {
            if (Fill)
            {
                Fill.fillAmount = DLC.DLCDownloader.DownloadProgress;
            }
        }
        public static void DoDestroy()
        {
            Destroy(Ins.gameObject);
        }
        #endregion

    }
}