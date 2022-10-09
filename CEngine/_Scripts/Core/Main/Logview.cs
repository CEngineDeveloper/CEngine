using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class Logview : BaseCoreMono
    {        /// <summary>
             /// The key to toggle console visibility.
             /// </summary>
        [SerializeField]
        KeyCode ToggleKey = KeyCode.F2;

        class LogMessage
        {
            public string Message;
            public LogType Type;

            public LogMessage(string msg, LogType type)
            {
                Message = msg;
                Type = type;
            }
        }

        public enum LogAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [HideInInspector]
        public bool ShowLog = false;
        public bool EditorOnly = true;


        [Tooltip("Height of the log area as a percentage of the screen height")]
        [Range(0.3f, 1.0f)]
        public float Height = 0.5f;

        [Tooltip("Width of the log area as a percentage of the screen width")]
        [Range(0.3f, 1.0f)]
        public float Width = 0.5f;

        public int Margin = 20;

        public LogAnchor AnchorPosition = LogAnchor.BottomLeft;

        public int FontSize = 14;

        [Range(0f, 01f)]
        public float BackgroundOpacity = 0.5f;
        [HideInInspector] 
        public Color BackgroundColor = Color.black;

        [HideInInspector] public bool LogMessages = true;
        [HideInInspector] public bool LogWarnings = true;
        [HideInInspector] public bool LogErrors = true;

        [HideInInspector] public Color MessageColor = Color.white;
        [HideInInspector] public Color WarningColor = Color.yellow;
        [HideInInspector] public Color ErrorColor = new Color(1, 0.5f, 0.5f);

        [HideInInspector] public bool StackTraceMessages = false;
        [HideInInspector] public bool StackTraceWarnings = false;
        [HideInInspector] public bool StackTraceErrors = true;

        static Queue<LogMessage> queue = new Queue<LogMessage>();

        GUIStyle styleContainer, styleText;
        int padding = 5;

        private bool destroying = false;
		private bool styleChanged = true;

        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedGUI = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            InitStyles();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (EditorOnly && !Application.isEditor) return;
            queue = new Queue<LogMessage>();
            Application.logMessageReceived += HandleLog;
        }
        public override void OnDisable()
        {
            // If destroyed because already exists, don't need to de-register callback
            if (destroying) return;
            Application.logMessageReceived -= HandleLog;
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (EditorOnly && !Application.isEditor) return;

            if (Input.GetKeyDown(ToggleKey))
            {
                ShowLog = !ShowLog;
            }

            float InnerHeight = (Screen.height - 2 * Margin) * Height - 2 * padding;
            int TotalRows = (int)(InnerHeight / styleText.lineHeight);

            // Remove overflowing rows
            while (queue.Count > TotalRows)
                queue.Dequeue();
        }
        float scrollbaclVal=0.1f;
        public override void OnGUIPaint()
        {
            base.OnGUIPaint();
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) return;
            if (!ShowLog) return;
            if (EditorOnly && !Application.isEditor) return;

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

            GUILayout.BeginArea(new Rect(x, y, w, h), styleContainer);
            scrollPos = GUILayout.BeginScrollView(scrollPos,false,false);
            //scrollbaclVal = GUILayout.VerticalScrollbar(scrollbaclVal, 1, 0, 1);
            foreach (LogMessage m in queue)
            {
                switch (m.Type)
                {
                    case LogType.Warning:
                        styleText.normal.textColor = WarningColor;
                        break;

                    case LogType.Log:
                        styleText.normal.textColor = MessageColor;
                        break;

                    case LogType.Assert:
                    case LogType.Exception:
                    case LogType.Error:
                        styleText.normal.textColor = ErrorColor;
                        break;

                    default:
                        styleText.normal.textColor = MessageColor;
                        break;
                }

                GUILayout.Label(m.Message, styleText);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
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

            styleText = new GUIStyle();
            styleText.fontSize = FontSize;

			styleChanged = false;
		}

        Vector2 scrollPos;

        void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Assert && !LogErrors) return;
            if (type == LogType.Error && !LogErrors) return;
            if (type == LogType.Exception && !LogErrors) return;
            if (type == LogType.Log && !LogMessages) return;
            if (type == LogType.Warning && !LogWarnings) return;

            string[] lines = message.Split(new char[] { '\n' });

            foreach (string l in lines)
                queue.Enqueue(new LogMessage(l, type));

            if (type == LogType.Assert && !StackTraceErrors) return;
            if (type == LogType.Error && !StackTraceErrors) return;
            if (type == LogType.Exception && !StackTraceErrors) return;
            if (type == LogType.Log && !StackTraceMessages) return;
            if (type == LogType.Warning && !StackTraceWarnings) return;

            string[] trace = stackTrace.Split(new char[] { '\n' });

            foreach (string t in trace)
                if (t.Length != 0) queue.Enqueue(new LogMessage("  " + t, type));
        }

        public void InspectorGUIUpdated()
        {
			styleChanged = true;
		}
    }
}