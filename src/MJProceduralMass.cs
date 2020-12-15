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

            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, 10);
            var center = input.SiteBoundary.Centroid();
            var analyze = new Func<Vector3, double>((v) =>
            {
              return center.DistanceTo(v);
            });

            Console.WriteLine("Greetings");
            var envelopes = new List<Envelope>();
             //trying stuff..
            var polys = new List<ModelCurve>();
            var elligibleCells = new List<sCell>();
            sGrid grid = null;
             try{
            
            grid = new sGrid(input.SiteBoundary, input.CellSize, input.TargetCellCount, input.StartingLocation, input.MinHeight, input.MaxHeight, colorScale, analyze, input.ObstaclePolygons);

            if(input.ObstaclePolygons == null)
                grid.InitCells(false);
            else
                grid.InitCells(true);
            
            
            //init start index
            elligibleCells = grid.cells.Values.Select(s => s).ToList();

            var totalCells = grid.cells.Count;
            int startIndex = (int)(elligibleCells.Count * input.StartingLocation);

            if (startIndex >= elligibleCells.Count)
                startIndex = elligibleCells.Count - 1;

            var selected = elligibleCells[startIndex];

            Console.WriteLine("start index:" + startIndex);

            grid.Run(selected);


            var branchCount = grid.treeRects.Count;
            var increment = 1.0/ branchCount;

            /////JITTERVALS
            var rangeVals = new List<double>();
            for (int i = 0; i < branchCount + 1; i++)
                rangeVals.Add(increment * i);

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
            }

            Console.WriteLine("Overall branch count: "+ grid.finalTree.Keys.Count);

                var keyList = grid.finalTree.Keys.Count;



                for (int k = 0; k < grid.finalTree.Keys.Count; k++)
                {
                    var tempPolys = new List<Polygon>();
                    var listOfCells = grid.finalTree.Values.ToArray();
                    var cellCount = listOfCells[k].Count;
                    for (int j = 0; j < cellCount; j++)
                    {
                        Console.WriteLine($"x: {listOfCells[k][j].index.X} y: {listOfCells[k][j].index.Y}");
                        var poly = new Polygon(new List<Vector3>(){
                 new Vector3(listOfCells[k][j].rect.Min.X, listOfCells[k][j].rect.Min.Y),
                 new Vector3(listOfCells[k][j].rect.Min.X, listOfCells[k][j].rect.Max.Y),
                  new Vector3(listOfCells[k][j].rect.Max.X, listOfCells[k][j].rect.Max.Y),
                  new Vector3(listOfCells[k][j].rect.Max.X, listOfCells[k][j].rect.Min.Y)});
                        tempPolys.Add(poly);
                    }
                    var polyUnioned = Polygon.UnionAll(tempPolys, 0.01);
                    polys.Add(new ModelCurve(polyUnioned[0]));

                    var envMatl = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);


                    var profile = new Profile(polyUnioned);
                    //trying stuff..
                    var extrude = new Elements.Geometry.Solids.Extrude(profile, remappedVals[k], Vector3.ZAxis, false);
                    var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });

                    envelopes.Add(new Envelope(profile, 0.0, remappedVals[k], Vector3.ZAxis, 0.0, new Transform(), envMatl, geomRep, false, Guid.NewGuid(), ""));

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            var siteCover = string.Format("{0}%", grid.grownTree.Count/(elligibleCells.Count * 1.0));
            var output = new MJProceduralMassOutputs(grid.grownTree.Count, siteCover);

            output.Model.AddElements(envelopes);

            //output.Model.AddElements(polys);

            output.Model.AddElement(new ModelCurve(input.SiteBoundary));

             var greenMat = new Material("greenery", new Color(0.329, 1.0, 0.239, 0.6), 0.0f, 0.0f);

            output.Model.AddElements(input.ObstaclePolygons.Select(s=>new Mass(s, 1, greenMat)));

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