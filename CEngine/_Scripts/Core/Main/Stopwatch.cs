namespace CYM
{
    public class Stopwatch
    {
        static string Name;
        static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public static void Start(string name)
        {
            Name = name;
            sw.Reset();
            sw.Start();
        }
        public static long Restar(string name,bool showLog = true)
        {
            Name = name;
            var ret = Stop(showLog);
            sw.Restart();
            return ret;
        }
        //Milliseconds
        public static long Stop(bool showLog=true)
        {
            sw.Stop();
            long times = sw.ElapsedMilliseconds;
            if (showLog && UnityEngine.Application.isEditor)
            {
                CLog.Yellow($"Stopwatch[{Name}]：{times} ms");
            }
            return times;
        }
    }
}
