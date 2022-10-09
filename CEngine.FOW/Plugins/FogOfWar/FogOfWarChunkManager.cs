using UnityEngine;
using System.Collections.Generic;

namespace FoW
{
    class FogOfWarChunk
    {
        public Vector3Int coordinate;
        public byte[] fogData;
        public byte[] currentFogData;
    }

    [AddComponentMenu("FogOfWar/FogOfWarChunkManager")]
    [RequireComponent(typeof(FogOfWarTeam))]
    public class FogOfWarChunkManager : MonoBehaviour
    {
        public Transform followTransform;
        public bool rememberFog = true;
        public float verticalChunkSize = 10;
        public float verticalChunkOffset = 0;
        public Vector2 mapOffset = Vector2.zero;
        const byte _version = 0;

        List<FogOfWarChunk> _chunks = new List<FogOfWarChunk>();
        public int loadedChunkCount { get { return _chunks.Count; } }
        Vector3Int _loadedChunk;

        FogOfWarTeam _fogOfWar;
        int _mapResolution;
        int _valuesPerMap { get { return _mapResolution * _mapResolution; } }
        int _valuesPerChunk { get { return _valuesPerMap / 4; } }
        Vector3 _followPosition { get { return FogOfWarConversion.WorldToFogPlane3(followTransform.position, _fogOfWar.plane); } }

        void Start()
        {
            _fogOfWar = GetComponent<FogOfWarTeam>();
            if (_fogOfWar.mapResolution.x != _fogOfWar.mapResolution.y)
            {
                Debug.LogError("FogOfWarChunkManager requires FogOfWar Map Resolution to be square and a power of 2!");
                enabled = false;
                return;
            }

            _mapResolution = _fogOfWar.mapResolution.x;
            _fogOfWar.onRenderFogTexture.AddListener(OnRenderFog);

            ForceLoad();
        }

        public Vector3Int WorldPositionToChunk(Vector3 pos)
        {
            float halfmapsize = _fogOfWar.mapSize * 0.5f;
            Vector3Int chunk = new Vector3Int(
                Mathf.RoundToInt(pos.x / halfmapsize) - 1,
                Mathf.RoundToInt(pos.y / halfmapsize) - 1,
                Mathf.FloorToInt((pos.z - verticalChunkOffset) / verticalChunkSize)
            );
            if (pos.z - verticalChunkOffset < 0)
                --chunk.z;

            return chunk;
        }

        public Vector3 ChunkToWorldPosition(Vector3Int pos)
        {
            float halfmapsize = _fogOfWar.mapSize * 0.5f;
            return new Vector3(
                halfmapsize * (pos.x + 1),
                halfmapsize * (pos.y + 1),
                pos.z * verticalChunkSize + verticalChunkOffset
                );
        }

        void SaveChunk(byte[] currentdata, byte[] totaldata, int xc, int yc)
        {
            // reuse chunk if it already exists
            Vector3Int coordinate = _loadedChunk + new Vector3Int(xc, yc, 0);
            FogOfWarChunk chunk = _chunks.Find(c => c.coordinate == coordinate);
            if (chunk == null)
            {
                chunk = new FogOfWarChunk()
                {
                    coordinate = coordinate,
                    fogData = new byte[_valuesPerChunk],
                    currentFogData = new byte[_valuesPerChunk]
                };
                _chunks.Add(chunk);
            }
            else
            {
                if (chunk.fogData == null || chunk.fogData.Length != _valuesPerChunk)
                    chunk.fogData = new byte[_valuesPerChunk];
                if (chunk.currentFogData == null || chunk.currentFogData.Length != _valuesPerChunk)
                    chunk.currentFogData = new byte[_valuesPerChunk];
            }

            int halfmapsize = _mapResolution / 2;
            int xstart = halfmapsize * xc;
            int ystart = halfmapsize * yc;

            // copy values
            for (int y = 0; y < halfmapsize; ++y)
            {
                System.Array.Copy(totaldata, (ystart + y) * _mapResolution + xstart, chunk.fogData, y * halfmapsize, halfmapsize);
                System.Array.Copy(currentdata, (ystart + y) * _mapResolution + xstart, chunk.currentFogData, y * halfmapsize, halfmapsize);
            }
        }

