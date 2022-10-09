using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
#endif

namespace HighlightPlus {

    public static class InputProxy {

#if ENABLE_INPUT_SYSTEM

        static Vector3 lastPointerPosition;

        public static void Init() {
            if (!EnhancedTouch.EnhancedTouchSupport.enabled) {
                EnhancedTouch.EnhancedTouchSupport.Enable();
            }
        }

        public static Vector3 mousePosition {
            get {
                if (touchCount > 0) {
                    lastPointerPosition = EnhancedTouch.Touch.activeTouches[0].screenPosition;
                } else {
                    Mouse m = Mouse.current;
                    if (m != null) {
                        lastPointerPosition = m.position.ReadValue();
                    }
                }
                return lastPointerPosition;
            }
        }

        public static bool GetMouseButtonDown(int buttonIndex) {
            if (touchCount > 0) {
                return EnhancedTouch.Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began;
            } else {
                Mouse m = Mouse.current;
                if (m == null) return false;
                switch (buttonIndex) {
                    case 1: return m.rightButton.wasPressedThisFrame;
                    case 2: return m.middleButton.wasPressedThisFrame;
                    default: return m.leftButton.wasPressedThisFrame;
                }
            }
        }

        public static int touchCount { get { return EnhancedTouch.Touch.activeTouches.Count; } }

        public static int GetFingerIdFromTouch(int touchIndex) {
            EnhancedTouch.Touch touch = EnhancedTouch.Touch.activeTouches[touchIndex];
            return touch.finger.index;
        }

        public static bool GetKeyDown(string name) {
            return ((KeyControl)Keyboard.current[name]).wasPressedThisFrame;
        }

#else
        public static void Init() {}

        public static Vector3 mousePosition {
            get {
                return Input.mousePosition;
            }
        }

        public static bool GetMouseButtonDown(int buttonIndex) {
            return Input.GetMouseButtonDown(buttonIndex);
        }

        public static int touchCount {
            get { return Input.touchCount; }
        }

        public static int GetFingerIdFromTouch(int touchIndex) {
            return Input.GetTouch(touchIndex).fingerId;
        }

        public static bool GetKeyDown(string name) {
            return Input.GetKeyDown(name);
        }

#endif

    }
}
