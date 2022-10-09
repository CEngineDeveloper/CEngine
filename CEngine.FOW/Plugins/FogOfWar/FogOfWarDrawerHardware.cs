using UnityEngine;

namespace FoW
{
    public class FogOfWarDrawerHardware : FogOfWarDrawer
    {
        public Material _material = null;
        public RenderTexture _renderTexture = null;
        Texture2D _outputTexture = null;
        Texture2D _lineOfSightTex = null;
        byte[] _lineOfSightCopyBuffer = null;
        public Texture2D _lineOfSightCellsTex = null;
        byte[] _lineOfSightCellsCopyBuffer = null;
        byte[] _colorDataCache = null;

        public override void GetValues(byte[] outvalues)
        {
            RenderTexture temp = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            _outputTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
            //_outputTexture.Apply(false, false);
            RenderTexture.active = temp;

            Unity.Collections.NativeArray<byte> ptr = _outputTexture.GetRawTextureData<byte>();
            if (_colorDataCache == null || _colorDataCache.Length != outvalues.Length * 4)
                _colorDataCache = new byte[outvalues.Length * 4];
            ptr.CopyTo(_colorDataCache);
            for (int i = 0; i < outvalues.Length; ++i)
                outvalues[i] = _colorDataCache[(i << 2) + 1];
        }

        public override void SetValues(byte[] values)
        {
            //
        }

        protected override void OnInitialise()
        {
            if (_material == null)
                _material = new Material(FogOfWarUtils.FindShader("Hidden/FogOfWarHardware"));

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Object.Destroy(_renderTexture);
                _renderTexture = null;

                Object.Destroy(_outputTexture);
                _outputTexture = null;
            }

            _renderTexture = new RenderTexture(_map.resolution.x, _map.resolution.y, 16, RenderTextureFormat.ARGB32);
            _outputTexture = new Texture2D(_map.resolution.x, _map.resolution.y, TextureFormat.ARGB32, false);
        }

        public override void OnDestroy()
        {
            if (_material != null)
                Object.Destroy(_material);
            if (_renderTexture != null)
            {
                if (RenderTexture.active == _renderTexture)
                    RenderTexture.active = null; // not sure why this is triggered...
                _renderTexture.Release();
                Object.Destroy(_renderTexture);
            }
            if (_outputTexture != null)
                Object.Destroy(_outputTexture);
            if (_lineOfSightTex != null)
                Object.Destroy(_lineOfSightTex);
            if (_lineOfSightCellsTex != null)
                Object.Destroy(_lineOfSightCellsTex);
        }

        public override void Clear(byte value)
        {
            RenderTexture temp = RenderTexture.active;
            RenderTexture.active = _renderTexture;

            _material.SetPass(0);
            float colorvalue = value / 255.0f;
            GL.Clear(true, true, new Color(colorvalue, colorvalue, colorvalue, colorvalue));

            RenderTexture.active = temp;
        }

        public override bool Fade(byte[] currentvalues, byte[] totalvalues, float partialfogamount, int inamount, int outamount)
        {
            // partial fog needs to be inversed
            partialfogamount = 1 - partialfogamount;
            int partialfog = (int)(partialfogamount * (1 << 8));

            bool haschanged = false;
            for (int i = 0; i < currentvalues.Length; ++i)
            {
                // if nothing has changed, don't do anything
                if (currentvalues[i] == totalvalues[i])
                    continue;

                // decrease fog
                if (currentvalues[i] < totalvalues[i])
                    totalvalues[i] = (byte)Mathf.Max(totalvalues[i] - inamount, currentvalues[i]);
                else
                {
                    // increase fog
                    int target = (currentvalues[i] * partialfog) >> 8;
                    if (totalvalues[i] < target)
                        totalvalues[i] = (byte)Mathf.Min(totalvalues[i] + outamount, target);
                }

                haschanged = true;
            }

            return haschanged;
        }

