using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Analysis;
using System.Linq;

namespace MJProceduralMass
{
     public class sGrid : GeometricElement
      {
      private List<(BBox3 cell, double value)> _results = new List<(BBox3 cell, double value)>();

       public SortedDictionary<Vector2dInt, sCell> cells = new SortedDictionary<Vector2dInt, sCell>();

       public List<sCell> grownTree;
       public Dictionary<int, List<sCell>> treeRects;
       public Dictionary<int, List<sCell>> finalTree;
       private double targetNumCells;
       public IList<Polygon> obstacles;
                /// <summary>
        /// The length of the cells in the u direction.
        /// </summary>
        public double cellSize { get; set; }

        public double startingParam {get; set;}
        public double minHeight {get; set;}
        public double maxHeight {get; set;}


        /// <summary>
        /// The perimeter of the analysis mesh.
        /// </summary>
        public Polygon Perimeter { get; set; }

        /// <summary>
        /// Construct an analysis mesh.
        /// </summary>
        /// <param name="perimeter">The perimeter of the mesh.</param>
        /// <param name="uLength">The number of divisions in the u direction.</param>
        /// <param name="vLength">The number of divisions in the v direction.</param>
        /// <param name="colorScale">The color scale to be used in the visualization.</param>
        /// <param name="analyze">A function which takes a location and computes a value.</param>
        /// <param name="id">The id of the analysis mesh.</param>
        /// <param name="name">The name of the analysis mesh.</param>
        public sGrid(Polygon perimeter,
                            double cellSize,
                            double targetNumCells,
                            double startingParam,
                            double minHeight,
                            double maxHeight,
                            IList<Polygon> obstacles,
                            Guid id = default(Guid),
                            string name = null) : base(new Transform(),
                                                       BuiltInMaterials.Default,
                                                       null,
                                                       false,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this.Perimeter = perimeter;
            this.cellSize = cellSize;
            this.obstacles = obstacles;
            this.targetNumCells = targetNumCells;
            this.startingParam = startingParam;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.Material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, null, true, true, Guid.NewGuid());
        }

           public sGrid(Polygon perimeter,
                            double cellSize,
                            double targetNumCells,
                            double startingParam,
                            double minHeight,
                            double maxHeight,
                            Guid id = default(Guid),
                            string name = null) : base(new Transform(),
                                                       BuiltInMaterials.Default,
                                                       null,
                                                       false,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this.Perimeter = perimeter;
            this.cellSize = cellSize;
            //this.obstacles = null;
            this.targetNumCells = targetNumCells;
            this.startingParam = startingParam;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.Material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, null, true, true, Guid.NewGuid());
        }

