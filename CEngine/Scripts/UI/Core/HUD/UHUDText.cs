//**********************************************
// Class Name	: CYMBaseHUDText
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
// HUD跳字的基类
//**********************************************
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UHUDText : UHUDItem
    {
        #region inspector
        [SerializeField]
        Text Text;
        #endregion

        #region methon
        public override void Init( BaseUnit unit, Transform followObj)
        {
            base.Init(unit, followObj);

        }
        public void SetText(string text)
        {
            if (Text != null)
                Text.text = text;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Text != null)
                Text.color = Color;
        }
        #endregion
    }
}