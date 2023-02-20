using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS.Geom
{
    public class Polygon
    {

        public List<Contour> contours;
        public Rectangle bounds;

        public Polygon()
        {
            contours = new List<Contour>();
        }

        public Polygon(Point[] points)
        {
            Contour contour = new Contour(points);
            contours = new List<Contour>();
            contours.Add(contour);
        }

        public void Clear()
        {
            contours.Clear();
            bounds = null;
        }

        public Polygon Clone()
        {
            Polygon g = new Polygon();
            int contourCount = contours.Count;
            for (int k = 0; k < contourCount; k++)
            {
                g.AddContour(contours[k].Clone());
            }
            return g;
        }


        public Rectangle boundingBox
        {
            get
            {
                if (bounds != null)
                    return bounds;

                Rectangle bb = null;
                foreach (Contour c in contours)
                {
                    Rectangle cBB = c.boundingBox;
                    if (bb == null)
                        bb = cBB;
                    else
                        bb = bb.Union(cBB);
                }
                bounds = bb;
                return bounds;
            }
        }

        public void AddContour(Contour c)
        {
            contours.Add(c);
        }

    }

}