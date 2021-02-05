using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Analysis;
using System.Linq;
using Elements.Geometry.Solids;


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
            var siteModel = inputModels["Site"];
            var siteElement = siteModel.AllElementsOfType<Site>().First();
            var sitePerimeter = siteElement.Perimeter;
            var offsetPerimeter = sitePerimeter.Offset(-input.SiteSetback).OrderByDescending<Polygon, double>(s=>s.Area()).First();

            var envelopes = new List<Envelope>();
            var polys = new List<ModelCurve>();
            var elligibleCells = new List<sCell>();
            sGrid grid = null;
            List<sPolygon> smartPolys = new List<sPolygon>();
            List<ModelCurve> sketches = new List<ModelCurve>();
            try
            {

                bool obstaclesExist = input.ObstaclePolygons !=null? true: false;
                    grid = new sGrid(offsetPerimeter, input.CellSize, input.TargetCellCount, input.StartingLocation, input.MinHeight, input.MaxHeight, input.ObstaclePolygons);
                    grid.InitCells(obstaclesExist);


                //init start index
                elligibleCells = grid.cells.Values.Select(s => s).ToList();

                var totalCells = grid.cells.Count;
                int startIndex = (int)(elligibleCells.Count * input.StartingLocation);

                if (startIndex >= elligibleCells.Count)
                    startIndex = elligibleCells.Count - 1;

                var selected = elligibleCells[startIndex];

                Console.WriteLine("start index:" + startIndex);

                ///Main Run
                grid.Run(selected);


                /// <summary>
                /// jittered heights logic
                /// </summary>
                var branchCount = grid.treeRects.Count;
                var increment = 1.0 / branchCount;

                var rangeVals = new List<double>();
                for (int i = 0; i < branchCount + 1; i++)
                    rangeVals.Add(increment * i);

                var jitteredHeights = Jitter(rangeVals, input.HeightJitter);
                var jitterMin = jitteredHeights.Min();
                var jitterMax = jitteredHeights.Max();

                var remappedVals = new List<double>();
                for (int i = 0; i < jitteredHeights.Length; i++)
                {
                    var remapped = mapValue(jitteredHeights[i], jitterMin, jitterMax, input.MinHeight, input.MaxHeight);
                    remappedVals.Add(remapped);
                }

                Console.WriteLine("Overall branch count: " + grid.finalTree.Keys.Count);

                var keyList = grid.finalTree.Keys.Count;

                var envMatl = new Material("envelope", new Color(0.27, 0.73, 0.73, 0.6), 0.0f, 0.0f);
                
                int globalIndex = 0;
                for (int k = 0; k < grid.finalTree.Keys.Count; k++)
                {
                    var listOfCells = grid.finalTree.Values.ToArray();
                    var cellCount = listOfCells[k].Count;
                    for (int j = 0; j < cellCount; j++)
                    {
                        Console.WriteLine($"x: {listOfCells[k][j].index.X} y: {listOfCells[k][j].index.Y}");
                        sPolygon smPoly;
                        if(grid.GetOrthoActiveNeighbors(listOfCells[k][j], grid.cells).ToList().Count>0)
                        {
                        var poly = new Polygon(new List<Vector3>(){
                            new Vector3(listOfCells[k][j].rect.Min.X, listOfCells[k][j].rect.Min.Y),
                            new Vector3(listOfCells[k][j].rect.Min.X, listOfCells[k][j].rect.Max.Y),
                            new Vector3(listOfCells[k][j].rect.Max.X, listOfCells[k][j].rect.Max.Y),
                            new Vector3(listOfCells[k][j].rect.Max.X, listOfCells[k][j].rect.Min.Y)});

                        smPoly = new sPolygon(poly, remappedVals[k]);
                        smPoly.index = globalIndex;

                        smartPolys.Add(smPoly); 
                        sketches.Add(new ModelCurve(smPoly.polygon));
                        globalIndex++;
                        }  
                    }
                }


            ///height/ grouping logic
            var distinctHeights = smartPolys.Select(c => c.height).Distinct().OrderBy(d => d);
            var currBase = 0.0;
            foreach (var height in distinctHeights)
            {
                var clinesBelowHeight = smartPolys.Where(c => c.height >= height);
                var individualPolygons = new List<Polygon>();

                foreach (var pg in clinesBelowHeight)
                {
                    var thickened = pg.polygon;

                    if (thickened.IsClockWise())
                        thickened = thickened.Reversed();

                    individualPolygons.Add(thickened);
                }

                var union = individualPolygons.Count > 1 ? Polygon.UnionAll(individualPolygons) : individualPolygons;

                foreach (var polygon in union)
                {
                    var representation = new Representation(new SolidOperation[] { new Extrude(polygon, height - currBase, Vector3.ZAxis, false) });
                    var envelope = new Envelope(polygon, currBase, height - currBase, Vector3.ZAxis, 0, new Transform(0, 0, currBase), envMatl, representation, false, Guid.NewGuid(), "");

                    envelopes.Add(envelope);
                    
                }
                currBase = height;
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //site percentage cover.
            var siteCover = string.Format("{0}%", grid.grownTree.Count / (elligibleCells.Count * 1.0) * 100);

            var output = new MJProceduralMassOutputs(grid.grownTree.Count, siteCover, grid.cellSize);

            output.Model.AddElements(sketches);

            //envelopes
            output.Model.AddElements(envelopes);
            //site boundary curve
            output.Model.AddElement(new ModelCurve(offsetPerimeter));

            //obstacle outputs
            var grayMat = new Material("greenery", new Color(0.44, 0.44, 0.44, 0.6), 0.0f, 0.0f);

            output.Model.AddElements(input.ObstaclePolygons.Select(s => new Mass(s, 2, grayMat)));

            return output;
        }

        public static int[] Jitter(List<double> initList, double jitterFactor)
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

    public class sPolygon 
    {
        public Polygon polygon; 
        public double height;
        public int index {get; set;}


        public sPolygon(Polygon polygon, double height)
        {
            this.polygon = polygon;
            this.height = height;
        }
    }


}