        void SaveChunks()
        {
            // save all visible chunks
            byte[] currentdata = new byte[_valuesPerMap];
            _fogOfWar.GetCurrentFogValues(ref currentdata);

            byte[] totaldata = new byte[_valuesPerMap];
            _fogOfWar.GetTotalFogValues(ref totaldata);

            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 2; ++x)
                    SaveChunk(currentdata, totaldata, x, y);
            }
        }

        void LoadChunk(byte[] currentdata, byte[] totaldata, int xc, int yc)
        {
            // only load if the chunk exists
            Vector3Int coordinate = _loadedChunk + new Vector3Int(xc, yc, 0);
            FogOfWarChunk chunk = _chunks.Find(c => c.coordinate == coordinate);
            if (chunk == null || chunk.fogData == null || chunk.fogData.Length != _valuesPerChunk)
                return;

            int halfmapsize = _mapResolution / 2;
            int xstart = halfmapsize * xc;
            int ystart = halfmapsize * yc;

            // copy values
            for (int y = 0; y < halfmapsize; ++y)
            {
                System.Array.Copy(chunk.fogData, y * halfmapsize, totaldata, (ystart + y) * _mapResolution + xstart, halfmapsize);
                System.Array.Copy(chunk.currentFogData, y * halfmapsize, currentdata, (ystart + y) * _mapResolution + xstart, halfmapsize);
            }
        }

        void LoadChunks()
        {
            byte[] currentdata = new byte[_valuesPerMap];
            byte[] totaldata = new byte[_valuesPerMap];

            // set fog full by default
            for (int i = 0; i < currentdata.Length; ++i)
            {
                currentdata[i] = 255;
                totaldata[i] = 255;
            }

            // load each visible chunk
            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 2; ++x)
                    LoadChunk(currentdata, totaldata, x, y);
            }

            // put the new map into fow
            _fogOfWar.SetCurrentFogValues(currentdata);
            _fogOfWar.SetTotalFogValues(totaldata);
        }

        void ForceLoad()
        {
            if (followTransform == null)
                return;

            Vector3Int desiredchunk = WorldPositionToChunk(_followPosition);

            // move fow
            float chunksize = _fogOfWar.mapSize * 0.5f;
            _fogOfWar.mapOffset = new Vector2(desiredchunk.x, desiredchunk.y) * chunksize + Vector2.one * chunksize + mapOffset;
            _loadedChunk = desiredchunk;
            _fogOfWar.Reinitialize();

            LoadChunks();
        }

        void OnRenderFog()
        {
            if (followTransform == null)
                return;

            // is fow in the best position?
            if (WorldPositionToChunk(_followPosition) != _loadedChunk)
            {
                SaveChunks();
                ForceLoad();

                // clear memory 
                if (!rememberFog)
                    _chunks.Clear();
            }
        }

        public void Clear()
        {
            _chunks.Clear();
        }

        public byte[] Save()
        {
            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);

                writer.Write(_version);
                writer.Write(_valuesPerChunk);

                writer.Write(_chunks.Count);
                foreach (FogOfWarChunk chunk in _chunks)
                {
                    writer.Write(chunk.coordinate.x);
                    writer.Write(chunk.coordinate.y);
                    writer.Write(chunk.coordinate.z);
                    writer.Write(chunk.fogData.Length);
                    writer.Write(chunk.fogData);
                    writer.Write(chunk.currentFogData.Length);
                    writer.Write(chunk.currentFogData);
                }

                return stream.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public bool Load(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            try
            {
                System.IO.BinaryReader reader = new System.IO.BinaryReader(new System.IO.MemoryStream(data));

                byte version = reader.ReadByte();
                if (version != _version)
                {
                    Debug.LogWarningFormat(this, "Invalid FogOfWarChunkManager version (got {0}, expected {1}).", version, _version);
                    return false;
                }

                int valuesperchunk = reader.ReadInt32();
                if (_valuesPerChunk != valuesperchunk)
                {
                    Debug.LogWarning("FogOfWarChunkManager valuesPerChunk is different. This is probably due to the FogOfWarTeam having a different map resolution.", this);
                    return false;
                }

                int chunkcount = reader.ReadInt32();
                if (chunkcount < 0 || chunkcount > 99999)
                {
                    Debug.LogWarning("FogOfWarChunkManager recieved an invalid chunk count: " + chunkcount.ToString(), this);
                    return false;
                }

                _chunks.Clear();
                while (_chunks.Count < chunkcount)
                {
                    _chunks.Add(new FogOfWarChunk()
                    {
                        coordinate = new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                        fogData = reader.ReadBytes(reader.ReadInt32()),
                        currentFogData = reader.ReadBytes(reader.ReadInt32())
                    });
                }

                ForceLoad();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }
    }
}
