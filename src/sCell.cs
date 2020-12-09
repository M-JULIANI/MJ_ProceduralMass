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
            public bool isActive { get; set; }

            public sCell Parent;

            public sCell()
            {

            }

            public sCell(Vector3 location, double _resolution)
            {
                this._resolution = _resolution;
                this.location = location;
                int roundedX = (int)Math.Round(location.X);
                roundedX *= 5;
                int roundedY = (int)Math.Round(location.Y);
                roundedY *= 5;

                //index = new Vector2d(Math.Round(roundedX / this._resolution * 5.0), Math.Round(roundedX / this._resolution * 5.0));
  
                index = new Vector2dInt((int)Math.Round(location.X / this._resolution), (int)Math.Round(location.Y / this._resolution));

                rect = new BBox3(new Vector3(location.X, location.Y), new Vector3(location.X + this._resolution, location.Y + this._resolution));
                isActive = false;
            }
      }
}