#region initiating cells

        /// <summary>
        /// Initializes a grid2d given a starting index and a site boundary.
        /// </summary>
        /// <param name="obstaclesExist"></param>
        public void InitCells(bool obstaclesExist)
        {
          var bounds = new BBox3(new[] { this.Perimeter });
            var x = bounds.Min.X;
            var y = bounds.Min.Y;

            var minPoint = bounds.Min;

            int max = 100;
            int min = 5;
            var rangeX = bounds.Max.X - bounds.Min.X;
            var rangeY = bounds.Max.Y - bounds.Min.Y;


            int divsX = (int)(rangeX / cellSize) + 1;
            int divsY = (int)(rangeY / cellSize) + 1;

            if (divsX > max)
            {
                divsX = max;
                cellSize = rangeX / divsX * 1f;
            }

            if (divsY > max)
            {
                divsY = max;
                cellSize = rangeY / divsY * 1f;
            }

            if (divsX < min)
            {
                divsX = min;
                cellSize = rangeX / divsX * 1f;
            }

            if (divsY < min)
            {
                divsY = min;
                cellSize = rangeY / divsY * 1f;
            }

            divsX = (int)(rangeX / cellSize) + 1;
            divsY = (int)(rangeY / cellSize) + 1;

            Vector3[,] initStorage = new Vector3[divsX, divsY];


            for (int i = 0; i < divsX; i++)
                for (int j = 0; j < divsY; j++)
                {
                    var samplePt = new Vector3(i * cellSize + minPoint.X, j * cellSize + minPoint.Y);
                    var roundedPt = new Vector3((int)Math.Round(samplePt.X, 0, MidpointRounding.AwayFromZero),
                      (int)Math.Round(samplePt.Y, 0, MidpointRounding.AwayFromZero));
                    var loc = new Vector3((samplePt.X), (samplePt.Y));
                    var index = new Vector2dInt((int)Math.Round(roundedPt.X / cellSize), (int)Math.Round(roundedPt.Y /cellSize));

                    if(!Perimeter.Contains(samplePt))
                        continue;

                    else
                    {
                        if (obstaclesExist == false)
                        {
                            var _cell = new sCell(loc, cellSize);
                            sCell cellExisting;
                            if (cells.TryGetValue(_cell.index, out cellExisting))
                                continue;
                            else
                                cells.Add(_cell.index, _cell);
                        }
                        else
                        {
                            if (InsideCrvsGroup(samplePt, obstacles) == true)
                                continue;
                            else
                            {
                                for (int c = 0; c < obstacles.Count; c++)
                                {
                                    if (!obstacles[c].Contains(samplePt))
                                    {
                                        var _cell = new sCell(loc, cellSize);
                                        sCell cellExisting;
                                        if (cells.TryGetValue(_cell.index, out cellExisting))
                                            continue;
                                        else
                                            cells.Add(_cell.index, _cell);
                                    }
                                }
                            }
                        }
                    }
                }
        }

        #endregion

        /// <summary>
        /// Main 'grow' function given a dictionary, a starting point.
        /// </summary>
        /// <param name="startCell"></param>
        /// <returns></returns>
        public bool Run(sCell startCell)
        {
            bool done = false;
            int count = 0;

            grownTree = new List<sCell>();
            var localTreeList = new List<sCell>();

            localTreeList.Add(startCell);

            sCell currentCell;

            while (localTreeList.Count > 0)
            {
                if (count >= targetNumCells)
                    goto ProcessDict;

                sCell validCell;
                if (ReturnOne(localTreeList, cells, grownTree, out validCell)){
                    currentCell = validCell;
                    // Console.WriteLine($"current: x: {currentCell.index.X}, y:{currentCell.index.Y}, valid:{validCell.index.X}, y:{validCell.index.Y} ");
                }
                else
                    goto ProcessDict;

                if (cells.TryGetValue(currentCell.index, out validCell))
                    validCell.isActive = true;

                string dis = "";
                var validNeighborCells = CheckPlacementLocations(currentCell, cells, grownTree, out dis);

                sCell actualCell;
                if (validNeighborCells.Count > 0)
                {

                    localTreeList.AddRange(validNeighborCells);
                    localTreeList.Remove(currentCell);

                    foreach (var vn in validNeighborCells)
                    {
                        if (cells.TryGetValue(vn.index, out actualCell) && actualCell.isActive == false)
                            actualCell.isActive = true;
                    }
                    grownTree.Add(currentCell);
                }
                count++;
            }

        ProcessDict:
            {
                var valList = cells.Values.ToList();
                var keyList = cells.Keys.ToList();

                var tempDict = new SortedDictionary<Vector2dInt, sCell>();
                for (int i = 0; i < valList.Count; i++)
                {
                    if (grownTree.Contains(valList[i]))
                    {
                        tempDict.Add(keyList[i], valList[i]);
                    }
                    else
                    {
                        var tempCell = valList[i];
                        tempCell.isActive = false;
                        tempDict.Add(keyList[i], tempCell);
                    }
                }
                cells = tempDict;
                ProcessGroups(cells);
            }

            return done;
        }


