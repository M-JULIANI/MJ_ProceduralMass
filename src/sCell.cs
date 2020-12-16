using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Analysis;
using System.Linq;

namespace MJProceduralMass
{
    public class sCell
    {
            public Vector3 location;
            public Vector2dInt index;
            public double _resolution;
            public BBox3 rect;
            public Polygon polyCrv;
            public bool isActive { get; set; }

            public sCell Parent;

            public sCell()
            {

            }

            public sCell(Vector3 location, double _resolution)
            {
                this._resolution = _resolution;
                this.location = location;
  
                index = new Vector2dInt((int)Math.Round(location.X / this._resolution), (int)Math.Round(location.Y / this._resolution));

                rect = new BBox3(new Vector3(location.X - (this._resolution * 0.5), location.Y - (this._resolution * 0.5)), new Vector3(location.X + (this._resolution * 0.5), location.Y + (this._resolution * 0.5)));
                this.polyCrv = new Polygon(new List<Vector3>()
                {
                     new Vector3(rect.Min.X, rect.Min.Y),
                     new Vector3(rect.Min.X, rect.Max.Y),
                     new Vector3(rect.Max.X, rect.Max.Y),
                     new Vector3(rect.Max.X, rect.Min.Y)
                  });
                isActive = false;
            }
      }
}