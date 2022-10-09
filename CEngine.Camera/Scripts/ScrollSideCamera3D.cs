using UnityEngine;
namespace CYM.Cam
{
    /// <summary>
    /// 适用于横版游戏的摄像机跟随
    /// </summary>
    public class ScrollSideCamera3D : BaseMono
    {
        [HideInInspector]
        public Transform target;
        [Header("Follow Settings")]
        public float distanceToTarget = 5; // The distance to the target
        public float heightOffset = 5; // the height offset of the camera relative to it's target
        public float viewAngle = 10; //a downwards rotation
        public Vector3 AdditionalOffset; //any additional offset
        public bool FollowZAxis; //enable or disable the camera following the z axis
        //private bool TempFollowZAxis=false;//临时设置FollowZAxis
        public float ZBorderOffset = 5;

        [Header("Damp Settings")]
        public float Damp = 3f;

        BoxCollider CurrentAreaCollider;
        BoxCollider StartAreaCollider;
        BoxCollider ZAreaCollider;
        float AreaColliderViewOffset;
        float StartAreaColliderViewOffset;

        float currentX;
        float currentY;
        float currentZ;

        float targetX;
        float targetY;
        float targetZ;

        Vector3 playerPos;
        BoxCollider Bound;
        public bool IsDamp { get; set; } = true;

        Camera Camera;

        float preTargetX;

        public override void Awake()
        {
            base.Awake();
            Camera = GetComponent<Camera>();
        }

        #region set
        /// <summary>
        /// 设置跟随目标
        /// </summary>
        /// <param name="_target"></param>
        public void SetTarget(Transform _target)
        {
            IsDamp = true;
            if (target == null && _target != null)
            {
                Vector3 playerPos = _target.position;
                Trans.position = new Vector3(playerPos.x, playerPos.y - heightOffset, playerPos.z + (distanceToTarget));
            }
            target = _target;
        }
        /// <summary>
        /// 设置摄像机Bound
        /// </summary>
        /// <param name="cameraBound"></param>
        public void SetBound(BoxCollider cameraBound)
        {
            Bound = cameraBound;
        }
        /// <summary>
        /// 设置AreaCollider
        /// </summary>
        public void SetAreaCollider(BoxCollider areaCollider, float offset = 4.5f)
        {
            CurrentAreaCollider = areaCollider;
            AreaColliderViewOffset = offset;
        }
        public void SetStartAreaCollider(BoxCollider areaCollider, float offset = 4.5f)
        {
            StartAreaCollider = areaCollider;
            StartAreaColliderViewOffset = offset;
        }
        public void SetZAreaCollider(BoxCollider zAreaCollider)
        {
            ZAreaCollider = zAreaCollider;
        }
        #endregion

        #region life
        void Update()
        {
            if (target)
            {
                currentX = Trans.position.x;
                currentY = Trans.position.y;
                currentZ = Trans.position.z;

                playerPos = target.position + AdditionalOffset;
                targetX = playerPos.x;
                targetY = playerPos.y - heightOffset;
                targetZ = playerPos.z + distanceToTarget;
                if (!FollowZAxis)
                {
                    targetZ = distanceToTarget;
                }

                float lerpTime = Damp * Time.smoothDeltaTime;

                //Set cam position
                if (CurrentAreaCollider == null &&
                    StartAreaCollider == null &&
                    ZAreaCollider == null)
                {
                }
                else
                {
                    float cur = CurrentAreaCollider ? CurrentAreaCollider.transform.position.x + AreaColliderViewOffset : targetX; //越往右越小
                    float start = StartAreaCollider ? StartAreaCollider.transform.position.x - StartAreaColliderViewOffset : targetX;  //越往左越大
                    if (cur > start)
                    {
                        targetX = start;
                    }
                    else
                    {
                        targetX = Mathf.Clamp(targetX, cur, start);
                    }
                }

                float zBorder = ZAreaCollider.transform.position.z + ZBorderOffset;
                if (targetZ >= zBorder)
                {
                    targetZ = zBorder;
                }


                //Damp X
                currentX = IsDamp ? Mathf.Lerp(currentX, targetX, lerpTime) : targetX;
                //DampY
                currentY = IsDamp ? Mathf.Lerp(currentY, targetY, lerpTime) : targetY;
                //DampZ
                currentZ = IsDamp ? Mathf.Lerp(currentZ, targetZ, lerpTime) : targetZ;

                //Set cam rotation
                transform.rotation = new Quaternion(0, 180f, viewAngle, 0);
                Trans.position = new Vector3(currentX, currentY, currentZ);
                preTargetX = currentX;
            }

        }
        #endregion


    }
}