#region Support Functions

        /// <summary>
        /// Checks to see if a given Vector3 is inside a group of curves
        /// </summary>
        /// <param name="samplePt"></param>
        /// <param name="obstacleCrvs"></param>
        /// <returns></returns>
        private bool InsideCrvsGroup(Vector3 samplePt, IList<Polygon> obstacleCrvs)
        {
            bool invalid = false;
            for (int i = 0; i < obstacleCrvs.Count; i++)
                if (obstacleCrvs[i].Contains(samplePt))
                    return true;
            return invalid;
        }

        /// <summary>
        /// This logic looks ahead in the dictionary to locations which would be allowed or prohibited.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dict"></param>
        /// <param name="listCells"></param>
        /// <param name="debugger"></param>
        /// <returns></returns>
        public List<sCell> CheckPlacementLocations(sCell cell, SortedDictionary<Vector2dInt, sCell> dict, List<sCell> listCells, out string debugger)
        {
            var neighbors = new List<sCell>();
            debugger = "all not so good";

            for (int yi = -1; yi <= 1; yi++)
            {
                int y = (int)(yi + cell.index.Y);

                for (int xi = -1; xi <= 1; xi++)
                {
                    int x = (int)(xi + cell.index.X);

                    var i = new Vector2dInt(x, y);

                    if (cell.index == i) continue;

                    sCell voxel;


                    if (dict.TryGetValue(i, out voxel))
                    {
                        if ((Math.Abs(xi * yi) == 0))
                        {
                            var xDelta = Math.Abs(cell.index.X - voxel.index.X) != 0 ? true : false;

                            debugger = xDelta.ToString();

                            sCell localNeigh;
                            Vector2dInt[] l_neighbors = new Vector2dInt[2];
                            int clearanceCounter = 0;

                            if (xDelta)
                            {
                                l_neighbors[0] = new Vector2dInt(x, y - 1);
                                l_neighbors[1] = new Vector2dInt(x, y + 1);
                            }
                            else
                            {
                                l_neighbors[0] = new Vector2dInt(x - 1, y);
                                l_neighbors[1] = new Vector2dInt(x + 1, y);
                            }

                            for (int ln = 0; ln < l_neighbors.Length; ln++)
                                if (dict.TryGetValue(l_neighbors[ln], out localNeigh) && localNeigh.isActive == true)
                                    clearanceCounter++;

                            if (clearanceCounter < 1)
                                neighbors.Add(voxel);
                        }
                    }
                }
            }
            return neighbors;


        }

         public bool ReturnOne(List<sCell> listCells, SortedDictionary<Vector2dInt, sCell> dict, List<sCell> cellsCheckAgainst, out sCell outputCell)
        {
            bool canDo = false;
            outputCell = new sCell();
            for (int i = 0; i < listCells.Count; i++)
            {
                string dis = "";
                if (CheckPlacementLocations(listCells[i], dict, cellsCheckAgainst, out dis).Count > 0)
                {
                    outputCell = listCells[i];
                    canDo = true;
                    return canDo;
                }
            }
            return canDo;

        }

        /// <summary>
        /// Processes branches and turns them into clusters.
        /// </summary>
        /// <param name="dict"></param>
         public void ProcessGroups(SortedDictionary<Vector2dInt, sCell> dict)
        {
            treeRects = new Dictionary<int, List<sCell>>();
            var tempList = new List<sCell>();

            var newDict = new SortedDictionary<Vector2dInt, sCell>();

            foreach (KeyValuePair<Vector2dInt, sCell> pair in dict)
                newDict.Add(pair.Key, pair.Value);


            tempList.AddRange(newDict.Values.Where(s => s.isActive).ToList());

            sCell current = newDict.Values.Where(s => s.isActive).ToList()[0];

            int count = 0;
     
            while (tempList.Count > 0)
            {
                current = tempList[0];
                var neighbors = GetOrthoActiveNeighbors(current, newDict);
                //var internalList = current;
                
                if (neighbors.Count > 0)
                {
                    //internalList.Add(current);
                    List<sCell> existingList;
                    if (treeRects.TryGetValue(count, out existingList))
                    {
                        existingList.Add(current);
                        treeRects[count] = existingList;
                    }
                    else
                        treeRects.Add(count, new List<sCell>(){current});

                    
                    tempList.Remove(current);
                    newDict.Remove(current.index);
                    var neighRects = neighbors.Select(s => s).ToList();

                    if(treeRects.TryGetValue(count, out existingList))
                    {
                        existingList.AddRange(neighRects);
                        treeRects[count] = existingList;
                    }
                    else
                        treeRects.Add(count, neighRects);

                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        newDict.Remove(neighbors[i].index);
                        tempList.Remove(neighbors[i]);
                    }
                }
                else
                {
                    //internalList.Add(current);

                    List<sCell> existingList;
                    if(treeRects.TryGetValue(count, out existingList))
                    {
                        existingList.Add(current);
                        treeRects[count] = existingList;
                    }
                    else
                        treeRects.Add(count, new List<sCell>(){current});

                    tempList.Remove(current);
                    newDict.Remove(current.index);
                }

                count++;
            }

            finalTree = new Dictionary<int, List<sCell>>();

            var valArray = treeRects.Values.ToArray();
            for (int i = 0; i < treeRects.Keys.Count; i++)
            {
                if(valArray[i].Count == 1)
                {
                    var index = FindNearestTreeIndex(valArray[i][0].rect, treeRects);

                     if(index==-1)
                         continue;

                    List<sCell> existingList;
                    if (finalTree.TryGetValue(index, out existingList))
                    {
                        existingList.Add(valArray[i][0]);
                        finalTree[index] = existingList;
                    }
                    else
                    {
                        var newList = new List<sCell>();
                        newList.Add(valArray[i][0]);
                        finalTree.Add(index, newList);
                    }
                }

                else
                {
                     List<sCell> existingList;
                    if (finalTree.TryGetValue(i, out existingList))
                    {
                        existingList.AddRange(valArray[i]);
                        finalTree[i] = existingList;
                    }
                    else{

                        var newList = new List<sCell>();
                        newList.AddRange(valArray[i]);
                        finalTree.Add(i, newList);
                    }
                }
            }
        }

        /// <summary>
        /// Finds nearest grouping for mass clustering.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
          public int FindNearestTreeIndex(BBox3 rect, Dictionary <int, List<sCell>> tree)
        {
            int val = -1;

            List<PointSorter> pts = new List<PointSorter>();
            int count = 0;

            foreach(KeyValuePair<int, List<sCell>> kp in tree)
            {
                for(int k = 0; k< kp.Value.Count; k++)
                {
                    var dist = rect.Center().DistanceTo(kp.Value[k].rect.Center());
                    if (dist < 0.01)
                        continue;

                    pts.Add(new PointSorter(kp.Value[k].rect, dist, count));
                    count++;
                }
            }


            var closest = pts.OrderBy(s => s.dist).Select(i => i.index).ToList();

            val = closest[0];
            return val;
        }

        /// <summary>
        /// Finds ortho and active neighbors for deciding branching growth.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public List<sCell> GetOrthoActiveNeighbors(sCell cell, SortedDictionary<Vector2dInt, sCell> dict)
        {
            var neighbors = new List<sCell>();

            for (int yi = -1; yi <= 1; yi++)
            {
                int y = (int)(yi + cell.index.Y);

                for (int xi = -1; xi <= 1; xi++)
                {
                    int x = (int)(xi + cell.index.X);

                    var i = new Vector2dInt(x, y);

                    if (cell.index == i) continue;

                    sCell voxel;

                    if (dict.TryGetValue(i, out voxel))
                    {
                        if ((Math.Abs(xi * yi) == 0) && voxel.isActive == true)
                            neighbors.Add(voxel);
                    }
                }
            }
            return neighbors;
        }

        #endregion
    }
}