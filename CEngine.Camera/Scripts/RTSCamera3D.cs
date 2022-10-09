using System.Collections.Generic;
using UnityEngine;
public enum MouseButton
{
    Left=0, 
    Right=1, 
    Middle=2
}
public enum RotateAxis
{ 
    X,
    Y,
    XY,
}
namespace CYM.Cam
{

    public class RTSCamera3D : MonoBehaviour
    {
        #region Output param
        public float DesktopMoveDragSpeed { get; private set; } = 10.0f;
        public float DesktopMoveSpeed { get; private set; } = 300;
        public float DesktopKeyMoveSpeed { get; private set; } = 2;
        public float DesktopScrollSpeed { get; private set; } = 5;
        public float DesktopRotateSpeed { get; private set; } = 5;

        public float TouchMoveDragSpeed { get; private set; } = 10;
        public float TouchMoveSpeed { get; private set; } = 300;
        public float TouchScrollSpeed { get; private set; } = 5;
        public float TouchRotateSpeed { get; private set; } = 5;

        public bool IsControl { get; private set; } = true;
        #endregion

        #region Param
        public AnimationCurve scrollXAngle = new AnimationCurve();
        public AnimationCurve scrollHigh = new AnimationCurve();
        public float maxHight = 20;
        public float minHight = 5;
        public float maxAngle = 45;
        public float minAngle = 45;
        public CamScrollAnimType scrollAnimationType;
        public bool groundHighTest;
        public float groundHighTestValMax = 125; //高于这个高度后进行插值
        public float groundHighTestValLerpSpeed = 10f; //高于这个高度后进行插值速度
        public LayerMask groundMask;
        public float seaLevel = 0; 
        public Rect bound;
        public bool unlockWhenMove;
        public float movementLerpSpeed;
        public float rotationLerpSpeed;

        public float desktopScrollSpeed;
        public float desktopMoveSpeed;
        public float desktopMoveDragSpeed;
        public float desktopRotateSpeed;
        public float deskScreenEdgeWidth = 1;

        public float touchScrollSpeed;
        public float touchMoveSpeed;
        public float touchMoveDragSpeed;
        public float touchRotateSpeed;
        public float touchScreenEdgeWidth = 1;

        public int mouseDragButton;
        public int mouseRotateButton;
        public RotateAxis rotateAxis = RotateAxis.Y;
        public float minXAxisRotateAngle = 25;
        public float desktopRotateDelay = 0;
        #endregion

        #region prop
        public float MaxHight { get; private set; }
        public float MinHight { get; private set; }
        public float ScrollValue { get; private set; }
        Vector3 objectPos;
        Transform followingTarget;
        Transform fixedPoint;
        Transform trans;
        float targetCurrentGroundHigh = 0;
        float wantYAngle;
        float wantXAngle;
        bool isLastClickUI = false;
        bool isDragOut = false;
        bool isInXRotate = false;
        bool rotateControl;
        bool screenEdgeMoveControl;
        bool dragControl;
        bool scrollControl;
        float curDesktopRotateDelayTime = 0;
        private RTSCamera3D self;
        const string KeyMouseScrollWheel = "Mouse ScrollWheel";
        const string KeyMouseX = "Mouse X";
        const string KeyMouseY = "Mouse Y";
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
        public void RotateSpeed(float val)
        {
            DesktopRotateSpeed = desktopRotateSpeed * val;
            TouchRotateSpeed = touchRotateSpeed * val;
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
            trans = transform;
        }
        void Start()
        {
            scrollHigh = new AnimationCurve(new Keyframe(0,minHight),new Keyframe(1,maxHight));
            scrollXAngle = new AnimationCurve(new Keyframe(0,minAngle),new Keyframe(1,maxAngle));

            objectPos = CalculateCurrentObjectPosition();
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);

