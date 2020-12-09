using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Analysis;

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
            var output = new MJProceduralMassOutputs();
            return output;
        }
      }

      public class sGrid: GeometricElement
      {
      private List<(BBox3 cell, double value)> _results = new List<(BBox3 cell, double value)>();
        private Func<Vector3, double> _analyze;
        private double _min = double.MaxValue;
        private double _max = double.MinValue;

                /// <summary>
        /// The length of the cells in the u direction.
        /// </summary>
        public double ULength { get; set; }

        /// <summary>
        /// The length of the cells in the v direction.
        /// </summary>
        public double VLength { get; set; }

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
                            double uLength,
                            double vLength,
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
            this.ULength = uLength;
            this.VLength = vLength;
            this.ColorScale = colorScale;
            this._analyze = analyze;
            this.Material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, null, true, true, Guid.NewGuid());
        }
      }
}