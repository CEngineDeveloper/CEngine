using CYM.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace CYM
{
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
    /// <summary>
    /// Registers a public static method with supported argument types as a console command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConsoleCommandAttribute : Attribute 
    {
        /// <summary>
        /// When provided, alias is used instead of method name to reference the command.
        /// </summary>
        public string Alias { get; }

        public ConsoleCommandAttribute(string alias = null)
        {
            Alias = alias;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConsoleUpdateAttribute : Attribute
    {
        /// <summary>
        /// When provided, alias is used instead of method name to reference the command.
        /// </summary>
        public string Alias { get; }

        public ConsoleUpdateAttribute(string alias = null)
        {
            Alias = alias;
        }
    }

    /// <summary>
    /// Allows injecting delegates to modify the console input before it's send for execution to the <see cref="CommandDatabase"/>.
    /// </summary>
    public static class InputPreprocessor
    {
        private static readonly HashSet<Func<string, string>> preprocessors = new HashSet<Func<string, string>>();

        /// <summary>
        /// Executes all the added preprocessors in order on the provided input string and returns the resulting string.
        /// In case null is returned from a processor, the input would not be processed further.
        /// </summary>
        public static string PreprocessInput(string input)
        {
            if (input is null) return null;

            var result = input;
            foreach (var preprocessor in preprocessors)
            {
                result = preprocessor.Invoke(result);
                if (result is null) return null;
            }

            return result;
        }

        /// <summary>
        /// Adds the provided delegate as the input preprocessor.
        /// The delegate will be invoked before processing the console input.
        /// The only argument is the console input string. The return is the result of the preprocessing.
        /// When null is retured, the input won't be processed further.
        /// </summary>
        public static bool AddPreprocessor(Func<string, string> preprocessor)
        {
            return preprocessors.Add(preprocessor);
        }

        /// <summary>
        /// Removes the provided preprocessor from the preprocessors list.
        /// </summary>
        public static bool RemovePreprocessor(Func<string, string> preprocessor)
        {
            return preprocessors.Remove(preprocessor);
        }

        /// <summary>
        /// Removes all the added preprocessors from the preprocessors list.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetPreprocessor() => preprocessors.Clear();
    }

    internal static class CommandDatabase
    {
        //[RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            KeyConsole = BuildConfig.Ins.NameSpace + ".Console";
            KeySysConsole = "CYM.SysConsole";
        }

        static string KeyConsole = "";
        static string KeySysConsole = "";
        private static Dictionary<string, MethodInfo> methodInfoCache;
        private static Dictionary<string, MethodInfo> methodUpdateCache;
        public static void ExecuteCommand(string methodName, params string[] args)
        {
            if (methodInfoCache == null || !methodInfoCache.ContainsKey(methodName))
            {
                Debug.LogError($"UnityConsole: Command `{methodName}` is not registered in the database.");
                return;
            }
            var methodInfo = methodInfoCache[methodName];
            var parametersInfo = methodInfo.GetParameters();
            if (parametersInfo.Length != args.Length)
            {
                Debug.LogError($"UnityConsole: Command `{methodName}` requires {parametersInfo.Length} args, while {args.Length} were provided.");
                return;
            }
            var parameters = new object[parametersInfo.Length];
            for (int i = 0; i < args.Length; i++)
                parameters[i] = Convert.ChangeType(args[i], parametersInfo[i].ParameterType, System.Globalization.CultureInfo.InvariantCulture);
            methodInfo.Invoke(null, parameters);
        }
        public static void Update()
        {
            foreach (var item in methodUpdateCache)
            {
                item.Value.Invoke(null, null);
            }
        }

        internal static void RegisterCommands(Dictionary<string, MethodInfo> commands = null)
        {
            var types = new List<Type> { Starter.Assembly.GetType(KeyConsole), Starter.AssemblyFirstpass.GetType(KeySysConsole) };
            var mathond = types.SelectMany(t=>t.GetMethods(BindingFlags.Static | BindingFlags.Public));
            methodInfoCache = mathond.Where(method => method.GetCustomAttribute<ConsoleCommandAttribute>() != null)
                .ToDictionary(method => method.GetCustomAttribute<ConsoleCommandAttribute>().Alias ?? method.Name, StringComparer.OrdinalIgnoreCase);
        }
        internal static void RegisterUpdateCommands(Dictionary<string, MethodInfo> commands = null)
        {
            var types = new List<Type> { Starter.Assembly.GetType(KeyConsole), Starter.AssemblyFirstpass.GetType(KeySysConsole) };
            var mathond = types.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public));
            methodUpdateCache = mathond.Where(method => method.GetCustomAttribute<ConsoleUpdateAttribute>() != null)
                .ToDictionary(method => method.GetCustomAttribute<ConsoleUpdateAttribute>().Alias ?? method.Name, StringComparer.OrdinalIgnoreCase);
        }
    }

    [HideMonoScript]
    public partial class SysConsole : BaseCoreMono
    {
        #region prop
        float scrollbaclVal;
        const int height = 25;
        const string inputControlName = "input";
        readonly char[] separator = { ' ' };
        readonly List<string> inputBuffer = new List<string>();
        static bool isShow = false;
        static GUIStyle Dev_style;
        bool setFocusPending;
        string input;
        int inputBufferIndex = 0;
        #endregion

        #region Log
        static GUIStyle Log_styleContainer, Log_styleText;
        static Queue<LogMessage> Log_queue = new Queue<LogMessage>();
        LogAnchor AnchorPosition = LogAnchor.TopLeft;
        Vector2 scrollPos;
        float LogHeight = 0.5f;
        float LogWidth = 1f;
        int LogMargin = 20;
        bool LogMessages = true;
        bool LogWarnings = true;
        bool LogErrors = true;
        Color MessageColor = Color.white;
        Color WarningColor = Color.yellow;
        Color ErrorColor = new Color(1, 0.5f, 0.5f);
        bool StackTraceMessages = false;
        bool StackTraceWarnings = false;
        bool StackTraceErrors = true;
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedGUI = true;
        }
        public override void Awake()
        {
            base.Awake();
            Ins = this;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Application.logMessageReceived += HandleLog;
        }
        public override void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
            base.OnDisable();
        }
        public static void Initialize()
        {
            if (Ins==null) return;
            Log_queue = new Queue<LogMessage>();
            Texture2D back = new Texture2D(1, 1);
            back.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
            back.Apply();

            Log_styleContainer = new GUIStyle();
            Log_styleContainer.normal.background = back;
            Log_styleContainer.wordWrap = false;
            Log_styleContainer.padding = new RectOffset(5, 5, 5, 5);
            Log_styleContainer.fixedWidth = Screen.width;
            Log_styleText = new GUIStyle();
            Log_styleText.fontSize = 14;

            Dev_style = new GUIStyle
            {
                normal = new GUIStyleState { background = back, textColor = Color.white },
                padding = new RectOffset(5, 5, 5, 5),
                contentOffset = new Vector2(5, 0),
                fixedWidth = Screen.width * 0.75f,
                fixedHeight = 30,
            };
            CommandDatabase.Init();
            CommandDatabase.RegisterCommands();
            CommandDatabase.RegisterUpdateCommands();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!Application.isPlaying) return;

            if (Input.GetKeyUp(ToggleKey) || MultitouchDetected())
            {
                Toggle();
                setFocusPending = true;
            }

            if (IsGMMode ||
                Application.isEditor)
            {
                CommandDatabase.Update();
            }

            UpdateLog();
        }
        public override void OnGUIPaint()
        {
            base.OnGUIPaint();
            if (BuildConfig.Ins.IsShowConsoleBnt &&
                BuildConfig.Ins.BuildType == BuildType.Develop)
            {
                if (GUI.Button(new Rect(0, 0, 70, height), "Console"))
                {
                    Toggle();
                }
            }

            if (Event.current.isKey && Event.current.keyCode == ToggleKey)
            {
                Hide();
                return;
            }

            if (IsShow())
            {
                GUILayout.BeginVertical();
                DrawLog();
                DrawConsole();
                GUILayout.EndVertical();
            }
        }

        #endregion

        #region Draw
        void DrawLog()
        {
            float w = (Screen.width - 2 * LogMargin) * LogWidth;
            float h = (Screen.height - 2 * LogMargin) * LogHeight;
            float x = 1, y = 1;

            switch (AnchorPosition)
            {
                case LogAnchor.BottomLeft:
                    x = LogMargin;
                    y = LogMargin + (Screen.height - 2 * LogMargin) * (1 - LogHeight);
                    break;

                case LogAnchor.BottomRight:
                    x = LogMargin + (Screen.width - 2 * LogMargin) * (1 - LogWidth);
                    y = LogMargin + (Screen.height - 2 * LogMargin) * (1 - LogHeight);
                    break;

                case LogAnchor.TopLeft:
                    x = LogMargin;
                    y = LogMargin;
                    break;

                case LogAnchor.TopRight:
                    x = LogMargin + (Screen.width - 2 * LogMargin) * (1 - LogWidth);
                    y = LogMargin;
                    break;
            }

            GUILayout.BeginVertical(Log_styleContainer);
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
            //scrollbaclVal = GUILayout.VerticalScrollbar(scrollbaclVal, 100, 0, 100);
            foreach (LogMessage m in Log_queue)
            {
                switch (m.Type)
                {
                    case LogType.Warning:
                        Log_styleText.normal.textColor = WarningColor;
                        break;

                    case LogType.Log:
                        Log_styleText.normal.textColor = MessageColor;
                        break;

                    case LogType.Assert:
                    case LogType.Exception:
                    case LogType.Error:
                        Log_styleText.normal.textColor = ErrorColor;
                        break;

                    default:
                        Log_styleText.normal.textColor = MessageColor;
                        break;
                }

                GUILayout.Label(m.Message, Log_styleText);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        void DrawConsole()
        {
            GUILayout.BeginHorizontal(Log_styleContainer);
            GUI.backgroundColor = new Color(0, 0, 0, .7f);
            GUI.SetNextControlName(inputControlName);
            input = GUILayout.TextField(input, Dev_style);
            if (GUILayout.Button("EXECUTE",GUILayout.Height(30))) 
                ExecuteInput();
            if (GUILayout.Button("HIDE", GUILayout.Height(30))) 
                Hide();
            if (setFocusPending)
            {
                GUI.FocusControl(inputControlName);
                setFocusPending = false;
            }
            if (GUI.GetNameOfFocusedControl() == inputControlName) HandleConsoleGUIInput();
            GUILayout.EndHorizontal();
        }
        #endregion

        #region private
        void UpdateLog()
        {
            float InnerHeight = (Screen.height - 2 * LogMargin) * LogHeight - 2 * 5;
            int TotalRows = (int)(InnerHeight / Log_styleText.lineHeight);
            while (Log_queue.Count > TotalRows)
                Log_queue.Dequeue();
        }
        bool MultitouchDetected ()
        {
            if (!ToggleByMultitouch) return false;
            return Input.touchCount > 2 && Input.touches.Any(touch => touch.phase == TouchPhase.Began);
        }
        void HandleConsoleGUIInput ()
        {
            if (inputBuffer.Count > 0 && Event.current.isKey && Event.current.keyCode == KeyCode.UpArrow)
            {
                inputBufferIndex--;
                if (inputBufferIndex < 0) inputBufferIndex = inputBuffer.Count - 1;
                input = inputBuffer[inputBufferIndex];
            }

            if (inputBuffer.Count > 0 && Event.current.isKey && Event.current.keyCode == KeyCode.DownArrow)
            {
                inputBufferIndex++;
                if (inputBufferIndex >= inputBuffer.Count) inputBufferIndex = 0;
                input = inputBuffer[inputBufferIndex];
            }

            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                ExecuteInput();
                inputBuffer.Add(input);
                inputBufferIndex = 0;
                input = string.Empty;
                Hide();
            }
        }
        void ExecuteInput ()
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var preprocessedInput = InputPreprocessor.PreprocessInput(input);
            if (string.IsNullOrWhiteSpace(preprocessedInput)) return;

            var command = preprocessedInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (command.Length == 0) return;
            if (command.Length == 1) CommandDatabase.ExecuteCommand(command[0]);
            else CommandDatabase.ExecuteCommand(command[0], command.ToList().GetRange(1, command.Length - 1).ToArray());
        }
        void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Assert && !LogErrors) return;
            if (type == LogType.Error && !LogErrors) return;
            if (type == LogType.Exception && !LogErrors) return;
            if (type == LogType.Log && !LogMessages) return;
            if (type == LogType.Warning && !LogWarnings) return;

            string[] lines = message.Split(new char[] { '\n' });

            foreach (string l in lines)
                Log_queue.Enqueue(new LogMessage(l, type));

            if (type == LogType.Assert && !StackTraceErrors) return;
            if (type == LogType.Error && !StackTraceErrors) return;
            if (type == LogType.Exception && !StackTraceErrors) return;
            if (type == LogType.Log && !StackTraceMessages) return;
            if (type == LogType.Warning && !StackTraceWarnings) return;

            string[] trace = stackTrace.Split(new char[] { '\n' });

            foreach (string t in trace)
                if (t.Length != 0) Log_queue.Enqueue(new LogMessage("  " + t, type));
        }
        #endregion

        #region pub
        /// <summary>
        /// Whether to toggle console when multi-(3 or more) touch is detected.
        /// </summary>
        public static bool ToggleByMultitouch { get; set; } = true;
        public static SysConsole Ins { get; private set; }
        public static bool IsShow()
        {
            if (Ins == null)
                return false;
            return isShow;
        }
        public static void Show() => isShow = true;

        public static void Hide() => isShow = false;

        public static void Toggle() => isShow = !isShow;

        public static bool IsGMMode
        {
            get
            {
                if (BuildConfig.Ins.IsDevelop)
                    return true;
                if (BaseGlobal.DiffMgr == null)
                    return false;
                return BaseGlobal.DiffMgr.IsGMMode();
            }
        }
        #endregion

        #region Config
        [SerializeField,PropertyOrder(-100)]
        KeyCode ToggleKey = KeyCode.F1;
        [FoldoutGroup("Core")]
        public bool IsIgnoreCondition = false;
        [FoldoutGroup("Core")]
        public bool IsNoLoadLua = false;         //是否加载Lua脚本,有些情况需要屏蔽lua脚本
        #endregion
    }
}
