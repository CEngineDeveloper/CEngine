using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CYM
{
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
        private static Dictionary<string, MethodInfo> methodInfoCache;
        private static Dictionary<string, MethodInfo> methodUpdateCache;
        public static void ExecuteCommand(string methodName, params string[] args)
        {
            if (methodInfoCache == null || !methodInfoCache.ContainsKey(methodName))
            {
                Debug.LogWarning($"UnityConsole: Command `{methodName}` is not registered in the database.");
                return;
            }
            var methodInfo = methodInfoCache[methodName];
            var parametersInfo = methodInfo.GetParameters();
            if (parametersInfo.Length != args.Length)
            {
                Debug.LogWarning($"UnityConsole: Command `{methodName}` requires {parametersInfo.Length} args, while {args.Length} were provided.");
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
            
            List<Assembly> data = new List<Assembly>() { Starter.Assembly,Starter.AssemblyFirstpass };
            methodInfoCache = commands ?? data
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.GetCustomAttribute<ConsoleCommandAttribute>() != null)
                .ToDictionary(method => method.GetCustomAttribute<ConsoleCommandAttribute>().Alias ?? method.Name, StringComparer.OrdinalIgnoreCase);
        }
        internal static void RegisterUpdateCommands(Dictionary<string, MethodInfo> commands = null)
        {
            List<Assembly> data = new List<Assembly>() { Starter.Assembly, Starter.AssemblyFirstpass };
            methodUpdateCache = commands ?? data
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.GetCustomAttribute<ConsoleUpdateAttribute>() != null)
                .ToDictionary(method => method.GetCustomAttribute<ConsoleUpdateAttribute>().Alias ?? method.Name, StringComparer.OrdinalIgnoreCase);
        }
    }

    [HideMonoScript]
    public partial class SysConsole : BaseCoreMono
    {
        // To prevent garbage when the console is hidden.
        private class OnGUIProxy : MonoBehaviour
        {
            public Action OnGUIDelegate;
            private void OnGUI () => OnGUIDelegate();
        }

        #region prop
        private const int height = 25;
        private const string inputControlName = "input";
        private readonly char[] separator = { ' ' };
        private readonly List<string> inputBuffer = new List<string>();
        private OnGUIProxy guiProxy;
        private GUIStyle style;
        private bool setFocusPending;
        private string input;
        private int inputBufferIndex = 0;
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Awake()
        {
            base.Awake();
            Ins = this;
        }
        public static void Initialize(Dictionary<string, MethodInfo> commands = null)
        {
            if (Ins==null) return;

            CommandDatabase.RegisterCommands(commands);
            CommandDatabase.RegisterUpdateCommands();


            Ins.style = new GUIStyle
            {
                normal = new GUIStyleState { background = Texture2D.whiteTexture, textColor = Color.white },
                contentOffset = new Vector2(5, 5),
            };

            Ins.guiProxy = Ins.gameObject.AddComponent<OnGUIProxy>();
            Ins.guiProxy.OnGUIDelegate = Ins.DrawGUI;
            Ins.guiProxy.enabled = false;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!Application.isPlaying) return;
            if (Ins.guiProxy == null) return;

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
        }
        #endregion

        #region private
        private bool MultitouchDetected ()
        {
            if (!ToggleByMultitouch) return false;
            return Input.touchCount > 2 && Input.touches.Any(touch => touch.phase == TouchPhase.Began);
        }
        private void DrawGUI ()
        {
            if (Event.current.isKey && Event.current.keyCode == ToggleKey)
            {
                Hide();
                return;
            }

            GUI.backgroundColor = new Color(0, 0, 0, .65f);

            GUI.SetNextControlName(inputControlName);
            input = GUI.TextField(new Rect(0, 0, Screen.width - 125, height), input, style);
            if (GUI.Button(new Rect(Screen.width - 125, 0, 75, height), "EXECUTE", style)) ExecuteInput();
            if (GUI.Button(new Rect(Screen.width - 050, 0, 50, height), "HIDE", style)) Hide();

            if (setFocusPending)
            {
                GUI.FocusControl(inputControlName);
                setFocusPending = false;
            }

            if (GUI.GetNameOfFocusedControl() == inputControlName) HandleGUIInput();
        }
        private void HandleGUIInput ()
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
        private void ExecuteInput ()
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var preprocessedInput = InputPreprocessor.PreprocessInput(input);
            if (string.IsNullOrWhiteSpace(preprocessedInput)) return;

            var command = preprocessedInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (command.Length == 0) return;
            if (command.Length == 1) CommandDatabase.ExecuteCommand(command[0]);
            else CommandDatabase.ExecuteCommand(command[0], command.ToList().GetRange(1, command.Length - 1).ToArray());
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
            if (Ins.guiProxy == null)
                return false;
            return Ins.guiProxy.enabled;
        }
        public static void Show() => Ins.guiProxy.enabled = true;

        public static void Hide() => Ins.guiProxy.enabled = false;

        public static void Toggle() => Ins.guiProxy.enabled = !Ins.guiProxy.enabled;

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
        /// <summary>
        /// The key to toggle console visibility.
        /// </summary>
        [SerializeField,PropertyOrder(-100)]
        KeyCode ToggleKey = KeyCode.F1;
        [FoldoutGroup("Core")]
        public bool IsNoPlot = false;
        [FoldoutGroup("Core")]
        public bool IsIgnoreCondition = false;
        #endregion
    }
}
