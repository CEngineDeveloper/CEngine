using UnityEngine;
namespace CYM.Unit
{
    public class EffectFloatate : BaseCoreMono
    {
        [SerializeField]
        float bobSpeed = 3.0f;  //Bob speed
        [SerializeField]
        float bobHeight = 0.5f; //Bob height
        [SerializeField]
        float bobOffset = 0.0f;

        [SerializeField]
        float PrimaryRot = 80.0f;  //First axies degrees per second
        [SerializeField]
        float SecondaryRot = 40.0f; //Second axies degrees per second
        [SerializeField]
        float TertiaryRot = 20.0f;  //Third axies degrees per second

        float bottom;

        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Awake()
        {
            base.Awake();
            if (bobSpeed < 0)
            {
                Debug.LogWarning("Negative object bobSpeed value! May result in undesired behavior. Continuing anyway.", gameObject);
            }

            if (bobHeight < 0)
            {
                Debug.LogWarning("Negative object bobHeight value! May result in undesired behavior. Continuing anyway.", gameObject);
            }

            bottom = transform.position.y;

        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            transform.Rotate(new Vector3(0, PrimaryRot, 0) * Time.deltaTime, Space.World);
            transform.Rotate(new Vector3(SecondaryRot, 0, 0) * Time.deltaTime, Space.Self);
            transform.Rotate(new Vector3(0, 0, TertiaryRot) * Time.deltaTime, Space.Self);
            //float y = bottom + (((Mathf.Cos((Time.time + bobOffset) * bobSpeed) + 1) / 2) * bobHeight); 
            transform.position.Set(transform.position.x, bottom, transform.position.z);
        }
    }

}