using UnityEngine;
using System.Collections.Generic;

namespace FoW
{
    // ColliderFogRect represents the bounds of a collider within the fog texture space (in pixels).
    // This is used to optimise line of sight.
    public class ColliderFogRect
    {
        public Vector2Int position;
        public Vector2Int size;
        public Vector2 center { get { return new Vector2(position.x, position.y) + new Vector2(size.x, size.y) * 0.5f; } }
        public int xMin { get { return position.x; } set { size.x -= value - position.x; position.x = value; } }
        public int yMin { get { return position.y; } set { size.y -= value - position.y; position.y = value; } }
        public int xMax { get { return position.x + size.x; } set { size.x = value - position.x; } }
        public int yMax { get { return position.y + size.y; } set { size.y = value - position.y; } }

        public ColliderFogRect(Collider c, FogOfWarTeam fow)
        {
            Bounds b = c.bounds;
            position = fow.WorldPositionToFogPosition(b.min);
            size = fow.WorldPositionToFogPosition(b.max) - position;
        }

        public ColliderFogRect(Collider2D c, FogOfWarTeam fow)
        {
            Bounds b = c.bounds;
            position = fow.WorldPositionToFogPosition(b.min);
            size = fow.WorldPositionToFogPosition(b.max) - position;
        }

        public bool Contains(Vector2Int p)
        {
            return p.x >= xMin && p.x <= xMax && p.y >= yMin && p.y <= yMax;
        }

        public bool Contains(ColliderFogRect other)
        {
            return other.xMin >= xMin && other.xMax <= xMax &&
                other.yMin >= yMin && other.yMax <= yMax;
        }

        public bool ContainsCircle(Vector2Int p, int r)
        {
            return p.x - r >= xMin && p.x + r <= xMax &&
                p.y - r >= yMin && p.y + r <= yMax;
        }

        public void ExtendToCircleEdge(Vector2Int p, int radius)
        {
            if (xMin < p.x)
                xMin = p.x - radius;
            if (xMax > p.x)
                xMax = p.x + radius;
            if (yMin < p.y)
                yMin = p.y - radius;
            if (yMax > p.y)
                yMax = p.y + radius;
        }
    }

    // ColliderFogRectList is a container of ColliderFogRects.
    // This makes it nice and easy to add large amounts of colliders at once.
    public class ColliderFogRectList : List<ColliderFogRect>
    {
        public FogOfWarTeam fogOfWar { get; private set; }

        public ColliderFogRectList(FogOfWarTeam fow)
        {
            fogOfWar = fow;
        }

        public void Add(params Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; ++i)
                Add(new ColliderFogRect(colliders[i], fogOfWar));
        }

        public void Add(params Collider2D[] colliders)
        {
            for (int i = 0; i < colliders.Length; ++i)
                Add(new ColliderFogRect(colliders[i], fogOfWar));
        }

        public bool Contains(Vector2Int p)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].Contains(p))
                    return true;
            }
            return false;
        }

        public void Optimise()
        {
            // remove any rects that are contained within eachother
            RemoveAll(r => { for (int i = 0; i < Count; ++i) { if (this[i] != r && this[i].Contains(r)) return true; } return false; });
        }

        public void ExtendToCircleEdge(Vector2Int p, int radius)
        {
            for (int i = 0; i < Count; ++i)
                this[i].ExtendToCircleEdge(p, radius);
        }
    }
}