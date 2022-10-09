using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.UI.Particle
{
    public class UIParticleFollower : MonoBehaviour
    {
        public enum UpdateFrequency
        {
            OnAwake, OnStart, EveryUpdate, ScriptOnly
        }

        [System.Serializable]
        public class UISnapshot
        {
            public Vector3 rectWorldDiagonal;
            public Vector3 offsetFromMiddle;
            public Vector3 transformScale;
        }

        public RectTransform targetRectTransform;
        public UpdateFrequency scaleUpdateRefresh;
        public bool updatePositionToo = true;
        public bool updateOnGizmosRefresh = false;
        public UISnapshot dataSnapshot;

        private Vector3[] cornersTmp;
        private Vector3 lastRectLeftBottomCorner;
        private Vector3 lastRectRightTopCorner;

        public Transform Trans { get; private set; }

        protected void Awake()
        {
            lastRectRightTopCorner = lastRectLeftBottomCorner = Vector3.zero;

            cornersTmp = new Vector3[4];
            Trans = transform;

            if (scaleUpdateRefresh <= UpdateFrequency.EveryUpdate)
                Refresh();
        }

        protected void Start()
        {
            if (scaleUpdateRefresh == UpdateFrequency.OnStart)
                Refresh();
        }

        protected void Update()
        {
            if (scaleUpdateRefresh == UpdateFrequency.EveryUpdate)
                Refresh();
            else
                enabled = false;
        }

        protected void OnDrawGizmosSelected()
        {
            if (targetRectTransform == null || dataSnapshot.transformScale == Vector3.zero)
                return;

            if (updateOnGizmosRefresh)
            {
                if (cornersTmp == null)
                    Awake();
                Refresh();
            }
        }

        public void MakeRectSnapshot()
        {
            cornersTmp = new Vector3[4];
            targetRectTransform.GetWorldCorners(cornersTmp);
            dataSnapshot.rectWorldDiagonal = cornersTmp[2] - cornersTmp[0];
            dataSnapshot.transformScale = transform.localScale;
            dataSnapshot.offsetFromMiddle = transform.position - (cornersTmp[2] + cornersTmp[0]) * 0.5f;
        }

        public void Refresh(bool forceRefresh = false)
        {
            if (targetRectTransform == null)
                return;

            targetRectTransform.GetWorldCorners(cornersTmp);

            if (forceRefresh
                || cornersTmp[0] != lastRectLeftBottomCorner
                || cornersTmp[2] != lastRectRightTopCorner)
            {
                SetScaleToCorners(cornersTmp[0], cornersTmp[2]);
            }
        }

        private void SetScaleToCorners(Vector3 leftBottomCorner, Vector3 rightTopCorner)
        {
            lastRectLeftBottomCorner = leftBottomCorner;
            lastRectRightTopCorner = rightTopCorner;
            Vector3 newRectDiagonal = rightTopCorner - leftBottomCorner;
            Vector3 scaleFactor = newRectDiagonal;
            if (dataSnapshot.rectWorldDiagonal.x != 0f)
                scaleFactor.x /= dataSnapshot.rectWorldDiagonal.x;
            else
                scaleFactor.x = 1f;

            if (dataSnapshot.rectWorldDiagonal.y != 0f)
                scaleFactor.y /= dataSnapshot.rectWorldDiagonal.y;
            else
                scaleFactor.y = 1f;

            if (dataSnapshot.rectWorldDiagonal.z != 0f)
                scaleFactor.z /= dataSnapshot.rectWorldDiagonal.z;
            else
                scaleFactor.z = 1f;

            Trans.localScale = new Vector3(scaleFactor.x * dataSnapshot.transformScale.x,
                                            scaleFactor.y * dataSnapshot.transformScale.y,
                                            scaleFactor.z * dataSnapshot.transformScale.z);

            if (updatePositionToo)
            {
                Trans.position = (rightTopCorner + leftBottomCorner) * 0.5f + dataSnapshot.offsetFromMiddle;
            }
        }

    }
}