using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HighlightPlus {

    public delegate bool OnObjectSelectionEvent(GameObject obj);


    [RequireComponent(typeof(HighlightEffect))]
    [DefaultExecutionOrder(100)]
    [HelpURL("https://www.dropbox.com/s/v9qgn68ydblqz8x/Documentation.pdf?dl=0")]
    public class HighlightManager : MonoBehaviour {

        [Tooltip("Enables highlight when pointer is over this object.")]
        public bool highlightOnHover = true;

        public LayerMask layerMask = -1;
        public Camera raycastCamera;
        public RayCastSource raycastSource = RayCastSource.MousePosition;
        [Tooltip("Minimum distance for target.")]
        public float minDistance;
        [Tooltip("Maximum distance for target. 0 = infinity")]
        public float maxDistance;
        [Tooltip("Blocks interaction if pointer is over an UI element")]
        public bool respectUI = true;

        [Tooltip("If the object will be selected by clicking with mouse or tapping on it.")]
        public bool selectOnClick;
        [Tooltip("Optional profile for objects selected by clicking on them")]
        public HighlightProfile selectedProfile;
        [Tooltip("Profile to use whtn object is selected and highlighted.")]
        public HighlightProfile selectedAndHighlightedProfile;
        [Tooltip("Automatically deselects other previously selected objects")]
        public bool singleSelection;
        [Tooltip("Toggles selection on/off when clicking object")]
        public bool toggle;

        HighlightEffect baseEffect, currentEffect;
        Transform currentObject;

        public readonly static List<HighlightEffect> selectedObjects = new List<HighlightEffect>();
        public event OnObjectSelectionEvent OnObjectSelected;
        public event OnObjectSelectionEvent OnObjectUnSelected;
        public event OnObjectHighlightEvent OnObjectHighlightStart;
        public event OnObjectHighlightEvent OnObjectHighlightEnd;
        public static int lastTriggerTime;

        static HighlightManager _instance;
        public static HighlightManager instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<HighlightManager>();
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        void DomainReloadDisabledSupport() {
            selectedObjects.Clear();
            lastTriggerTime = 0;
            _instance = null;
        }

        void OnEnable() {
            currentObject = null;
            currentEffect = null;
            if (baseEffect == null) {
                baseEffect = GetComponent<HighlightEffect>();
                if (baseEffect == null) {
                    baseEffect = gameObject.AddComponent<HighlightEffect>();
                }
            }
            raycastCamera = GetComponent<Camera>();
            if (raycastCamera == null) {
                raycastCamera = GetCamera();
                if (raycastCamera == null) {
                    Debug.LogError("Highlight Manager: no camera found!");
                }
            }
            InputProxy.Init();
        }


        void OnDisable() {
            SwitchesCollider(null);
            internal_DeselectAll();
        }

        void Update() {
            if (raycastCamera == null)
                return;

#if ENABLE_INPUT_SYSTEM
            if (respectUI) {
                EventSystem es = EventSystem.current;
                if (es == null) {
                    es = CreateEventSystem();
                }
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                PointerEventData eventData = new PointerEventData(es);
                Vector3 cameraPos = raycastCamera.transform.position;
                if (raycastSource == RayCastSource.MousePosition) {
                    eventData.position = InputProxy.mousePosition;
                } else {
                    eventData.position = new Vector2(raycastCamera.pixelWidth * 0.5f, raycastCamera.pixelHeight * 0.5f);
                }
                es.RaycastAll(eventData, raycastResults);
                int hitCount = raycastResults.Count;
                // check UI blocker
                bool blocked = false;
                for (int k = 0; k < hitCount; k++) {
                    RaycastResult rr = raycastResults[k];
                    if (rr.module is UnityEngine.UI.GraphicRaycaster) {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) return;

                // look for our gameobject
                for (int k = 0; k < hitCount; k++) {
                    RaycastResult rr = raycastResults[k];
                    float distance = Vector3.Distance(rr.worldPosition, cameraPos);
                    if (distance < minDistance || (maxDistance > 0 && distance > maxDistance)) continue;

                    GameObject theGameObject = rr.gameObject;
                    if ((layerMask & (1 << rr.gameObject.layer)) == 0) continue;

                    // is this object state controller by Highlight Trigger?
                    HighlightTrigger trigger = theGameObject.GetComponent<HighlightTrigger>();
                    if (trigger != null) return;

                    // Toggles selection
                    Transform t = theGameObject.transform;
                    if (InputProxy.GetMouseButtonDown(0)) {
                        if (selectOnClick) {
                            ToggleSelection(t, !toggle);
                        } else if (lastTriggerTime < Time.frameCount) {
                            internal_DeselectAll();
                        }
                    } else {
                        // Check if the object has a Highlight Effect
                        if (t != currentObject) {
                            SwitchesCollider(t);
                        }
                    }
                    return;
                }
            }
            // if not blocked by UI and no hit found, fallback to raycast (required if no PhysicsRaycaster is present on the camera)
#endif

            Ray ray;
            if (raycastSource == RayCastSource.MousePosition) {
#if !ENABLE_INPUT_SYSTEM
                if (!CanInteract()) {
                    return;
                }
#endif
                ray = raycastCamera.ScreenPointToRay(InputProxy.mousePosition);
            } else {
                ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
            }
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxDistance > 0 ? maxDistance : raycastCamera.farClipPlane, layerMask) && Vector3.Distance(hitInfo.point, ray.origin) >= minDistance) {
                Transform t = hitInfo.collider.transform;
                // is this object state controller by Highlight Trigger?
                HighlightTrigger trigger = t.GetComponent<HighlightTrigger>();
                if (trigger != null) return;

                // Toggles selection
                if (InputProxy.GetMouseButtonDown(0)) {
                    if (selectOnClick) {
                        ToggleSelection(t, !toggle);
                    } else if (lastTriggerTime < Time.frameCount) {
                        internal_DeselectAll();
                    }
                } else {
                    // Check if the object has a Highlight Effect
                    if (t != currentObject) {
                        SwitchesCollider(t);
                    }
                }
                return;
            }

            // no hit
            if (InputProxy.GetMouseButtonDown(0) && lastTriggerTime < Time.frameCount) {
                internal_DeselectAll();
            }
            SwitchesCollider(null);
        }


#if ENABLE_INPUT_SYSTEM
        EventSystem CreateEventSystem() {
            GameObject eo = new GameObject("Event System created by Highlight Plus", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            return eo.GetComponent<EventSystem>();
        }
#endif


        void SwitchesCollider(Transform newObject) {
            if (currentEffect != null) {
                if (highlightOnHover) {
                    Highlight(false);
                }
                currentEffect = null;
            }
            currentObject = newObject;
            if (newObject == null) return;
            HighlightTrigger ht = newObject.GetComponent<HighlightTrigger>();
            if (ht != null && ht.enabled)
                return;

            HighlightEffect otherEffect = newObject.GetComponent<HighlightEffect>();
            if (otherEffect == null) {
                // Check if there's a parent highlight effect that includes this object
                HighlightEffect parentEffect = newObject.GetComponentInParent<HighlightEffect>();
                if (parentEffect != null && parentEffect.Includes(newObject)) {
                    currentEffect = parentEffect;
                    if (highlightOnHover) {
                        Highlight(true);
                    }
                    return;
                }
            }
            currentEffect = otherEffect != null ? otherEffect : baseEffect;
            baseEffect.enabled = currentEffect == baseEffect;
            currentEffect.SetTarget(currentObject);

            if (highlightOnHover) {
                Highlight(true);
            }
        }


        bool CanInteract() {
            if (!respectUI) return true;
            EventSystem es = EventSystem.current;
            if (es == null) return true;
            if (Application.isMobilePlatform && InputProxy.touchCount > 0 && es.IsPointerOverGameObject(InputProxy.GetFingerIdFromTouch(0))) {
                return false;
            } else if (es.IsPointerOverGameObject(-1))
                return false;
            return true;
        }


        void ToggleSelection(Transform t, bool forceSelection) {

            // We need a highlight effect on each selected object
            HighlightEffect hb = t.GetComponent<HighlightEffect>();
            if (hb == null) {
                HighlightEffect parentEffect = t.GetComponentInParent<HighlightEffect>();
                if (parentEffect != null && parentEffect.Includes(t)) {
                    hb = parentEffect;
                    if (hb.previousSettings == null) {
                        hb.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                    }
                    hb.previousSettings.Save(hb);
                } else {
                    hb = t.gameObject.AddComponent<HighlightEffect>();
                    hb.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                    // copy default highlight effect settings from this manager into this highlight plus component
                    hb.previousSettings.Save(baseEffect);
                    hb.previousSettings.Load(hb);
                }
            }

            bool currentState = hb.isSelected;
            bool newState = forceSelection ? true : !currentState;
            if (newState == currentState) return;

            if (newState) {
                if (OnObjectSelected != null && !OnObjectSelected(t.gameObject)) return;
            } else {
                if (OnObjectUnSelected != null && !OnObjectUnSelected(t.gameObject)) return;
            }

            if (singleSelection) {
                internal_DeselectAll();
            }

            currentEffect = hb;
            currentEffect.isSelected = newState;
            baseEffect.enabled = false;

            if (currentEffect.isSelected) {
                if (currentEffect.previousSettings == null) {
                    currentEffect.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                }
                hb.previousSettings.Save(hb);

                if (!selectedObjects.Contains(currentEffect)) {
                    selectedObjects.Add(currentEffect);
                }
            } else {
                if (currentEffect.previousSettings != null) {
                    currentEffect.previousSettings.Load(hb);
                }
                if (selectedObjects.Contains(currentEffect)) {
                    selectedObjects.Remove(currentEffect);
                }
            }

            Highlight(true);
        }

        void Highlight(bool state) {
            if (state) {
                if (!currentEffect.highlighted) {
                    if (OnObjectHighlightStart != null && currentEffect.target != null) {
                        if (!OnObjectHighlightStart(currentEffect.target.gameObject)) return;
                    }
                }
            } else {
                if (currentEffect.highlighted) {
                    if (OnObjectHighlightEnd != null && currentEffect.target != null) {
                        OnObjectHighlightEnd(currentEffect.target.gameObject);
                    }
                }
            }
            if (selectOnClick) {
                if (currentEffect.isSelected) {
                    if (state && selectedAndHighlightedProfile != null) {
                        selectedAndHighlightedProfile.Load(currentEffect);
                    } else if (selectedProfile != null) {
                        selectedProfile.Load(currentEffect);
                    } else {
                        currentEffect.previousSettings.Load(currentEffect);
                    }
                    if (currentEffect.highlighted) {
                        currentEffect.UpdateMaterialProperties();
                    } else {
                        currentEffect.SetHighlighted(true);
                    }
                    return;
                } else if (!highlightOnHover) {
                    currentEffect.SetHighlighted(false);
                    return;
                }
            }
            currentEffect.SetHighlighted(state);
        }

        public static Camera GetCamera() {
            Camera raycastCamera = Camera.main;
            if (raycastCamera == null) {
                raycastCamera = FindObjectOfType<Camera>();
            }
            return raycastCamera;
        }

        void internal_DeselectAll() {
            foreach (HighlightEffect hb in selectedObjects) {
                if (hb != null && hb.gameObject != null) {
                    if (OnObjectUnSelected != null) {
                        if (!OnObjectUnSelected(hb.gameObject)) continue;
                    }
                    hb.RestorePreviousHighlightEffectSettings();
                    hb.isSelected = false;
                    hb.SetHighlighted(false);
                }
            }
            selectedObjects.Clear();
        }


        public static void DeselectAll() {
            foreach (HighlightEffect hb in selectedObjects) {
                if (hb != null && hb.gameObject != null) {
                    hb.isSelected = false;
                    if (hb.highlighted && _instance != null) {
                        _instance.Highlight(false);
                    } else {
                        hb.SetHighlighted(false);
                    }
                }
            }
            selectedObjects.Clear();
        }

        /// <summary>
        /// Manually causes highlight manager to select an object
        /// </summary>
        public void SelectObject(Transform t) {
            ToggleSelection(t, true);
        }

        /// <summary>
        /// Manually causes highlight manager to toggle selection on an object
        /// </summary>
        public void ToggleObject(Transform t) {
            ToggleSelection(t, false);
        }

        /// <summary>
        /// Manually causes highlight manager to unselect an object
        /// </summary>
        public void UnselectObject(Transform t) {
            if (t == null) return;
            HighlightEffect hb = t.GetComponent<HighlightEffect>();
            if (hb == null) return;

            if (selectedObjects.Contains(hb)) {
                if (OnObjectUnSelected != null) {
                    if (!OnObjectUnSelected(hb.gameObject)) return;
                }
                hb.isSelected = false;
                hb.SetHighlighted(false);
                selectedObjects.Remove(hb);
            }
        }


    }

}