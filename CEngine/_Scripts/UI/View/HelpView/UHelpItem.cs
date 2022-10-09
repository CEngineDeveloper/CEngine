//**********************************************
// Class Name	: HelpItem
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
namespace CYM.UI
{
    public class UHelpItemData : UData
    {
        public UButtonData Close;
    }

    public class UHelpItem : UPres<UHelpItemData>
    {
        #region inspector
        [SerializeField]
        UText Info;
        #endregion

        #region prop
        Timer timer = new Timer();
        float Duration = 0.0f;
        #endregion

        #region life
        public override void Init(UHelpItemData data)
        {
            base.Init(data);
        }
        private void Update()
        {
            if (IsShow)
            {
                if (timer.Elapsed() > Duration)
                {
                    Show(false);
                    timer.Restart();
                }

            }
        }
        #endregion

        #region set
        public override void Show(bool b = true, bool isForce = false)
        {
            base.Show(b, isForce);
        }
        /// <summary>
        /// 显示帮助内容
        /// </summary>
        /// <param name="info"></param>
        /// <param name="duration">如果不为null 则在指定时间后自动关闭</param>
        public void Show(string key, float? duration = null, params object[] ps)
        {
            Show(true);
            Info.NameText = GetStr(key, ps);
            if (duration.HasValue)
                Duration = duration.Value;
            else
                Duration = float.MaxValue;
            timer.Restart();
        }
        /// <summary>
        /// 显示字符
        /// </summary>
        public void ShowStr(string str, float? duration = null)
        {
            Show(true);
            Info.NameText = str;
            if (duration.HasValue)
                Duration = duration.Value;
            else
                Duration = float.MaxValue;
            timer.Restart();
        }
        #endregion
    }
}