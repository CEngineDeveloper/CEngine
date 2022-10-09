using System.Collections.Generic;
using UnityEngine;

namespace FoW
{
    public enum FogOfWarPhysics
    {
        None,
        Physics2D,
        Physics3D
    }

    public enum FogOfWarPlane
    {
        XY, // 2D
        YZ,
        XZ // 3D
    }

    public enum FogOfWarRenderType
    {
        Software,
        Hardware
    }

    class FogOfWarDrawThreadTask : FogOfWarThreadTask
    {
        public FogOfWarShape shape;
        public FogOfWarDrawer drawer;

        public override void Run()
        {
            drawer.Draw(shape, true);
        }
    }

    [AddComponentMenu("FogOfWar/FogOfWarTeam")]
    public class FogOfWarTeam : MonoBehaviour
    {
        public int team = 0;

        [Header("Map")]
        public Vector2Int mapResolution = new Vector2Int(128, 128);
        public float mapSize = 128;
        public Vector2 mapOffset = Vector2.zero;

        public FogOfWarPlane plane = FogOfWarPlane.XZ;
        public FogOfWarPhysics physics = FogOfWarPhysics.Physics3D;

        [Header("Visuals")]
        public bool pointFiltering = false;
        public FilterMode filterMode { get { return pointFiltering ? FilterMode.Point : FilterMode.Bilinear; } }
        public int blurAmount = 0;
        public int blurIterations = 0;
        public FogOfWarBlurType blurType = FogOfWarBlurType.Gaussian3;

        [Header("Behaviour")]
        public bool updateUnits = true;
        public bool updateAutomatically = true;
        public bool outputToTexture = true;
        bool _isPerformingManualUpdate = false;
        [Range(0.0f, 1.0f)]
        public float partialFogAmount = 0.5f;
        float _fadeInAmount = 0;
        float _fadeOutAmount = 0;
        public float fadeInDuration = 0.1f;
        public float fadeOutDuration = 0.1f;
        bool _hasFogChanged = false;

        [Header("Multithreading")]
        public FogOfWarRenderType renderType = FogOfWarRenderType.Software;
        public bool multithreaded = false;
        bool _isMultithreaded { get { return multithreaded && renderType == FogOfWarRenderType.Software; } }
        [Range(2, 8)]
        public int threads = 2;
        public double maxMillisecondsPerFrame = 5;
        FogOfWarThreadPool _threadPool = null;
        int _currentUnitProcessing = 0;
        float _timeSinceLastUpdate = 0;
        System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        // core stuff
        public int mapByteSize { get { return mapResolution.x * mapResolution.y; } }
        public Texture2D fogTexture { get; private set; }
        public Texture finalFogTexture { get; private set; }
        Texture _lastFinalFogTexture = null;
        byte[] _fogValuesCurrent = null; // how much the scene has been cleared for this frame only
        byte[] _fogValuesTotal = null; // how much the entire scene has been cleared (in the last finished fog frame)
        FogOfWarDrawer _drawer = null;
        int _drawThreadTaskPoolCount = 0;
        List<FogOfWarDrawThreadTask> _drawThreadTaskPool = new List<FogOfWarDrawThreadTask>();
        FogOfWarBlur _blur = new FogOfWarBlur();
        public UnityEngine.Events.UnityEvent onRenderFogTexture { get; private set; } = new UnityEngine.Events.UnityEvent(); // only call SetTotalFogValues() with multithreading when this is invoked!
        public UnityEngine.Events.UnityEvent onTextureChanged { get; private set; } = new UnityEngine.Events.UnityEvent(); // called when fogTexture or finalFogTexture has changed

        static List<FogOfWarTeam> _instances = new List<FogOfWarTeam>();
        public static List<FogOfWarTeam> instances { get { return _instances; } }

        /// <summary>
        /// Returns the FogOfWarTeam for a particular team.
        /// If no team exists, null will be returned.
        /// </summary>
        /// <param name="team">The index of the team to get.</param>
        /// <returns>The data for the team.</returns>
        public static FogOfWarTeam GetTeam(int team)
        {
            for (int i = 0; i < instances.Count; ++i)
            {
                if (instances[i].team == team)
                    return instances[i];
            }
            return null;
        }

        void Awake()
        {
            Reinitialize();
        }

