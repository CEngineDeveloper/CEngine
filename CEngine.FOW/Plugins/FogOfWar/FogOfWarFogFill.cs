using UnityEngine;
/*
namespace FoW
{
    // FogFill is a single request to clear an area of the fog.
    public class FogFill
    {
        public FogOfWar fogOfWar { get; private set; }

        public Vector2i fogPosition { get; private set; }
        public Vector3 worldPosition { get; private set; }
        public Vector2 forward { get; private set; }
        public float dotAngle { get; private set; }

        public int xStart { get; private set; }
        public int xEnd { get; private set; }
        public int yStart { get; private set; }
        public int yEnd { get; private set; }

        public int fogRadius { get; private set; }
        public float worldRadius { get; private set; }
        public int fogRadiusSqr { get; private set; }
        public int innerRadius { get; private set; }
        public int innerRadiusSqr { get; private set; }

        public FogFill(FogOfWar fow, Vector3 worldpos, float worldradius, float viewangle, Vector3 worldforward)
        {
            fogOfWar = fow;
            worldPosition = worldpos;
            worldRadius = worldradius;

            fogPosition = fow.WorldPositionToFogPosition(worldpos);
            fogRadius = (int)(worldRadius * fow.mapResolution / fow.mapSize);
            dotAngle = 1.0f - viewangle / 90;
            forward = fow.WorldToFogPlane(worldforward);

            xStart = Mathf.Clamp(fogPosition.x - fogRadius, 0, fow.mapResolution);
            xEnd = Mathf.Clamp(xStart + fogRadius + fogRadius, 0, fow.mapResolution);
            yStart = Mathf.Clamp(fogPosition.y - fogRadius, 0, fow.mapResolution);
            yEnd = Mathf.Clamp(yStart + fogRadius + fogRadius, 0, fow.mapResolution);

            fogRadiusSqr = fogRadius * fogRadius;
            innerRadius = (int)((1.0f - fow.fogEdgeRadius) * fogRadius);
            innerRadiusSqr = innerRadius * innerRadius;
        }

        bool IsInViewCone(Vector2 dir)
        {
            if (dotAngle <= -0.99f)
                return true;
            return Vector2.Dot(dir, forward) > dotAngle;
        }

        bool Raycast(float sqrdistance, Vector2i offset, int layermask, out float distance)
        {
            distance = 0.0f;
            if (fogOfWar.physics != FogOfWarPhysics.None)
            {
                float hitdistance = Mathf.Sqrt(sqrdistance) * fogOfWar.pixelSize;

                if (fogOfWar.physics == FogOfWarPhysics.Physics2D)
                {
                    RaycastHit2D hit = Physics2D.Raycast(worldPosition, offset.vector2, hitdistance, layermask);
                    if (hit.collider == null)
                        return false;
                    distance = hit.distance;
                }
                else if (fogOfWar.physics == FogOfWarPhysics.Physics3D)
                {
                    RaycastHit hit;
                    if (!Physics.Raycast(worldPosition, new Vector3(offset.x, 0, offset.y), out hit, hitdistance, layermask))
                        return false;
                    distance = hit.distance;
                }
                else
                    Debug.LogError("FogOfWarPhysics is an invalid value!");
            }
            return true;
        }

        public void UnfogCircle(ColliderFogRectList excluderects, int layermask)
        {
            byte[] values = fogOfWar.fogValues;
            for (int y = yStart; y < yEnd; ++y)
            {
                for (int x = xStart; x < xEnd; ++x)
                {
                    int index = y * fogOfWar.mapResolution + x;

                    // do nothing if it is already completely unfogged
                    if (values[index] == 0)
                        continue;

                    Vector2i offset = new Vector2i(x - fogPosition.x, y - fogPosition.y);

                    // do nothing if too far too see
                    int sqrdistance = offset.sqrMagnitude;
                    if (sqrdistance >= fogRadiusSqr)
                        continue;

                    // is this point in the view cone?
                    Vector2 dir = offset.normalized;
                    if (!IsInViewCone(dir))
                        continue;

                    // if it could be hidden, raycast to make sure
                    if (excluderects != null && (!fogOfWar.experimentalLineOfSightOptimisation || excluderects.Contains(new Vector2i(x, y))))
                    {
                        // perform raycast
                        float hitdistance;
                        if (Raycast(sqrdistance, offset, layermask, out hitdistance))
                        {

                            // optimisation
                            if (fogOfWar.fieldOfViewPenetration == 0.0f)
                                continue;

                            // offset the pixel back so that we can see what we are looking at
                            // TODO: This could be optimised by keeping it as ints
                            float pixeldistsqr = new Vector2(offset.x * fogOfWar.pixelSize, offset.y * fogOfWar.pixelSize).sqrMagnitude;
                            float penetration = Mathf.Min(hitdistance + fogOfWar.fieldOfViewPenetration, worldRadius);
                            if (pixeldistsqr >= penetration * penetration)
                                continue;
                        }
                    }

                    // fully unfogged
                    if (sqrdistance <= innerRadiusSqr)
                        values[index] = 0;

                    // partially fogged (lerp between innerradius and radius)
                    else// if (sqrdistance <= radiusSqr)
                    {
                        float t = (Mathf.Sqrt(sqrdistance) - innerRadius) / (fogRadius - innerRadius);
                        byte v = (byte)Mathf.Lerp(0, 255, t);
                        if (v < values[index])
                            values[index] = v;
                    }
                }
            }
        }
    }
}*/