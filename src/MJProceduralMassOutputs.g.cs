// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace MJProceduralMass
{
    public class MJProceduralMassOutputs: ResultsBase
    {
		/// <summary>
		/// Cell count covered
		/// </summary>
		[JsonProperty("Cells")]
		public double Cells {get;}

		/// <summary>
		/// Site cover
		/// </summary>
		[JsonProperty("Site Cover")]
		public string SiteCover {get;}

		/// <summary>
		/// Length of cell.
		/// </summary>
		[JsonProperty("Cell Size")]
		public double CellSize {get;}



        /// <summary>
        /// Construct a MJProceduralMassOutputs with default inputs.
        /// This should be used for testing only.
        /// </summary>
        public MJProceduralMassOutputs() : base()
        {

        }


        /// <summary>
        /// Construct a MJProceduralMassOutputs specifying all inputs.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public MJProceduralMassOutputs(double cells, string siteCover, double cellSize): base()
        {
			this.Cells = cells;
			this.SiteCover = siteCover;
			this.CellSize = cellSize;

		}

		public override string ToString()
		{
			var json = JsonConvert.SerializeObject(this);
			return json;
		}
	}
}