        void OnEnable()
        {
            _instances.Add(this);
        }

        void OnDisable()
        {
            _instances.Remove(this);
        }

        void OnDestroy()
        {
            if (_drawer != null)
                _drawer.OnDestroy();
        }

        /// <summary>
        /// Reinitializes fog texture. Call this if you have changed the mapSize, mapResolution or mapOffset during runtime.
        /// This will also reset the fog. You can manually call this from the editor by right-clicking the FogOfWar component.
        /// </summary>
        public void Reinitialize()
        {
            if (_drawer != null)
                _drawer.OnDestroy();
            if (renderType == FogOfWarRenderType.Software)
                _drawer = new FogOfWarDrawerSoftware();
            else if (renderType == FogOfWarRenderType.Hardware)
                _drawer = new FogOfWarDrawerHardware();
            _drawer.Initialise(new FogOfWarMap(this));
            _drawer.Clear(255);

            if (_fogValuesCurrent == null || _fogValuesCurrent.Length != mapResolution.x * mapResolution.y)
            {
                _fogValuesCurrent = new byte[mapResolution.x * mapResolution.y];
                _fogValuesTotal = new byte[mapResolution.x * mapResolution.y];
            }

            for (int i = 0; i < _fogValuesCurrent.Length; ++i)
            {
                _fogValuesCurrent[i] = 255;
                _fogValuesTotal[i] = 255;
            }

            _drawThreadTaskPool.Clear();
        }

        /// <summary>
        /// Copies the current fog values into the specified array.
        /// The fog values is the fog that was cleared from the last frame.
        /// If you want the fog as it is displayed on-screen, use GetTotalFogValues().
        /// Returned values are 0 for completely unfogged, and 255 for completely fogged.
        /// The size of the array should be mapResolution * mapResolution.
        /// </summary>
        /// <param name="values"></param>
        public void GetCurrentFogValues(ref byte[] values)
        {
            if (values == null || values.Length != _fogValuesCurrent.Length)
                Debug.LogError("GetCurrentFogValues cannot take null as parameter or arrays of different sizes");
            else
                System.Array.Copy(_fogValuesCurrent, values, _fogValuesCurrent.Length);
        }

        /// <summary>
        /// Copies the specified array into the current fog values.
        /// The fog values is the fog that was cleared from the last frame.
        /// If you want the fog as it is displayed on-screen, use SetTotalFogValues().
        /// Returned values are 0 for completely unfogged, and 255 for completely fogged.
        /// The size of the array should be mapResolution * mapResolution.
        /// </summary>
        /// <param name="currentvalues"></param>
        public void SetCurrentFogValues(byte[] currentvalues)
        {
            if (currentvalues == null || currentvalues.Length != _fogValuesCurrent.Length)
            {
                Debug.LogError("SetCurrentFogValues cannot take null as parameter or arrays of different sizes");
                return;
            }

            System.Array.Copy(currentvalues, _fogValuesCurrent, _fogValuesCurrent.Length);
            _drawer.SetValues(_fogValuesCurrent);
        }

        /// <summary>
        /// Copies the total fog values into the specified array.
        /// The total fog values is the fog as it appears on screen from the last frame.
        /// If you want the fog.
        /// Returned values are 0 for completely unfogged, and 255 for completely fogged.
        /// The size of the array should be mapResolution * mapResolution.
        /// </summary>
        /// <param name="totalvalues"></param>
        public void GetTotalFogValues(ref byte[] totalvalues)
        {
            if (totalvalues == null || totalvalues.Length != _fogValuesTotal.Length)
                Debug.LogError("GetTotalFogValues cannot take null as parameter or arrays of different sizes");
            else
                System.Array.Copy(_fogValuesTotal, totalvalues, _fogValuesTotal.Length);
        }

        /// <summary>
        /// Copies the specified array into the total fog values.
        /// The total fog values is the fog as it appears on screen from the last frame.
        /// If you want the fog.
        /// Returned values are 0 for completely unfogged, and 255 for completely fogged.
        /// The size of the array should be mapResolution * mapResolution.
        /// </summary>
        /// <param name="totalvalues"></param>
        public void SetTotalFogValues(byte[] totalvalues)
        {
            if (totalvalues == null || totalvalues.Length != _fogValuesTotal.Length)
            {
                Debug.LogError("SetFogValues cannot take null as parameter or arrays of different sizes");
                return;
            }

            System.Array.Copy(totalvalues, _fogValuesTotal, _fogValuesTotal.Length);
        }

