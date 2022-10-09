using UnityEngine;
namespace CYM.Cam
{
    public class RTSCamera2D : MonoBehaviour
    {
        #region bound
        public Rect bound;
        public Rect maxBound;
        float bound_yMax { get; set; }
        float bound_xMax { get; set; }
        float bound_yMin { get; set; }
        float bound_xMin { get; set; }
        AnimationCurve curve_bound_yMax;
        AnimationCurve curve_bound_xMax;
        AnimationCurve curve_bound_yMin;
        AnimationCurve curve_bound_xMin;
        #endregion

        #region speed
        public float DesktopScrollSpeed { get; set; }
        public float DesktopMoveSpeed { get; set; }
        public float DesktopMoveDragSpeed { get; set; }

        public float TouchScrollSpeed { get; set; }
        public float TouchMoveSpeed { get; set; }
        public float TouchMoveDragSpeed { get; set; }

        public bool IsControl { get; private set; } = true;
        #endregion

        #region Output param
        public float ScrollValue { get; private set; }
        public float desktopScrollSpeed;
        public float desktopMoveSpeed;
        public float desktopMoveDragSpeed;

        public float touchScrollSpeed;
        public float touchMoveSpeed;
        public float touchMoveDragSpeed;
        public float touchScreenEdgeWidth = 1;

        public bool unlockWhenMove;
        public Range zoomRange;
        public float moveLerpSpeed;
        #endregion

        #region const
        const int mouseDragButton = 0;
        const string MouseScrollWheel = "Mouse ScrollWheel";
        const string MouseX = "Mouse X";
        const string MouseY = "Mouse Y";
        #endregion

        #region prop
        private Transform followingTarget;
        private RTSCamera2D self;
        private Transform selfTrans;
        private Camera selfCamera;
        private Vector2 objectPos;
        bool isLastClickUI = false;
        bool isDragOut = false;

        bool rotateControl;
        bool screenEdgeMoveControl;
        bool dragControl;
        bool scrollControl;
        #endregion

        #region runtime_control_switch
        public void ScreenEdgeMoveControl(bool enable) => screenEdgeMoveControl = enable;
        public void DragControl(bool enable) => dragControl = enable;
        public void RotateControl(bool enable) => rotateControl = enable;
        public void ScrollControl(bool enable) => scrollControl = enable;
        public void AllControl(bool enable) => IsControl = enable;

        public void ScreenEdgeSpeed(float val)
        {
            DesktopMoveSpeed = desktopMoveSpeed * val;
            TouchMoveSpeed = touchMoveSpeed * val;
        }
        public void DragSpeed(float val)
        {
            DesktopMoveDragSpeed = desktopMoveDragSpeed * val;
            TouchMoveDragSpeed = touchMoveDragSpeed * val;
        }
        public void ScrollSpeed(float val)
        {
            DesktopScrollSpeed = desktopScrollSpeed * val;
            TouchScrollSpeed = touchScrollSpeed * val;
        }
        #endregion

        #region life
        void Awake()
        {
            self = this;
            selfTrans = transform;
            selfCamera = GetComponent<Camera>();
            curve_bound_yMax = new AnimationCurve(new Keyframe(0, bound.yMax), new Keyframe(1, maxBound.yMax));
            curve_bound_xMax = new AnimationCurve(new Keyframe(0, bound.xMax), new Keyframe(1, maxBound.xMax));
            curve_bound_yMin = new AnimationCurve(new Keyframe(0, bound.yMin), new Keyframe(1, maxBound.yMin));
            curve_bound_xMin = new AnimationCurve(new Keyframe(0, bound.xMin), new Keyframe(1, maxBound.xMin));
        }

