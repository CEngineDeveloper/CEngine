using UnityEngine;
using UnityEngine.Events;

namespace FoW
{
    [AddComponentMenu("FogOfWar/FogOfWarEvents")]
    public class FogOfWarEvents : MonoBehaviour
    {
        public int team = 0;

        [Range(0.0f, 1.0f)]
        public float minFogStrength = 0.2f;
        public UnityEvent onFogEnter;
        public UnityEvent onFogExit;

        bool _isInFog = false;
        Transform _transform;

        void Start()
        {
            _transform = transform;
        }

        void Update()
        {
            bool isinfog = FogOfWarTeam.GetTeam(team).GetFogValue(_transform.position) < (byte)(minFogStrength * 255);
            if (_isInFog == isinfog)
                return;

            _isInFog = !_isInFog;

            if (_isInFog)
                onFogEnter.Invoke();
            else
                onFogExit.Invoke();
        }
    }
}
