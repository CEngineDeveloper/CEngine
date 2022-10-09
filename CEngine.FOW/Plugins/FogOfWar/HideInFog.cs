using UnityEngine;
using UnityEngine.UI;

namespace FoW
{
    [AddComponentMenu("FogOfWar/HideInFog")]
    public class HideInFog : MonoBehaviour
    {
        public int team = 0;

        [Range(0.0f, 1.0f)]
        public float minFogStrength = 0.2f;
        
        Transform _transform;
        Renderer _renderer;
        Graphic _graphic;
        Canvas _canvas;

        void Start()
        {
            _transform = transform;
            _renderer = GetComponent<Renderer>();
            _graphic = GetComponent<Graphic>();
            _canvas = GetComponent<Canvas>();
        }

        void Update()
        {
            FogOfWarTeam fow = FogOfWarTeam.GetTeam(team);
            if (fow == null)
            {
                Debug.LogWarning("There is no Fog Of War team for team #" + team.ToString());
                return;
            }

            bool visible = fow.GetFogValue(_transform.position) < minFogStrength * 255;
            if (_renderer != null)
                _renderer.enabled = visible;
            if (_graphic != null)
                _graphic.enabled = visible;
            if (_canvas != null)
                _canvas.enabled = visible;
        }
    }
}
