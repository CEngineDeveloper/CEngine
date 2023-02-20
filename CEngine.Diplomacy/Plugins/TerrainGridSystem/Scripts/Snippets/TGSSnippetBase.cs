using System;
using System.Collections;
using UnityEngine;

namespace TGS {

    public enum ExecutionEvent {
        Never,
        Immediate,
        OnlyInEditMode,
        OnStart
    }

    public enum ExecutionEaseType {
        Linear = 0,
        EaseOut = 1,
        EaseIn = 2,
        EaseInOut = 3,
        QuadraticEaseIn = 4,
        QuadraticEaseOut = 5,
        QuadraticEaseInOut = 6,
        SmoothStep = 7,
        SmootherStep = 8
    }


    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public class TGSSnippetBase : MonoBehaviour {

        // general configuration fields
        [HideInInspector]
        public ExecutionEvent execute = ExecutionEvent.Immediate;

        [HideInInspector]
        public int order;

        [HideInInspector]
        public float delay;

        [HideInInspector]
        public float duration;

        [HideInInspector]
        public ExecutionEaseType easeType;

        // state fields
        [NonSerialized]
        public TerrainGridSystem tgs;

        [NonSerialized]
        public string instructions;

        [NonSerialized]
        public bool supportsTweening;

        [NonSerialized]
        public bool hideOptions;

        [NonSerialized]
        public bool isExecuting;



        protected virtual void Configure() {
        }

        protected virtual bool Prepare() {
            return true;
        }

        protected virtual void Execute(float t = 1f) {
        }



        void OnEnable() {
            tgs = TerrainGridSystem.instance;
            Configure();
            PingSnippets();
        }


        void Start() {
            if (enabled && Application.isPlaying && execute == ExecutionEvent.OnStart) {
                if (!supportsTweening) {
                    duration = 0;
                }
                Invoke("Run", order / 1000f + delay);
            }
        }

        public void PingSnippets() {
            SendMessage("RunIfImmediate", SendMessageOptions.DontRequireReceiver);
        }

        void RunIfImmediate() {
            if (enabled && (execute == ExecutionEvent.Immediate || (!Application.isPlaying && execute == ExecutionEvent.OnlyInEditMode))) {
                if (Application.isPlaying) {
                    Invoke("Run", order / 1000f);
                } else {
                    Run();
                }
            }
        }

        public void Run() {
            if (isExecuting)
                return;

            if (tgs == null) {
                tgs = TerrainGridSystem.instance;
                if (tgs == null)
                    return;
            }
            if (tgs.cells == null) {
                return;
            }
            if (Prepare()) {
                if (duration == 0) {
                    Execute();
                } else {
                    StartCoroutine(ExecuteWithDuration());
                }
            }
        }

        IEnumerator ExecuteWithDuration() {

            isExecuting = true;
            float startTime = Time.time;
            float t = 0;

            while (t < 1f) {
                t = (Time.time - startTime) / duration;
                if (t < 0)
                    t = 0;
                else if (t > 1f) {
                    t = 1f;
                }
                Execute(t);
                if (t >= 1f) {
                    break;
                }
                yield return null;
            }

            isExecuting = false;

        }

        protected float Tween(float t) {
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

            switch (easeType) {
                case ExecutionEaseType.EaseIn:
                    return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                case ExecutionEaseType.EaseOut:
                    return Mathf.Sin(t * Mathf.PI * 0.5f);
                case ExecutionEaseType.EaseInOut:
                    return 0.5f * (1 - Mathf.Cos(t * Mathf.PI));
                case ExecutionEaseType.QuadraticEaseIn:
                    return t * t;
                case ExecutionEaseType.QuadraticEaseOut:
                    return -(t * (t - 2));
                case ExecutionEaseType.QuadraticEaseInOut:
                    if (t < 0.5f) {
                        return 2 * t * t;
                    }
                    return (-2 * t * t) + (4 * t) - 1;
                case ExecutionEaseType.SmoothStep:
                    return t * t * (3f - 2f * t);
                case ExecutionEaseType.SmootherStep:
                    return t * t * t * (t * (6f * t - 15f) + 10f);
            }
            return t;


        }

    }
}