        /// <summary>
        /// Returns how much of the map has been explored/unfogged, where 0 is 0% and 1 is 100%.
        /// Increase the skip value to improve performance but sacrifice accuracy.
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public float ExploredArea(int skip = 1)
        {
            skip = Mathf.Max(skip, 1);
            int total = 0;
            for (int i = 0; i < _fogValuesTotal.Length; i += skip)
                total += _fogValuesTotal[i];
            return (1.0f - total / (_fogValuesTotal.Length * 255.0f / skip)) * 2;
        }
        
        /// <summary>
        /// Converts a world position to a fog pixel position. Values will be between 0 and mapResolution.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2Int WorldPositionToFogPosition(Vector3 position)
        {
            Vector2 fogplanepos = FogOfWarConversion.WorldToFogPlane(position, plane);
            Vector2 mappos = FogOfWarConversion.WorldToFog(fogplanepos, mapOffset, mapResolution, mapSize);
            return mappos.ToInt();
        }
        
        /// <summary>
        /// Returns the fog amount at a particular world position. 0 is fully unfogged and 255 if fully fogged.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public byte GetFogValue(Vector3 position)
        {
            Vector2Int mappos = WorldPositionToFogPosition(position);
            mappos.x = Mathf.Clamp(mappos.x, 0, mapResolution.x - 1);
            mappos.y = Mathf.Clamp(mappos.y, 0, mapResolution.y - 1);
            return _fogValuesTotal[mappos.y * mapResolution.x + mappos.x];
        }
        
        /// <summary>
        /// Set the fog for a square area of the map. Positions are all in world coordinates. 0 is fully unfogged and 255 if fully fogged.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="value"></param>
        public void SetFog(Bounds bounds, byte value)
        {
            Rect rect = new Rect();
            rect.min = FogOfWarConversion.WorldToFog(bounds.min, plane, mapOffset, mapResolution, mapSize);
            rect.max = FogOfWarConversion.WorldToFog(bounds.max, plane, mapOffset, mapResolution, mapSize);

            int xmin = (int)Mathf.Max(rect.xMin, 0);
            int xmax = (int)Mathf.Min(rect.xMax, mapResolution.x);
            int ymin = (int)Mathf.Max(rect.yMin, 0);
            int ymax = (int)Mathf.Min(rect.yMax, mapResolution.y);

            // if it is not visible on the map
            if (xmin >= mapResolution.x || xmax < 0 || ymin >= mapResolution.y || ymax < 0)
                return;

            for (int y = ymin; y < ymax; ++y)
            {
                for (int x = xmin; x < xmax; ++x)
                    _fogValuesTotal[y * mapResolution.x + x] = value;
            }
        }

        /// <summary>
        /// Sets the fog value for the entire map. Set to 0 for completely unfogged, to 255 for completely fogged.
        /// </summary>
        /// <param name="value"></param>
        public void SetAll(byte value = 255)
        {
            for (int i = 0; i < _fogValuesTotal.Length; ++i)
                _fogValuesTotal[i] = value;
        }

        /// <summary>
        /// Checks the average visibility of an area. 0 is fully unfogged and 1 if fully fogged.
        /// </summary>
        /// <param name="worldbounds"></param>
        public float VisibilityOfArea(Bounds worldbounds)
        {
            Vector2 min = FogOfWarConversion.WorldToFog(worldbounds.min, plane, mapOffset, mapResolution, mapSize);
            Vector2 max = FogOfWarConversion.WorldToFog(worldbounds.max, plane, mapOffset, mapResolution, mapSize);

            int xmin = Mathf.Clamp(Mathf.RoundToInt(min.x), 0, mapResolution.x);
            int xmax = Mathf.Clamp(Mathf.RoundToInt(max.x), 0, mapResolution.x);
            int ymin = Mathf.Clamp(Mathf.RoundToInt(min.y), 0, mapResolution.y);
            int ymax = Mathf.Clamp(Mathf.RoundToInt(max.y), 0, mapResolution.y);

            float total = 0;
            int count = 0;
            for (int y = ymin; y < ymax; ++y)
            {
                for (int x = xmin; x < xmax; ++x)
                {
                    ++count;
                    total += _fogValuesTotal[y * mapResolution.x + x] / 255.0f;
                }
            }

            return total / count;
        }

