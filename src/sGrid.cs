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
       private List<List<sCell>> treeRects;
       public List<List<sCell>> finalTree;
       private double targetNumCells;
        private Func<Vector3, double> _analyze;
        private double _min = double.MaxValue;
        private double _max = double.MinValue;

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
        /// The color scale used to represent this analysis mesh.
        /// </summary>
        public ColorScale ColorScale { get; set; }

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
                            ColorScale colorScale,
                            Func<Vector3, double> analyze,
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
            this.targetNumCells = targetNumCells;
            this.startingParam = startingParam;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.ColorScale = colorScale;
            this._analyze = analyze;
            this.Material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, null, true, true, Guid.NewGuid());
        }

        public void InitCells()
        {
          var bounds = new BBox3(new[] { this.Perimeter });
            var w = bounds.Max.X - bounds.Min.X;
            var h = bounds.Max.Y - bounds.Min.Y;
            var x = bounds.Min.X;
            var y = bounds.Min.Y;

            while (x <= bounds.Min.X + w)
            {
                while (y <= bounds.Min.Y + h)
                {
                    var location = new Vector3(x,y);
                    var _cell = new sCell(location, cellSize);
                    cells.Add(_cell.index, _cell);
                    var result = this._analyze(_cell.rect.Center());
                    this._min = Math.Min(this._min, result);
                    this._max = Math.Max(this._max, result);
                    this._results.Add((_cell.rect, result));
                    y += this.cellSize;
                }
                y = bounds.Min.Y;
                x += this.cellSize;
            }
        }
    
    /////////Main 'Run' function
        public bool Run(sCell startCell)
        {
            //List<sCell> outputCells = new List<sCell>();
            bool done = false;
            // bool leafLeft = true;
            int count = 0;

            // growthCells = new SortedDictionary<Vector2d, sCell>();
            grownTree = new List<sCell>();
            //tempList
            var localTreeList = new List<sCell>();

            //startCell.isActive = true;
            //sCell cell;
            // if(cells.TryGetValue(startCell.index, out cell))
            ///cell.isActive = true;

            localTreeList.Add(startCell);
            //growthCells.Add(startCell.index, startCell);

            sCell currentCell;

            while (localTreeList.Count > 0)
            {
                if (count >= targetNumCells)
                {
                    //worked = true;
                    //debugString = String.Format("All placed!");
                    goto ProcessDict;
                }

                sCell validCell;
                if (ReturnOne(localTreeList, cells, grownTree, out validCell))
                {
                    currentCell = validCell;
                    //
                    /// sCell anothaCell;
                    //if(cells.TryGetValue(currentCell.index, out anothaCell))
                    // anothaCell.isActive = true;
                    //currentCell.isActive = true;
                    //debugString = "work";


                }
                else
                {
                    //debugString = String.Format("Done. {0} remaining", targetNumCells - count);
                    goto ProcessDict;
                }


                if (cells.TryGetValue(currentCell.index, out validCell))
                    validCell.isActive = true;

                string dis = "";
                var validNeighborCells = CheckPlacementLocations(currentCell, cells, grownTree, out dis);
                //        debugString = validNeighborCells.Count.ToString();
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

                    // currentCell.isActive = true;
                    grownTree.Add(currentCell);

                    // if(cells.TryGetValue(currentCell.index, out actualCell))
                    //  actualCell.isActive = true;
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
                ProcessGroups(cells, 2);
            }

            return done;
        }
     
     //should be run using the parent dictionary(cells)
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
                        // if(cell.isActive == true)
                        // {
                        //voxel.isActive = true;
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
                                //l_neighbors[2] = new Vector2d(x+1, y + 1);
                                // l_neighbors[3] = new Vector2d(x+1, y - 1);


                                debugger = clearanceCounter.ToString();
                            }
                            else
                            {
                                l_neighbors[0] = new Vector2dInt(x - 1, y);
                                l_neighbors[1] = new Vector2dInt(x + 1, y);
                                //l_neighbors[2] = new Vector2d(x-1, y+1);
                                // l_neighbors[3] = new Vector2d(x+1, y+1);

                                debugger = clearanceCounter.ToString();
                            }

                            for (int ln = 0; ln < l_neighbors.Length; ln++)
                                if (dict.TryGetValue(l_neighbors[ln], out localNeigh) && localNeigh.isActive == true)
                                    clearanceCounter++;

                            if (clearanceCounter < 1)
                            {
                                //voxel.isActive = true;
                                neighbors.Add(voxel);
                            }
                        }
                        //}
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

         public void ProcessGroups(SortedDictionary<Vector2dInt, sCell> dict, int goal)
        {
            treeRects = new List<List<sCell>>();
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

                var internalList = new List<sCell>();
                if (neighbors.Count > 0)
                {
                    internalList.Add(current);
                    tempList.Remove(current);

                    newDict.Remove(current.index);
                    var neighRects = neighbors.Select(s => s.rect).ToList();
                    tempList.AddRange(internalList);
                    //treeRects.AddRange(neighRects, new GH_Path(count));
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        newDict.Remove(neighbors[i].index);
                        tempList.Remove(neighbors[i]);
                    }
                    treeRects.Add(internalList);
                }
                else
                {
                    internalList.Add(current);
                    treeRects.Add(internalList);
                    tempList.Remove(current);
                    newDict.Remove(current.index);
                }

                count++;
            }

            finalTree = new List<List<sCell>>();

           
            for(int i =0; i <treeRects.Count; i++)
            {
                 var tempTree2 = new List<sCell>();
                  if(treeRects[i].Count==1)
                     {
                         var index = FindNearestTreeIndex(treeRects[i][0].rect, treeRects);
                        tempTree2.Add(treeRects[i][0]);
                        finalTree.Add(tempTree2);
                         
                     }
                     else{
                         tempTree2.AddRange(treeRects[i]);
                         finalTree.Add(tempTree2);
                     }
            }
        }

          public int FindNearestTreeIndex(BBox3 rect, List<List<sCell>> tree)
        {
            int val = 0;

            List<PointSorter> pts = new List<PointSorter>();
            int count = 0;

            for (int i = 0; i < tree.Count; i++)
            {
                for (int j = 0; i < tree[j].Count; j++)
                {
                    var dist = rect.Center().DistanceTo(tree[i][j].rect.Center());
                    if (dist < 0.01)
                        continue;

                    pts.Add(new PointSorter(tree[i][j].rect, dist, count));
                    count++;

                }
               
            }

            var closest = pts.OrderBy(s => s.dist).Select(i => i.index).ToList();

            // if(closest > -1)
            val = closest[0];

            return val;
 
        }

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
                            //if (voxel.isActive ==true)
                            neighbors.Add(voxel);
                    }
                }
            }
            return neighbors;
        }



    }
}