        public void Start()
        {
            objectPos = CalculateCurrentObjectPosition();
            ScrollValue = Mathf.Clamp01(ScrollValue);
        }
        private void Update()
        {
            UpdateMouse();
            UpdateTouch();
            UpdateTransform();
            UpdateBound();
        }
        void UpdateTransform()
        {
            Vector3 cameraPos;

            if (followingTarget)
            {
                objectPos.x = followingTarget.position.x;
                objectPos.y = followingTarget.position.y;
            }

            float wantedSize = zoomRange.Min + (ScrollValue * zoomRange.Length);
            selfCamera.orthographicSize = Mathf.Lerp(selfCamera.orthographicSize, wantedSize, moveLerpSpeed * Time.deltaTime);

            cameraPos = objectPos;
            cameraPos.z = selfCamera.transform.position.z;
            selfTrans.position = Vector3.Lerp(selfTrans.position, cameraPos, moveLerpSpeed * Time.deltaTime);
        }
        void UpdateBound()
        {
            bound_xMax = curve_bound_xMax.Evaluate(ScrollValue);
            bound_yMax = curve_bound_yMax.Evaluate(ScrollValue);
            bound_xMin = curve_bound_xMin.Evaluate(ScrollValue);
            bound_yMin = curve_bound_yMin.Evaluate(ScrollValue);
        }
        void UpdatePosClamp()
        {
            objectPos.x = Mathf.Clamp(objectPos.x, bound_xMin, bound_xMax);
            objectPos.y = Mathf.Clamp(objectPos.y, bound_yMin, bound_yMax);
        }
        void UpdateTouch()
        {
            if (!Application.isMobilePlatform) return;
            if (Application.isEditor) return;
            Vector2 dV = BaseInputMgr.TouchDragValue;
            if (dragControl && dV != Vector2.zero)
            {
                Move(dV.y * Vector2.up, TouchMoveDragSpeed * 100 * Time.deltaTime);
                Move(dV.x * Vector2.right, TouchMoveDragSpeed * 100 * Time.deltaTime);
            }
            if (scrollControl && BaseInputMgr.TouchScaleValue != 0)
            {
                Scroll(BaseInputMgr.TouchScaleValue * TouchScrollSpeed * 100);
            }
            if (screenEdgeMoveControl && BaseInputMgr.TouchPosition != Vector3.zero)
            {
                float speedFaction = 1.0f;
                if (BaseInputMgr.TouchPosition.y >= Screen.height - touchScreenEdgeWidth) { Move(Vector2.up, TouchMoveDragSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.y <= touchScreenEdgeWidth) { Move(-Vector2.up, TouchMoveDragSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.x <= touchScreenEdgeWidth) { Move(-Vector2.right, TouchMoveDragSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.x >= Screen.width - touchScreenEdgeWidth) { Move(Vector2.right, TouchMoveDragSpeed * Time.deltaTime * speedFaction); }
            }
        }
        void UpdateMouse()
        {
            if (Application.isMobilePlatform) return;

            if (Input.GetMouseButtonDown(mouseDragButton))
                isLastClickUI = BaseInputMgr.IsStayInUIWithoutHUD;
            if (Input.GetMouseButtonUp(mouseDragButton))
                isLastClickUI = false;

            if (scrollControl)
            {
                Scroll(Input.GetAxis(MouseScrollWheel) * -DesktopScrollSpeed);
            }

            if (screenEdgeMoveControl)
            {
                if (!IsMouseButton())
                {
                    if (isDragOut && !IsInScreenEdge())
                        isDragOut = false;

                    float speedFaction = 1.0f;
                    if (isDragOut)
                        speedFaction = 0.05f;

                    if (Input.mousePosition.y >= Screen.height - 1f) { Move(Vector2.up, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.y <= 0) { Move(-Vector2.up, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x <= 0) { Move(-Vector2.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x >= Screen.width - 1f) { Move(Vector2.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                }
            }

            if (dragControl)
            {
                if (Input.GetMouseButton(mouseDragButton))
                {
                    if (!isLastClickUI)
                    {
                        float mouseX = Input.GetAxis(MouseX) / Screen.width * 10000f;
                        float mouseY = Input.GetAxis(MouseY) / Screen.height * 10000f;
                        Move(-Vector2.right * mouseX, DesktopMoveDragSpeed * Time.deltaTime);
                        Move(-Vector2.up * mouseY, DesktopMoveDragSpeed * Time.deltaTime);

                        if (IsInScreenEdge())
                            isDragOut = true;
                        else
                            isDragOut = false;
                    }
                }
            }
        }
        #endregion

        #region set
        public void JumpTo(Vector2 pos)
        {
            self.objectPos.x = pos.x;
            self.objectPos.y = pos.y;
        }
        public void JumpTo(Transform target)
        {
            JumpTo(target.position);
        }
        public void Follow(Transform target)
        {
            self.followingTarget = target;
        }
        public void CancelFollow()
        {
            self.followingTarget = null;
        }
        public void Move(Vector2 dir, float offset)
        {
            if (!IsControl) return;
            dir *= offset;
            if (unlockWhenMove && dir != Vector2.zero)
            {
                followingTarget = null;
            }
            objectPos += dir;
            UpdatePosClamp();
        }

        public void Scroll(float value)
        {
            if (!IsControl) return;
            ScrollValue += value;
            ScrollValue = Mathf.Clamp01(ScrollValue);
            UpdatePosClamp();
        }
        public void SetScroll(float val)
        {
            if (!IsControl) return;
            ScrollValue = val;
            UpdatePosClamp();
        }
        #endregion

        #region get
        public Transform GetFollowingTarget() { return followingTarget; }
        #endregion

        #region private
        Vector3 CalculateCurrentObjectPosition()
        {
            return transform.position;
        }
        void Adjust2AttitudeBaseOnCurrentSetting()
        {
            objectPos = CalculateCurrentObjectPosition();
            ScrollValue = Mathf.Clamp01(ScrollValue);

            GetComponent<Camera>().orthographicSize = zoomRange.Min + (ScrollValue * zoomRange.Length);
        }
        #endregion

        #region is
        bool IsMouseButton()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                return true;
            return false;
        }
        bool IsInScreenEdge()
        {
            if (Input.mousePosition.y >= Screen.height - 1f) return true;
            if (Input.mousePosition.y <= 1) return true;
            if (Input.mousePosition.x <= 1) return true;
            if (Input.mousePosition.x >= Screen.width - 1f) return true;
            return false;
        }
        public bool IsMoving
        {
            get
            {
                return
                    !MathUtil.Approximately(selfTrans.position.x, objectPos.x, 0.02f) ||
                    !MathUtil.Approximately(selfTrans.position.y, objectPos.y, 0.02f);
            }
        }
        #endregion

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMin, 0), new Vector3(bound.xMin, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMax, 0), new Vector3(bound.xMax, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMax, 0), new Vector3(bound.xMax, bound.yMin, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMin, 0), new Vector3(bound.xMin, bound.yMin, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(maxBound.xMin, maxBound.yMin, 0), new Vector3(maxBound.xMin, maxBound.yMax, 0));
            Gizmos.DrawLine(new Vector3(maxBound.xMin, maxBound.yMax, 0), new Vector3(maxBound.xMax, maxBound.yMax, 0));
            Gizmos.DrawLine(new Vector3(maxBound.xMax, maxBound.yMax, 0), new Vector3(maxBound.xMax, maxBound.yMin, 0));
            Gizmos.DrawLine(new Vector3(maxBound.xMax, maxBound.yMin, 0), new Vector3(maxBound.xMin, maxBound.yMin, 0));

            Gizmos.color = Color.white;
        }
    }
}