        /// <summary>
        /// Returns all of the visible units of a team that this team can see.
        /// </summary>
        /// <param name="teamindex"></param>
        /// <param name="maxfog"></param>
        /// <param name="units"></param>
        public void GetVisibleUnits(int teamindex, byte maxfog, List<FogOfWarUnit> units)
        {
            foreach (FogOfWarUnit unit in units)
            {
                if (unit.team == teamindex && GetFogValue(unit.transform.position) < maxfog)
                    units.Add(unit);
            }
        }

        void ProcessUnits(System.Diagnostics.Stopwatch stopwatch)
        {
            // if we are not updating units and all units have finished processing
            if (!updateUnits && _currentUnitProcessing >= FogOfWarUnit.registeredUnits.Count)
                return;

            // remove any invalid units
            FogOfWarUnit.registeredUnits.RemoveAll(u => u == null);

            double millisecondfrequency = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            for (; _currentUnitProcessing < FogOfWarUnit.registeredUnits.Count; ++_currentUnitProcessing)
            {
                if (!FogOfWarUnit.registeredUnits[_currentUnitProcessing].isActiveAndEnabled || FogOfWarUnit.registeredUnits[_currentUnitProcessing].team != team)
                    continue;

                FogOfWarShape shape = FogOfWarUnit.registeredUnits[_currentUnitProcessing].GetShape(this, physics, plane);
                if (_isMultithreaded)
                {
                    ++_drawThreadTaskPoolCount;
                    while (_drawThreadTaskPoolCount > _drawThreadTaskPool.Count)
                        _drawThreadTaskPool.Add(new FogOfWarDrawThreadTask());

                    FogOfWarDrawThreadTask task = _drawThreadTaskPool[_drawThreadTaskPoolCount - 1];
                    task.drawer = _drawer;
                    task.shape = shape;
                    _threadPool.Run(task);
                }
                else
                    _drawer.Draw(shape, false);

                // do the timer check here so that at least one unit will be processed
                if (stopwatch != null && _stopwatch.ElapsedTicks * millisecondfrequency >= maxMillisecondsPerFrame)
                {
                    ++_currentUnitProcessing;
                    break;
                }
            }
        }

        /// <summary>
        /// Forces the fog to update. This should only be called when updateAutomatically is true. You can manually call this from the editor by right-clicking the FogOfWar component.
        /// </summary>
        /// <param name="timesincelastupdate"></param>
        public void ManualUpdate(float timesincelastupdate)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Cannot do manual update when not playing!", this);
                return;
            }

            if (updateAutomatically && updateUnits)
            {
                Debug.LogWarning("Cannot do manual update when both updateAutomatically and updateMode are true!", this);
                return;
            }

            if (_isPerformingManualUpdate)
                return;

            _currentUnitProcessing = 0;
            _isPerformingManualUpdate = true; // flag for only one draw
            if (!updateUnits)
                _drawer.Clear(255);

