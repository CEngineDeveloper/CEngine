#if !ENABLE_INPUT_SYSTEM

using UnityEngine;

namespace TGS {
    /// <summary>
    /// This class provides an input layer that can be replaced or overriden to provide other kind of input systems
    /// </summary>

    public class DefaultInputSystem : IInputProxy {

        public virtual void Init() {}

        public virtual Vector3 mousePosition { get { return Input.mousePosition; } }

        public virtual bool touchSupported { get { return Input.touchSupported; } }

        public virtual int touchCount { get { return Input.touchCount; } }

        public virtual float GetAxis(string axisName) {
            return Input.GetAxis(axisName);
        }

        public virtual bool GetButtonDown(string buttonName) {
            return Input.GetButtonDown(buttonName);
        }

        public virtual bool GetButtonUp(string buttonName) {
            return Input.GetButtonUp(buttonName);
        }

        public virtual bool GetKey(string name) {
            return Input.GetKey(name);
        }

        public virtual bool GetKey(KeyCode keyCode) {
            return Input.GetKey(keyCode);
        }

        public virtual bool GetKeyDown(string name) {
            return Input.GetKeyDown(name);
        }

        public virtual bool GetKeyDown(KeyCode keyCode) {
            return Input.GetKeyDown(keyCode);
        }

        public virtual bool GetKeyUp(string name) {
            return Input.GetKeyUp(name);
        }

        public virtual bool GetKeyUp(KeyCode keyCode) {
            return Input.GetKeyUp(keyCode);
        }

        public virtual bool GetMouseButton(int buttonIndex) {
            return Input.GetMouseButton(buttonIndex);
        }

        public virtual bool GetMouseButtonDown(int buttonIndex) {
            return Input.GetMouseButtonDown(buttonIndex);
        }

        public virtual bool GetMouseButtonUp(int buttonIndex) {
            return Input.GetMouseButtonUp(buttonIndex);
        }

        public virtual bool IsTouchStarting(int touchIndex) {
            return Input.GetTouch(touchIndex).phase == TouchPhase.Began;
        }

        public virtual int GetFingerIdFromTouch(int touchIndex) {
            return Input.GetTouch(touchIndex).fingerId;
        }

        public virtual bool IsPointerOverUI() {
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1);
        }
    }
}

#endif
