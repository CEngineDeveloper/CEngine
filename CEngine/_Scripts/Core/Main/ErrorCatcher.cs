using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM
{
    [HideMonoScript]
    public class ErrorCatcher : MonoBehaviour
    {
        public static ErrorCatcher Ins { get; private set; }

        class LogMessage
        {
            public string Message;
            public string StackTrace;

            public LogMessage(string msg,string trace)
            {
                Message = msg;
                StackTrace = trace;
            }

            public string GetError()
            {
                return Message + "\n" + StackTrace;
            }
        }

        public enum LogAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Range(0.3f, 1.0f)]
        public float Height = 1.0f;
        [Range(0.3f, 1.0f)]
        public float Width = 0.5f;
        public bool IsSendAchieve = true;

        public int Margin = 20;

        public LogAnchor AnchorPosition = LogAnchor.BottomLeft;

        public int FontSize = 14;

        [Range(0f, 01f)]
        public float BackgroundOpacity = 0.0f;
        public Color BackgroundColor = Color.black;
        public Color ErrorColor = new Color(1, 0.0f, 0.0f);

        static Queue<LogMessage> showQueue = new Queue<LogMessage>();
        static Queue<LogMessage> cacheQueue = new Queue<LogMessage>();

        GUIStyle styleContainer, styleTextStackTrace, styleTextMessage;
        int padding = 5;

        private bool destroying = false;
        private bool styleChanged = true;
        private bool dirtyCloseError = false;
        private bool dirtyCloseSafe = false;
        private string customDesc = "";

        public static bool IsShow => showQueue.Count > 0;

        public void Awake()
        {
            Ins = this;
            InitStyles();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            styleChanged = true;
        }

        private void InitStyles()
        {
            Texture2D back = new Texture2D(1, 1);
            BackgroundColor.a = BackgroundOpacity;
            back.SetPixel(0, 0, BackgroundColor);
            back.Apply();

            styleContainer = new GUIStyle();
            styleContainer.normal.background = back;
            styleContainer.wordWrap = false;
            styleContainer.padding = new RectOffset(padding, padding, padding, padding);

            styleTextMessage = new GUIStyle();
            styleTextMessage.fontSize = 20;
            styleTextMessage.normal.textColor = Color.red;

            styleTextStackTrace = new GUIStyle();
            styleTextStackTrace.fontSize = FontSize;
            styleTextStackTrace.normal.textColor = Color.yellow;

            styleChanged = false;
        }

        void OnEnable()
        {
            showQueue = new Queue<LogMessage>();
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            if (destroying) return;
            Application.logMessageReceived -= HandleLog;
        }

        void Update()
        {
            if (showQueue != null && cacheQueue != null)
            {
                float InnerHeight = (Screen.height - 2 * Margin) * Height - 2 * padding;
                int TotalRows = (int)(InnerHeight / styleTextStackTrace.lineHeight);

                if (showQueue.Count > 0)
                {
                    while (showQueue.Count > TotalRows)
                        showQueue.Dequeue();
                }
                if (cacheQueue.Count > 0)
                { 
                    while (cacheQueue.Count > 500)
                        cacheQueue.Dequeue();
                }
            }
        }
        private void FixedUpdate()
        {
            if (dirtyCloseError)
            {
                //写入本地
                FileUtil.WriteFile(SysConst.Path_Dev + "/error.txt", GetErrorString());
                Feedback.SendError(customDesc, "None",IsSendAchieve);
                showQueue.Clear();
                cacheQueue.Clear();
                dirtyCloseError = false;
            }
            if (dirtyCloseSafe)
            {
                showQueue.Clear();
                cacheQueue.Clear();
                dirtyCloseSafe = false;
            }
        }

        void OnGUI()
        {
            if (!BuildConfig.IsWindows)
                return;
            //if (Application.isEditor)
            //    return;
            if (!IsShow) return;
            if (styleChanged) InitStyles();

            float w = (Screen.width - 2 * Margin) * Width;
            float h = (Screen.height - 2 * Margin) * Height;
            float x = 1, y = 1;

            switch (AnchorPosition)
            {
                case LogAnchor.BottomLeft:
                    x = Margin;
                    y = Margin + (Screen.height - 2 * Margin) * (1 - Height);
                    break;

                case LogAnchor.BottomRight:
                    x = Margin + (Screen.width - 2 * Margin) * (1 - Width);
                    y = Margin + (Screen.height - 2 * Margin) * (1 - Height);
                    break;

                case LogAnchor.TopLeft:
                    x = Margin;
                    y = Margin;
                    break;

                case LogAnchor.TopRight:
                    x = Margin + (Screen.width - 2 * Margin) * (1 - Width);
                    y = Margin;
                    break;
            }

            GUILayout.BeginArea(new Rect(x, y, Screen.width, Screen.height), styleContainer);
            if (GUILayout.Button("Send", GUILayout.Width(300)))
            {
                dirtyCloseError = true;
            }
            else if (GUILayout.Button("Clear", GUILayout.Width(300)))
            {
                dirtyCloseSafe = true;
            }
            
            foreach (LogMessage m in showQueue)
            {
                GUILayout.Label(m.Message, styleTextMessage);
                GUILayout.TextArea(m.StackTrace,styleTextStackTrace);
            }
            GUILayout.EndArea();
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            if (!BuildConfig.IsWindows)
                return;
            if (SysConsole.IsShow())
                return;
            if (BaseGlobal.DiffMgr!=null && 
                BaseGlobal.DiffMgr.IsSettedGMMod())
                return;
            if (showQueue.Count >= 1)
                return;
            if (type != LogType.Assert &&
                type != LogType.Error &&
                type != LogType.Exception) 
                return;

            showQueue.Enqueue(new LogMessage(message , stackTrace));
            cacheQueue.Enqueue(new LogMessage(message , stackTrace));
        }

        public static string GetErrorString()
        {
            string ret = "";
            foreach (var item in cacheQueue)
            {
                ret += item.GetError() + "\n";
            }
            return ret;
        }
        public static string GetTitle()
        {
            if (cacheQueue == null || cacheQueue.Count <= 0)
                return "None";
            return cacheQueue.Dequeue().Message;
        }
    }
}