            // multithreading will update on the next Update(), but single thread can do it now
            if (!_isMultithreaded)
            {
                ProcessUnits(null);
                CompileFinalTexture(ref timesincelastupdate, false);
            }
        }

        void Update()
        {
            if (!updateAutomatically && !_isPerformingManualUpdate)
                return;

            // prepare threads
            if (_isMultithreaded)
            {
                if (_threadPool == null)
                    _threadPool = new FogOfWarThreadPool();

                // do some thread maintenance
                threads = Mathf.Clamp(threads, 2, 8);
                _threadPool.maxThreads = threads;
                _threadPool.Clean();
            }
            else if (_threadPool != null)
            {
                _threadPool.StopAllThreads();
                _threadPool = null;
            }

            _stopwatch.Reset();
            _stopwatch.Start();

            // draw unit shapes
            ProcessUnits(_stopwatch);

            // compile final texture
            _timeSinceLastUpdate += Time.deltaTime;
            CompileFinalTexture(ref _timeSinceLastUpdate, true);

            _stopwatch.Stop();
        }

        static byte GetFadeAmount(float fadeduration, ref float fadeamount, float deltatime)
        {
            if (fadeduration > 0.0001f)
                fadeamount += deltatime / fadeduration;
            if (fadeamount > 1)
                fadeamount = 1;
            byte fadebytes = (byte)(fadeamount * 255);
            fadeamount -= fadebytes / 255.0f;
            return fadebytes;
        }

        void CompileFinalTexture(ref float timesincelastupdate, bool checkstopwatch)
        {
            // don't compile until all units have been processed
            if (_currentUnitProcessing < FogOfWarUnit.registeredUnits.Count || (checkstopwatch && _isMultithreaded && !_threadPool.hasAllFinished))
                return;

            onRenderFogTexture.Invoke();

            // get the fog values from the drawer
            // get current values from units (if updateUnits is false, this will retain what it have since the last time updateUnits was true)
            _drawer.GetValues(_fogValuesCurrent);

            // fade in fog
            byte fadeinbytes = GetFadeAmount(fadeInDuration, ref _fadeInAmount, timesincelastupdate);
            byte fadeoutbytes = GetFadeAmount(fadeOutDuration, ref _fadeOutAmount, timesincelastupdate);
            if (updateUnits || _hasFogChanged)
                _hasFogChanged = _drawer.Fade(_fogValuesCurrent, _fogValuesTotal, partialFogAmount, fadeinbytes, fadeoutbytes);

            if (updateUnits)
                _drawer.Clear(255);

            // prepare texture
            if (fogTexture == null)
            {
                fogTexture = new Texture2D(mapResolution.x, mapResolution.y, TextureFormat.Alpha8, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = filterMode
                };
                onTextureChanged?.Invoke();
            }
            else if (fogTexture.width != mapResolution.x || fogTexture.height != mapResolution.y)
                fogTexture.Reinitialize(mapResolution.x, mapResolution.y, TextureFormat.Alpha8, false);
            else
                fogTexture.filterMode = filterMode;
            if (outputToTexture)
            {
                fogTexture.LoadRawTextureData(_fogValuesTotal);
                fogTexture.Apply();

                // apply blur
                finalFogTexture = _blur.Apply(fogTexture, mapResolution, blurAmount, blurIterations, blurType);

                if (finalFogTexture != _lastFinalFogTexture)
                {
                    _lastFinalFogTexture = finalFogTexture;
                    onTextureChanged?.Invoke();
                }
            }

            if (updateUnits)
                _currentUnitProcessing = 0;
            timesincelastupdate = 0;
            _drawThreadTaskPoolCount = 0;
            _isPerformingManualUpdate = false; // manual update has finished
        }

        /// <summary>
        /// Applies all properties to the specified material that are required to detect fog for this team.
        /// See CustomFogShader.shader for more info.
        /// </summary>
        public void ApplyToMaterial(Material material, float outsidefogstrength = 1)
        {
            FoWIDs ids = FoWIDs.instance;

            material.SetTexture(ids.fogTex, finalFogTexture);
            material.SetVector(ids.fogTextureSize, mapResolution.ToFloat());
            material.SetFloat(ids.mapSize, mapSize);
            material.SetVector(ids.mapOffset, mapOffset);
            material.SetFloat(ids.outsideFogStrength, outsidefogstrength);

            // which plane will the fog be rendered to?
            material.SetKeywordEnabled("PLANE_XY", plane == FogOfWarPlane.XY);
            material.SetKeywordEnabled("PLANE_YZ", plane == FogOfWarPlane.YZ);
            material.SetKeywordEnabled("PLANE_XZ", plane == FogOfWarPlane.XZ);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            if (plane == FogOfWarPlane.XY)
                Gizmos.DrawWireCube(new Vector3(mapOffset.x, mapOffset.y, 0), new Vector3(mapSize, mapSize, 0));
            else if(plane == FogOfWarPlane.XZ)
                Gizmos.DrawWireCube(new Vector3(mapOffset.x, 0, mapOffset.y), new Vector3(mapSize, 0, mapSize));
            else if (plane == FogOfWarPlane.YZ)
                Gizmos.DrawWireCube(new Vector3(0, mapOffset.x, mapOffset.y), new Vector3(0, mapSize, mapSize));
        }
    }
}
