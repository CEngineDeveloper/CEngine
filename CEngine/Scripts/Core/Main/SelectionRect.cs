namespace CYM
{
    using UnityEngine;
    public class SelectionRect : BaseCoreMono
    {
        #region Inspector
        [SerializeField]
        Texture mouseTexture;
        [SerializeField]
        Color planeColor = new Color(0, 0.8f, 1, 0.35f);
        [SerializeField]
        Color borderColor = new Color(1, 1, 1, 0.3f);
        [SerializeField]
        float width = 1;
        #endregion

        #region prop
        static float startDeltaThreshold = 3.0f;
        static Vector3 _lastSelectDownPos;
        static Vector3 p1 = Vector3.zero;
        static Vector3 p2 = Vector3.zero;
        static bool isSelecting = false;
        static bool isStartSelecting = false;
        static bool IgnoreFrameSelection = false;
        static Timer IgnoreFrameSelectionTimer = new Timer();
        #endregion

        #region Callback
        public static Callback Callback_OnStartSelect { get; set; }
        public static Callback Callback_OnEndSelect { get; set; }
        public static Callback Callback_OnChangeSelect { get; set; }
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedGUI = true;
        }
        public override void OnUpdate()
        {
            if (IgnoreFrameSelectionTimer.Elapsed() > 0.01 && IgnoreFrameSelection)
            {
                IgnoreFrameSelection = false;
            }
            if (IgnoreFrameSelection)
                return;
            if (Input.GetMouseButtonDown(0))
            {
                StartSelect();
            }
            else if (Input.GetMouseButton(0) && isStartSelecting)
            {
                Vector3 selectRectPos = _lastSelectDownPos - Input.mousePosition;
                if (Mathf.Abs(selectRectPos.x) > 3.0f &&
                    Mathf.Abs(selectRectPos.y) > 3.0f)
                {
                    if (!IsSelecting)
                        RealStartSelect();
                    IsSelecting = true;
                }
                if (HasSelection(_lastSelectDownPos, Input.mousePosition))
                {
                }
            }
            else if (Input.GetMouseButtonUp(0) && IsSelecting)
            {
                EndSelect();
                IsSelecting = false;
            }
            if (Input.GetMouseButtonUp(0))
            {
                isStartSelecting = false;
            }
        }
        #endregion

        #region set
        protected virtual void StartSelect()
        {
            if (BaseInputMgr.IsStayInUI)
                return;
            isStartSelecting = true;
            _lastSelectDownPos = Input.mousePosition;
        }
        protected virtual void RealStartSelect()
        {
            if (Callback_OnStartSelect != null)
                Callback_OnStartSelect();
        }
        protected virtual void EndSelect()
        {
            if (Callback_OnEndSelect != null)
                Callback_OnEndSelect();
            if (Callback_OnChangeSelect != null)
                Callback_OnChangeSelect();
        }
        public override void OnGUIPaint()
        {
            base.OnGUIPaint();
            if (IsSelecting)
            {
                GUI.color = planeColor;
                GUI.DrawTexture(new Rect(_lastSelectDownPos.x, Screen.height - _lastSelectDownPos.y, Input.mousePosition.x - _lastSelectDownPos.x, _lastSelectDownPos.y - Input.mousePosition.y), mouseTexture);
                GUI.color = borderColor;
                GUI.DrawTexture(new Rect(_lastSelectDownPos.x - width, Screen.height - _lastSelectDownPos.y - width, Input.mousePosition.x - _lastSelectDownPos.x + 2 * width, _lastSelectDownPos.y - Input.mousePosition.y + 2 * width), mouseTexture);
            }
        }
        #endregion

        #region is
        bool HasSelection(Vector3 startScreen, Vector3 endScreen)
        {
            if ((Mathf.Abs(startScreen.x - endScreen.x) < startDeltaThreshold) || (Mathf.Abs(startScreen.y - endScreen.y) < startDeltaThreshold))
            {
                return false;
            }
            return true;
        }
        public static bool IsSelecting
        {
            get
            {
                return isSelecting;
            }

            set
            {
                isSelecting = value;
            }
        }
        #endregion

        #region is static
        public static bool IsSelected(BaseUnit go)
        {
            if (!IsSelecting)
                return false;
            p1 = Vector3.zero;
            p2 = Vector3.zero;
            if (_lastSelectDownPos.x > Input.mousePosition.x)
            {
                p1.x = Input.mousePosition.x;
                p2.x = _lastSelectDownPos.x;
            }
            else
            {
                p1.x = _lastSelectDownPos.x;
                p2.x = Input.mousePosition.x;
            }
            if (_lastSelectDownPos.y > Input.mousePosition.y)
            {
                p1.y = Input.mousePosition.y;
                p2.y = _lastSelectDownPos.y;
            }
            else
            {
                p1.y = _lastSelectDownPos.y;
                p2.y = Input.mousePosition.y;
            }
            Vector3 location = BaseGlobal.MainCamera.WorldToScreenPoint(go.transform.position);
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y
                || location.z < BaseGlobal.MainCamera.nearClipPlane || location.z > BaseGlobal.MainCamera.farClipPlane)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
