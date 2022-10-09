using UnityEngine;

namespace CYM.Sense
{
    public sealed class SenseObj : BaseMono
    {
        #region prop
        ISenseMgr SenseMgr;
        #endregion

        #region set
        public void Init(ISenseMgr sense)
        {
            SenseMgr = sense;
        }
        #endregion

        #region Callback
        void OnTriggerEnter(Collider other) =>SenseMgr?.DoTestEnter(other);
        void OnTriggerExit(Collider other) =>SenseMgr?.DoTestExit(other);
        void OnTriggerEnter2D(Collider2D other) =>SenseMgr?.DoTestEnter(other);
        void OnTriggerExit2D(Collider2D other) =>SenseMgr?.DoTestExit(other);
        #endregion
    }

}