
using Sirenix.OdinInspector;
using UnityEngine;
namespace CYM
{
	[HideMonoScript]
	public sealed class FPSCounter : BaseCoreMono
	{
		[SerializeField] bool EditorOnly = true;

		[SerializeField] private bool _autoShow = true;
		[SerializeField] private Anchor _anchor;
		[SerializeField] [HideInInspector]private float _updateInterval = 1f;
		[SerializeField] [Range(1,120)]private int _targetFrameRate = 80;
		[SerializeField] private int _fontSize = 32;

#pragma warning disable 0649
		//[Separator]

		[SerializeField] private int _xOffset;
		[SerializeField] private int _yOffset;
#pragma warning restore 0649

		public static bool IsShow { get; private set; } = false;
		/// <summary>
		/// Skip some time at start to skip performance drop on game start
		/// and produce more accurate Avg FPS
		/// </summary>
		private float _idleTime = 2;

		private float _elapsed;
		private int _frames;
		private int _quantity;
		private float _fps;
		private float _averageFps;

		private Color _goodColor;
		private Color _okColor;
		private Color _badColor;

		private float _okFps;
		private float _badFps;

		private Rect _rect1;
		private Rect _rect2;

        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
			NeedUpdate = true;
			NeedGUI = true;
        }
        public override void Awake()
        {
            base.Awake();
			if (EditorOnly && !Application.isEditor) return;
			//Application.targetFrameRate = _targetFrameRate;
			IsShow = _autoShow;
			_goodColor = new Color(.0f, 1f, .0f);
			_okColor = new Color(.8f, .8f, .2f, .6f);
			_badColor = new Color(.8f, .6f, .6f);

			//var percent = _targetFrameRate / 100;
			//var percent10 = percent * 10;
			//var percent40 = percent * 40;
			_okFps = 60;
			_badFps = 30;

			var xPos = 0;
			var yPos = 0;
			var linesHeight = 40;
			var linesWidth = 90;
			if (_anchor == Anchor.LeftBottom || _anchor == Anchor.RightBottom) yPos = Screen.height - linesHeight;
			if (_anchor == Anchor.RightTop || _anchor == Anchor.RightBottom) xPos = Screen.width - linesWidth;
			xPos += _xOffset;
			yPos += _yOffset;
			var yPos2 = yPos + 18;
			_rect1 = new Rect(xPos, yPos, linesWidth, linesHeight);
			_rect2 = new Rect(xPos, yPos2, linesWidth, linesHeight);

			_elapsed = _updateInterval;
			FPSStyle.fontSize = _fontSize;
			Application.targetFrameRate = _targetFrameRate;
		}
        public override void OnUpdate()
        {
            base.OnUpdate();
			if (EditorOnly && !Application.isEditor) return;
			if (!IsShow) return;
			if (_idleTime > 0)
			{
				_idleTime -= Time.deltaTime;
				return;
			}

			_elapsed += Time.deltaTime;
			++_frames;

			if (_elapsed >= _updateInterval)
			{
				_fps = _frames / _elapsed;
				_elapsed = 0;
				_frames = 0;
			}

			_quantity++;
			_averageFps += (_fps - _averageFps) / _quantity;
		}
		GUIStyle FPSStyle = new GUIStyle();
        public override void OnGUIPaint()
        {
            base.OnGUIPaint();
			if (EditorOnly && !Application.isEditor) return;
			if (!IsShow) return;
			if (BaseGlobal.LoaderMgr != null && !BaseGlobal.LoaderMgr.IsLoadEnd) return;
			var defaultColor = GUI.color;
			var color = _goodColor;
			if (_fps <= _okFps || _averageFps <= _okFps) color = _okColor;
			if (_fps <= _badFps || _averageFps <= _badFps) color = _badColor;
			GUI.color = color;
			FPSStyle.normal.textColor = color;
			GUI.Label(_rect1, ((int)_fps).ToString(), FPSStyle);
			GUI.color = defaultColor;
		}

		private enum Anchor
		{
			LeftTop, LeftBottom, RightTop, RightBottom
		}

		public static void Show(bool b)
		{
			IsShow = b;
		}
		public static void Toggle()
		{
			Show(!IsShow); 
		}
	}

}