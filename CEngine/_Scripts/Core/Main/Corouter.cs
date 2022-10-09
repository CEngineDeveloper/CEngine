
//**********************************************
// Class Name	: Pool
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.Collections.Generic;

namespace CYM
{
    public sealed class Corouter
    {
        string tag;
        public Corouter(string tag)
        {
            this.tag = tag;
        }
        public CoroutineHandle Run(IEnumerator<float> coroutine)
        {
            return Timing.RunCoroutine(coroutine, Segment.Update, tag);
        }
        public void RunList(params IEnumerator<float>[] coroutine)
        {
            foreach (var item in coroutine)
                Run(item);
        }
        public void Kill()
        {
            Timing.KillCoroutines(tag);
        }
        public void Kill(CoroutineHandle coroutine)
        {
            coroutine.IsRunning = false;
        }
        public void Pause()
        {
            Timing.PauseCoroutines(tag);
        }
        public void Pause(CoroutineHandle handle)
        {
            Timing.PauseCoroutines(handle);
        }
        public void Resume()
        {
            Timing.ResumeCoroutines(tag);
        }
        public void Resume(CoroutineHandle handle)
        {
            Timing.ResumeCoroutines(handle);
        }

        #region static
        static Corouter Ins = new Corouter("System");
        public static CoroutineHandle GRun(IEnumerator<float> coroutine)
        {
            return Timing.RunCoroutine(coroutine, Segment.Update, Ins.tag);
        }
        public static void GRunList(params IEnumerator<float>[] coroutine)
        {
            foreach (var item in coroutine)
                GRun(item);
        }
        #endregion
    }
}
