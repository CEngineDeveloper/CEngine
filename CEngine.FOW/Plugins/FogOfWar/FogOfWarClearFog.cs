using UnityEngine;

namespace FoW
{
    [RequireComponent(typeof(Camera))]
    public class FogOfWarClearFog : MonoBehaviour
    {
        public Camera targetCamera;

        Camera _fogCamera;
        RenderTexture _renderTexture = null;

        void Awake()
        {
            _fogCamera = GetComponent<Camera>();
        }

        void OnDisable()
        {
            if (targetCamera.targetTexture == _renderTexture)
                targetCamera.targetTexture = null;
            targetCamera.enabled = false;
        }

        void OnDestroy()
        {
            DestroyRenderTexture();
        }

        void DestroyRenderTexture()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        void LateUpdate()
        {
            if (targetCamera == null)
                return;

            targetCamera.enabled = true;
            if (_renderTexture == null || _renderTexture.width != _fogCamera.pixelWidth || _renderTexture.height != _fogCamera.pixelHeight)
            {
                DestroyRenderTexture();
                _renderTexture = new RenderTexture(_fogCamera.pixelWidth, _fogCamera.pixelHeight, 16);
            }
            if (targetCamera.targetTexture != _renderTexture)
                targetCamera.targetTexture = _renderTexture;
        }
    }
}