        void SetupLineOfSight(FogOfWarShape shape)
        {
            // prepare texture
            if (shape.lineOfSight == null)
                return;
            
            if (_lineOfSightTex == null || _lineOfSightTex.width < shape.lineOfSight.Length)
            {
                if (_lineOfSightTex != null)
                    Object.Destroy(_lineOfSightTex);
                _lineOfSightTex = new Texture2D(shape.lineOfSight.Length, 1, TextureFormat.RFloat, false);
                _lineOfSightTex.filterMode = FilterMode.Bilinear;
                _lineOfSightTex.wrapMode = TextureWrapMode.Repeat;
                _lineOfSightCopyBuffer = new byte[shape.lineOfSight.Length * sizeof(float)];
            }

            System.Buffer.BlockCopy(shape.lineOfSight, 0, _lineOfSightCopyBuffer, 0, shape.lineOfSight.Length * sizeof(float));
            _lineOfSightTex.LoadRawTextureData(_lineOfSightCopyBuffer);
            _lineOfSightTex.Apply(false, false);

            // setup material
            _material.SetTexture("_LineOfSightTex", _lineOfSightTex);
            _material.SetVector("_LineOfSightValues", new Vector4(shape.CalculateMaxLineOfSightDistance() / _map.size, shape.lineOfSightMinAngle, shape.lineOfSightMaxAngle, shape.lineOfSightSeeOutsideRange ? 1 : 0));
        }

        void SetupLineOfSightCells(FogOfWarShape shape)
        {
            // prepare texture
            if (shape.visibleCells == null)
                return;

            if (_lineOfSightCellsTex == null || _lineOfSightCellsTex.width * _lineOfSightCellsTex.height < shape.visibleCells.Length)
            {
                if (_lineOfSightCellsTex != null)
                    Object.Destroy(_lineOfSightCellsTex);
                _lineOfSightCellsTex = new Texture2D(shape.visibleCellsWidth, shape.visibleCells.Length / shape.visibleCellsWidth, TextureFormat.Alpha8, false);
                _lineOfSightCellsTex.filterMode = FilterMode.Point;
                _lineOfSightCellsTex.wrapMode = TextureWrapMode.Clamp;
                _lineOfSightCellsCopyBuffer = new byte[shape.visibleCells.Length];
            }

            for (int i = 0; i < shape.visibleCells.Length; ++i)
                _lineOfSightCellsCopyBuffer[i] = shape.visibleCells[i] ? (byte)255 : (byte)0;
            _lineOfSightCellsTex.LoadRawTextureData(_lineOfSightCellsCopyBuffer);
            _lineOfSightCellsTex.Apply(false, false);

            // setup material
            _material.SetTexture("_LineOfSightTex", _lineOfSightCellsTex);
            int pixelsize = (int)shape.CalculateMaxLineOfSightDistance() * 2 + 1;
            _material.SetVector("_LineOfSightValues", new Vector4(pixelsize / _map.size, shape.lineOfSightMinAngle, shape.lineOfSightMaxAngle, shape.lineOfSightSeeOutsideRange ? 1 : 0));
        }

        public override void Draw(FogOfWarShape shape, bool ismultithreaded)
        {
            DrawInfo info = new DrawInfo(_map, shape);

            _material.SetKeywordEnabled("SHAPE_CIRCLE", shape is FogOfWarShapeCircle);
            _material.SetKeywordEnabled("SHAPE_BOX", shape is FogOfWarShapeBox);
            _material.SetKeywordEnabled("LINE_OF_SIGHT", shape.lineOfSight != null);
            _material.SetKeywordEnabled("LINE_OF_SIGHT_CELLS", shape.visibleCells != null);

            _material.SetVector("_CenterPosition", info.fogCenterPos / (_map.size * _map.pixelSize));
            _material.SetVector("_EyePosition", info.fogEyePos.ToFloat() / (_map.size * _map.pixelSize));
            _material.SetVector("_EyeForward", info.fogForward);
            _material.SetFloat("_ForwardAngle", info.forwardAngle);
            _material.SetFloat("_Brightness", shape.brightness);

            SetupLineOfSight(shape);
            SetupLineOfSightCells(shape);

            Texture2D tex = null;
            if (shape is FogOfWarShapeCircle circle)
            {
                _material.SetVector("_Size", new Vector4(shape.radius / _map.size, circle.innerRadius, 1, 1));
                _material.SetFloat("_Angle", 1 - circle.angle / 90);
            }
            else if (shape is FogOfWarShapeBox box)
            {
                _material.SetVector("_Size", shape.size / _map.size);
                tex = box.texture;
            }

            _renderTexture.MarkRestoreExpected();
            Graphics.Blit(tex ?? _outputTexture, _renderTexture, _material);
        }
    }
}
