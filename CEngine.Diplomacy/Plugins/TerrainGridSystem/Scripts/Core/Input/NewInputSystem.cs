#if ENABLE_INPUT_SYSTEM

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TGS {
    /// <summary>
    /// This class provides an input layer for the new input system that can be replaced or overriden to provide custom support
    /// </summary>
    public class NewInputSystem : IInputProxy {

        public virtual void Init() {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }

        public virtual Vector3 mousePosition {
            get {
                if (Application.isMobilePlatform) {
                    if (touchCount > 0) {
                        return Touch.activeFingers[0].currentTouch.screenPosition;
                    } else {
                        return Vector3.zero;
                    }
                } else {
                    return Mouse.current.position.ReadValue();
                }
            }
        }

        public virtual bool touchSupported { get { return Touchscreen.current != null; } }

        public virtual int touchCount {
            get {
                return Touch.activeTouches.Count;
            }
        }

        public virtual LocationService location { get { return null; } }

        public virtual bool GetMouseButton(int buttonIndex) {
            switch (buttonIndex) {
                case 1: return !Application.isMobilePlatform && Mouse.current.rightButton.isPressed;
                case 2: return !Application.isMobilePlatform && Mouse.current.middleButton.isPressed;
                default:
                    if (Application.isMobilePlatform) {
                        return touchCount > 0 && Touch.activeTouches[0].isInProgress;
                    }
                    return Mouse.current.leftButton.isPressed;
            }
        }

        public virtual bool GetMouseButtonDown(int buttonIndex) {
            switch (buttonIndex) {
                case 1: return !Application.isMobilePlatform && Mouse.current.rightButton.wasPressedThisFrame;
                case 2: return !Application.isMobilePlatform && Mouse.current.middleButton.wasPressedThisFrame;
                default:
                    if (Application.isMobilePlatform) {
                        if (touchCount > 0) {
                            return Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began;
                        }
                        return false;
                    }
                    return Mouse.current.leftButton.wasPressedThisFrame;
            }
        }

        public virtual bool GetMouseButtonUp(int buttonIndex) {
            switch (buttonIndex) {
                case 1: return !Application.isMobilePlatform && Mouse.current.rightButton.wasReleasedThisFrame;
                case 2: return !Application.isMobilePlatform && Mouse.current.middleButton.wasReleasedThisFrame;
                default:
                    if (Application.isMobilePlatform) {
                        return touchCount > 0 && Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended;
                    }
                    return Mouse.current.leftButton.wasReleasedThisFrame;
            }
        }

        public virtual bool IsTouchStarting(int touchIndex) {
            Touch touch = Touch.activeTouches[touchIndex];
            return touch.phase == UnityEngine.InputSystem.TouchPhase.Began;
        }

        public virtual bool IsTouchEnding(int touchIndex) {
            Touch touch = Touch.activeTouches[touchIndex];
            return touch.phase == UnityEngine.InputSystem.TouchPhase.Ended;
        }

        public virtual int GetFingerIdFromTouch(int touchIndex) {
            Touch touch = Touch.activeTouches[touchIndex];
            return touch.finger.index;
        }

        public virtual bool GetKey(string name) {
            return ((KeyControl)Keyboard.current[name]).isPressed;
        }

        public virtual bool GetKeyDown(string name) {
            return ((KeyControl)Keyboard.current[name]).wasPressedThisFrame;
        }

        public virtual bool GetKeyUp(string name) {
            return ((KeyControl)Keyboard.current[name]).wasReleasedThisFrame;
        }


        // Note: the followihg methods are not implemented. Feel free to add your own input 
        public virtual float GetAxis(string axisName) {
            return 0;
        }

        public virtual bool GetButtonDown(string buttonName) {
            return false;
        }

        public virtual bool GetButtonUp(string buttonName) {
            return false;
        }

        public virtual bool GetKey(KeyCode keyCode) {
            return false;
        }

        public virtual bool GetKeyDown(KeyCode keyCode) {
            return false;
        }

        public virtual bool GetKeyUp(KeyCode keyCode) {
            return false;
        }

        readonly List<RaycastResult> results = new List<RaycastResult>();
        public virtual bool IsPointerOverUI() {
            var eventData = new PointerEventData(EventSystem.current) { position = mousePosition };
            EventSystem.current.RaycastAll(eventData, results);
            int resultsCount = results.Count;
            for (int k = 0; k < resultsCount; k++) {
                if (results[k].gameObject.layer == 5 && results[k].gameObject.GetComponent<RectTransform>() != null) // UI Layer
                    return true;
            }
            return false;
        }

    }
}

#endif
