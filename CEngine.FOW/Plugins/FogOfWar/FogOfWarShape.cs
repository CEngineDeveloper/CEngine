using UnityEngine;

namespace FoW
{
    public abstract class FogOfWarShape
    {
        public Vector3 eyePosition;
        public Vector2 foward;
        public bool absoluteOffset;
        public Vector2 offset;
        public float[] lineOfSight;
        public float lineOfSightMinAngle;
        public float lineOfSightMaxAngle;
        public bool lineOfSightSeeOutsideRange;
        public bool[] visibleCells;
        public int visibleCellsWidth;
        public float radius;
        public float brightness;
        public byte maxBrightness { get { return (byte)((1 - brightness) * 255); } }
        public Vector2 size;

        public abstract float CalculateMaxLineOfSightDistance();
        public abstract Vector2 CalculateRadius();
    }

    public class FogOfWarShapeCircle : FogOfWarShape
    {
        public float innerRadius;
        public float angle;

        public override float CalculateMaxLineOfSightDistance()
        {
            return offset.magnitude + radius;
        }

        public override Vector2 CalculateRadius()
        {
            return new Vector2(radius, radius);
        }

        public byte GetFalloff(float normdist)
        {
            if (normdist < innerRadius)
                return maxBrightness;
            float v = Mathf.InverseLerp(innerRadius, 1, normdist);
            v = 1 - (1 - v) * brightness;
            return (byte)(v * 255);
        }
    }

    public class FogOfWarShapeBox : FogOfWarShape
    {
        public Texture2D texture;
        public bool hasTexture = false; // this is required for multithreading because == will use unity stuff!
        public bool rotateToForward = false;

        public override float CalculateMaxLineOfSightDistance()
        {
            return offset.magnitude + size.magnitude * 0.5f;
        }
        
        public override Vector2 CalculateRadius()
        {
            if (rotateToForward)
            {
                float r = size.magnitude * 0.5f;
                return new Vector2(r, r);
            }
            else
                return size * 0.5f;
        }
    }

    public class FogOfWarShapeMesh : FogOfWarShape
    {
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] indices;

        public override float CalculateMaxLineOfSightDistance()
        {
            return radius;
        }

        public override Vector2 CalculateRadius()
        {
            return new Vector2(radius, radius);
        }
    }
}
