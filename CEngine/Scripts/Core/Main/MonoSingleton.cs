using UnityEngine;
namespace CYM
{
    [Unobfus]
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T m_instance = null;
        [HideInInspector]
        public static T Ins
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Create();
                }

                return m_instance;
            }
        }
        protected static T Create()
        {
            var ret = GameObject.FindObjectOfType(typeof(T)) as T;
            if (ret == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                ret = go.AddComponent<T>();
                GameObject parent = GameObject.Find("Global");
                if (parent != null)
                {
                    go.transform.parent = parent.transform;
                }
            }
            return ret;
        }

        protected virtual void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this as T;
            }
            Init();
        }
        void OnDestroy()
        {
            m_instance = null;
        }

        protected virtual void Init()
        {

        }

        public virtual void Dispose()
        {

        }

    }

}