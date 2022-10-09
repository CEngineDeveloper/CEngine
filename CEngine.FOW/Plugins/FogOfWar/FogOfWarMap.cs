using UnityEngine;

namespace FoW
{
    // Describes a snapshot of the fog of war settings for another thread
    public class FogOfWarMap
    {
        public Vector2Int resolution;
        public float size;
        public Vector2 offset;
        public float pixelSize;
        public int pixelCount;
        public FogOfWarPlane plane;
        public FogOfWarPhysics physics;
        public FilterMode filterMode;
        public bool multithreaded;

        public FogOfWarMap(FogOfWarTeam fow)
        {
            Set(fow);
        }

        public void Set(FogOfWarTeam fow)
        {
            resolution = fow.mapResolution;
            size = fow.mapSize;
            offset = fow.mapOffset;
            pixelSize = resolution.x / size;
            pixelCount = resolution.x * resolution.y;
            plane = fow.plane;
            physics = fow.physics;
            filterMode = fow.filterMode;
            multithreaded = fow.multithreaded;
        }
    }
}