            Vector3 rot = trans.eulerAngles;
            rot.x = MathUtil.WrapAngle(rot.x);
            rot.y = MathUtil.WrapAngle(rot.y);
            wantYAngle = rot.y;
            rot.x = scrollXAngle.Evaluate(ScrollValue);
            wantXAngle = rot.x;
            trans.eulerAngles = rot;
            if (scrollHigh != null)
            {
                MaxHight = scrollHigh.Evaluate(1.0f);
                MinHight = scrollHigh.Evaluate(0.0f);
            }
        }
        void Update()
        {
            UpdateMouseControl();
            UpdateTouchControl();
            UpdateTransform();
        }
        void UpdateTouchControl()
        {
            if (!Application.isMobilePlatform) return;
            if (Application.isEditor) return;
            Vector2 dV = BaseInputMgr.TouchDragValue;

            if (dragControl && dV != Vector2.zero)
            {
                Quaternion fixedDir = Quaternion.Euler(new Vector3(0, trans.eulerAngles.y, trans.eulerAngles.z));
                Vector3 forwardDir = fixedDir * Vector3.forward;
                _Move(dV.y * forwardDir, TouchMoveDragSpeed * Time.deltaTime);
                _Move(dV.x * trans.right, TouchMoveDragSpeed * Time.deltaTime);
            }

            if (scrollControl && BaseInputMgr.TouchScaleValue != 0) 
                Scroll(BaseInputMgr.TouchScaleValue * touchScrollSpeed);

            if (rotateControl && BaseInputMgr.TouchRotateValue != 0) 
                RotateY(BaseInputMgr.TouchRotateValue * touchRotateSpeed);

            if (screenEdgeMoveControl && BaseInputMgr.TouchPosition != Vector3.zero)
            {
                float speedFaction = 1.0f;
                if (BaseInputMgr.TouchPosition.y >= Screen.height - touchScreenEdgeWidth) { _Move(trans.forward, TouchMoveSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.y <= touchScreenEdgeWidth) { _Move(-trans.forward, TouchMoveSpeed * Time.deltaTime * speedFaction);  }
                if (BaseInputMgr.TouchPosition.x <= touchScreenEdgeWidth) { _Move(-trans.right, TouchMoveSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.x >= Screen.width - touchScreenEdgeWidth) { _Move(trans.right, TouchMoveSpeed * Time.deltaTime * speedFaction);  }
            }
        }
        void UpdateMouseControl()
        {
            if (Application.isMobilePlatform) 
                return;
            if (Input.GetMouseButton((int)MouseButton.Right))
            {
                curDesktopRotateDelayTime += Time.deltaTime;
            }
            if (Input.GetMouseButtonUp((int)MouseButton.Right))
            {
                curDesktopRotateDelayTime = 0;
            }

            if (Input.GetMouseButtonDown(mouseDragButton))
                isLastClickUI = BaseInputMgr.IsStayInUIWithoutHUD;
            if (Input.GetMouseButtonUp(mouseDragButton))
                isLastClickUI = false;

            //Screen move
            if (screenEdgeMoveControl)
            {
                if (!IsMouseButton())
                {
                    if (isDragOut && !IsInScreenEdge())
                        isDragOut = false;

                    float speedFaction = 1.0f;
                    if (isDragOut)
                        speedFaction = 0.05f;

                    if (Input.mousePosition.y >= Screen.height - deskScreenEdgeWidth) { _Move(trans.forward, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.y <= deskScreenEdgeWidth) { _Move(-trans.forward, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x <= deskScreenEdgeWidth) { _Move(-trans.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x >= Screen.width - deskScreenEdgeWidth) { _Move(trans.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                }
            }

            //Scroll
            if (scrollControl)
            {
                if (!isInXRotate)
                    Scroll(Input.GetAxis(KeyMouseScrollWheel) * -DesktopScrollSpeed);
            }

            //Rotate
            if (rotateControl)
            {
                if (!isLastClickUI)
                {
                    if (Input.GetMouseButton(mouseRotateButton) && curDesktopRotateDelayTime > desktopRotateDelay)
                    {
                        if (rotateAxis == RotateAxis.Y)
                        {
                            RotateY(Input.GetAxis(KeyMouseX) * desktopRotateSpeed);
                            isInXRotate = false;
                        }
                        else if (rotateAxis == RotateAxis.X)
                        {
                            RotateX(Input.GetAxis(KeyMouseY) * desktopRotateSpeed*0.1f);
                            isInXRotate = true;
                        }
                        else if (rotateAxis == RotateAxis.XY)
                        {
                            RotateY(Input.GetAxis(KeyMouseX) * desktopRotateSpeed);
                            RotateX(Input.GetAxis(KeyMouseY) * desktopRotateSpeed * 0.1f);
                            isInXRotate = true;
                        }
                        else
                        {
                            isInXRotate = false;
                        }
                        if (IsInScreenEdge()) isDragOut = true;
                        else isDragOut = false;
                    }
                    else if (Input.GetMouseButtonUp(mouseRotateButton))
                    {
                        isInXRotate = false;
                    }
                }
            }

            //Drag
            if (dragControl)
            {
                if (!isLastClickUI)
                {
                    if (Input.GetMouseButton(mouseDragButton))
                    {
                        float mouseX = Input.GetAxis(KeyMouseX) / Screen.width * 10000f;
                        float mouseY = Input.GetAxis(KeyMouseY) / Screen.height * 10000f;
                        _Move(-trans.right, DesktopMoveDragSpeed * mouseX * Time.deltaTime);
                        _Move(-trans.forward, DesktopMoveDragSpeed * mouseY * Time.deltaTime);

                        if (IsInScreenEdge()) isDragOut = true;
                        else isDragOut = false;
                    }
                }
            }
        }
        void UpdateTransform()
        {
            Vector3 cameraPosDir = Vector3.zero;
            Vector3 cameraPos = Vector3.zero;

            if (!fixedPoint)
            {
                float currentGroundHigh = 0;

                //Set wanted position to target's position if we are following something.
                if (followingTarget)
                {
                    objectPos.x = followingTarget.position.x;
                    objectPos.z = followingTarget.position.z;
                }

                //Calculate vertical distance to ground to avoid intercepting ground.
                RaycastHit hit;
                Vector3 emitPos = objectPos;
                emitPos.y += 99999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = hit.point.y;
                }

                emitPos = trans.position;
                emitPos.y += 99999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = Mathf.Max(currentGroundHigh, hit.point.y, seaLevel);
                    //currentGroundHigh = Mathf.Max(currentGroundHigh, hit.point.y);
                }

                if (followingTarget)
                {
                    currentGroundHigh = followingTarget.position.y;
                }

                //Lerp actual rotation to wanted value.
                Quaternion targetRot = Quaternion.Euler(wantXAngle, wantYAngle, 0f);
                trans.rotation = Quaternion.Lerp(trans.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);

                //Calculate a world position refers to the center of screen.
                float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);

                //Use this vector to move camera back and rotate.
                Quaternion targetYRot = Quaternion.Euler(0f, wantYAngle, 0f);
                cameraPosDir = targetYRot * (Vector3.forward * dist);

                //Calculate the actual world position to prepare to move our camera object.
                cameraPos = objectPos - cameraPosDir;

                if (trans.position.y > groundHighTestValMax)
                {
                    targetCurrentGroundHigh = Mathf.Lerp(targetCurrentGroundHigh, currentGroundHigh, groundHighTestValLerpSpeed * Time.smoothDeltaTime);
                }
                else
                {
                    targetCurrentGroundHigh = currentGroundHigh;
                }
                cameraPos.y = (objectPos.y + targetCurrentGroundHigh);
                //Lerp to wanted position.
                trans.position = Vector3.Lerp(trans.position, cameraPos, movementLerpSpeed * Time.deltaTime);
            }
            else
            {
                //If we are positioning to a fixed point, we simply move to it.
                trans.rotation = Quaternion.Lerp(trans.rotation, fixedPoint.rotation, rotationLerpSpeed * Time.deltaTime);
                trans.position = Vector3.Lerp(trans.position, fixedPoint.position, movementLerpSpeed * Time.deltaTime);

                //We also keep objectPos to fixedPoint to make a stable feeling while leave fixed point mode.
                objectPos.x = fixedPoint.position.x;
                objectPos.z = fixedPoint.position.z;
            }
            if (scrollHigh != null)
            {
                maxHight = scrollHigh.Evaluate(1.0f);
                minHight = scrollHigh.Evaluate(0.0f);
            }
            if (scrollXAngle != null)
            {
                maxAngle = scrollXAngle.Evaluate(1.0f);
                minAngle = scrollXAngle.Evaluate(0.0f);
            }
        }
        #endregion

        #region set
        public void SetMinMaxHeight(float min, float max)
        {
            scrollHigh = AnimationCurve.Linear(0, min, 1, max);
        }
        public void SetGroundTest(bool b)
        {
            groundHighTest = b;
        }
        public void LockFixedPoint(Transform pos)
        {
            self.followingTarget = null;
            self.fixedPoint = pos;
        }
        public void UnlockFixedPoint()
        {
            self.fixedPoint = null;
        }
        public void Follow(Transform target)
        {
            self.fixedPoint = null;
            self.followingTarget = target;
        }
        public void CancelFollow()
        {
            self.followingTarget = null;
        }
        public void JumpTo(Vector3 pos)
        {
            self.objectPos.x = pos.x;
            self.objectPos.z = pos.z;
        }
        public void Move(Vector3 dir)
        {
            if (dir.z > 0) { _Move(trans.forward, DesktopMoveSpeed * Time.deltaTime); }
            if (dir.z < 0) { _Move(-trans.forward, DesktopMoveSpeed * Time.deltaTime); }
            if (dir.x < 0) { _Move(-trans.right, DesktopMoveSpeed * Time.deltaTime); }
            if (dir.x > 0) { _Move(trans.right, DesktopMoveSpeed * Time.deltaTime); }
        }
        public void SetBound(List<float> data)
        {
            if (data.Count < 4)
            {
                CLog.Error("data 数据不足4");
                return;
            }
            float x = data[0];
            float y = data[1];
            float width = data[2];
            float height = data[3];
            bound = new Rect(x, y,width - x,height - y);
        }
        public void RotateY(float dir)
        {
            if (!IsControl) 
                return;

            wantYAngle += dir;
            MathUtil.WrapAngle(wantYAngle);
        }
        public void RotateX(float dir)
        {
            if (!IsControl)
                return;

            wantXAngle += dir;
            wantXAngle = Mathf.Clamp(wantXAngle,minXAxisRotateAngle,maxAngle);
            MathUtil.WrapAngle(wantXAngle);
        }
        public void Scroll(float value)
        {
            if (!IsControl) 
                return;

            ScrollValue += value;
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);
        }
        public void SetScroll(float value)
        {
            if (!IsControl) 
                return;

            ScrollValue = value;
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);
        }
        #endregion

        #region get
        public Transform GetFollowingTarget() { return self.followingTarget; }
        public Transform GetFixedPoint() { return self.fixedPoint; }
        #endregion

        #region privae
        Vector3 CalculateCurrentObjectPosition()
        {
            float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);
            Vector3 objectPosDir = -(transform.rotation * (-Vector3.forward * dist));
            return transform.position + objectPosDir;
        }
        void _Move(Vector3 dir, float speed)
        {
            if (!IsControl) return;

            dir.y = 0;
            dir.Normalize();
            dir *= speed;
            if (unlockWhenMove && dir != Vector3.zero)
            {
                followingTarget = null;
                fixedPoint = null;
            }
            objectPos += dir;

            objectPos.x = Mathf.Clamp(objectPos.x, bound.xMin, bound.xMax);
            objectPos.z = Mathf.Clamp(objectPos.z, bound.yMin, bound.yMax);
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
                    !MathUtil.Approximately(self.trans.position.x, objectPos.x, 0.02f) ||
                    !MathUtil.Approximately(self.trans.position.y, objectPos.y, 0.02f) ||
                    !MathUtil.Approximately(self.trans.position.z, objectPos.z, 0.02f);
            }
        }
        #endregion

        void OnDrawGizmosSelected()
        {
            //Draw debug lines.
            Vector3 mp = transform.position;
            Gizmos.DrawLine(new Vector3(bound.xMin, 0, bound.yMin), new Vector3(bound.xMin, 0, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMin, 0, bound.yMax), new Vector3(bound.xMax, 0, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMax, 0, bound.yMax), new Vector3(bound.xMax, 0, bound.yMin));
            Gizmos.DrawLine(new Vector3(bound.xMax, 0, bound.yMin), new Vector3(bound.xMin, 0, bound.yMin));
        }
    }
}

public enum CamScrollAnimType
{
    Simple, 
    Advanced
}