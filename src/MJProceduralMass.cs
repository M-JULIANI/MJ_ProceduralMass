using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Analysis;
using System.Linq;

namespace MJProceduralMass
{
      public static class MJProceduralMass
    {
        /// <summary>
        /// Generates a procedural massing for a residential/office building.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A MJProceduralMassOutputs instance containing computed results and the model with any new elements.</returns>
        public static MJProceduralMassOutputs Execute(Dictionary<string, Model> inputModels, MJProceduralMassInputs input)
        {
             // Construct a color scale from a small number
            // of colors.
            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, 10);
            var center = input.SiteBoundary.Centroid();
            var analyze = new Func<Vector3, double>((v) =>
            {
              return center.DistanceTo(v);
            });
            
            sGrid grid = new sGrid(input.SiteBoundary, input.CellSize, input.TargetCellCount, input.StartingLocation, input.MinHeight, input.MaxHeight, colorScale, analyze);

            grid.InitCells();
            
            
            //init start index
            var elligibleCells = grid.cells.Values.Select(s => s).ToList();
            var totalCells = grid.cells.Count;
            int startIndex = (int)(elligibleCells.Count * input.StartingLocation);

            if (startIndex >= elligibleCells.Count)
                startIndex = elligibleCells.Count - 1;

            var selected = elligibleCells[startIndex];

            grid.Run(selected);

            // var perimeter2 = Polygon.Ngon(5, 5);
            // var move = new Transform(3, 7, 0);
            // var perimeter = perimeter1.Union((Polygon)perimeter2.Transformed(move));
            // var mc = new ModelCurve(perimeter);

            var branchCount = grid.finalTree.Count;
            var increment = 1.0/ branchCount;

            /////JITTERVALS
            var rangeVals = new List<double>();
            //RhinoApp.WriteLine("range vals:");
            for (int i = 0; i < branchCount + 1; i++)
            {
                rangeVals.Add(increment * i);

                // RhinoApp.WriteLine((increment * i).ToString());
            }

            var jitteredHeights = Jitter(rangeVals, input.HeightJitter * 0.01);
            var jitterMin = jitteredHeights.Min();
            var jitterMax = jitteredHeights.Max();

            var remappedVals = new List<double>();
            // RhinoApp.WriteLine("remapped vals:");
            for (int i = 0; i < jitteredHeights.Length; i++)
            {
                // RhinoApp.WriteLine("raw jitter: " + (jitteredHeights[i]).ToString());
                var remapped = mapValue(jitteredHeights[i], jitterMin, jitterMax, input.MinHeight, input.MaxHeight);
                remappedVals.Add(remapped);

                // RhinoApp.WriteLine((remapped).ToString());
            }

            //   var Centerline = input.Centerline;
            // var perimeter = Centerline.Offset(input.BarWidth / 2, EndType.Butt).First();

            var envelopes = new List<Envelope>();
            for (int k = 0; k < grid.finalTree.Count; k++)
            {
                var polyUnioned = Polygon.UnionAll(grid.finalTree[k].Select(s => new Polygon(new List<Vector3>(){
                 new Vector3(s.rect.Min.X, s.rect.Min.Y, 0),
                 new Vector3(s.rect.Min.X, s.rect.Max.Y, 0),
                  new Vector3(s.rect.Max.X, s.rect.Max.Y, 0),
                  new Vector3(s.rect.Max.X, s.rect.Min.Y, 0)
             })).ToArray(), 0.1);

                var profile = new Profile(polyUnioned);

                var extrude = new Elements.Geometry.Solids.Extrude(profile, remappedVals[k], Vector3.ZAxis, false);
                var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });

                var envMatl = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);


                envelopes.Add(new Envelope(profile, 0.0, remappedVals[k], Vector3.ZAxis, 0.0, new Transform(), envMatl, geomRep, false, Guid.NewGuid(), ""));
            }

            var siteCover = elligibleCells.Count/(totalCells * 1.0);
            var output = new MJProceduralMassOutputs(elligibleCells.Count, siteCover);

            //output.Model.AddElement(mc);

             output.Model.AddElements(envelopes);
            //var sketch = new Sketch(input.Centerline, Guid.NewGuid(), //"Centerline Sketch");
            //output.Model.AddElement(sketch);
            //return output;

            // Construct a mass from which we will measure
            // distance to the analysis mesh's cells.
           // var mass = new Mass(Polygon.Rectangle(1, 1));
            
           
            //mass.Transform.Move(center);
            //output.Model.AddElement(mass);

            // The analyze function computes the distance
            // to the attractor.


         

            //var analysisMesh = new AnalysisMesh(perimeter1, 0.2, 0.2, //colorScale, analyze);
            //analysisMesh.Analyze();

            return output;
        }

        public static int [] Jitter(List<double> initList, double jitterFactor)
        {
            int[] array = new int[initList.Count];

            double maxDistance = jitterFactor * array.Length;
            Random r = new Random(42);

            for (int i = 0; i < array.Length; i++)
            {
                double min = Math.Max(i - maxDistance, 0);
                double max = Math.Min(i + maxDistance, array.Length);

                var item = r.Next((int)min, (int)max);
                array[i] = item;
            }

            return array;
        }

        public static double mapValue(double mainValue, double inValueMin, double inValueMax, double outValueMin, double outValueMax)
        {
            return (mainValue - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
        }
    }


    /// <summary>
    /// point sorting class
    /// </summary>
    public class PointSorter
    {

        public BBox3 rect;
        public double dist;
        public int index;

        public PointSorter(BBox3 rect, double dist, int index)
        {
            this.rect = rect;
            this.dist = dist;
            this.index = index;
        }

    }

     
}