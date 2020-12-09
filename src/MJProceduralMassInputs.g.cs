// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Elements.Validators;
using Elements.Serialization.JSON;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace MJProceduralMass
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    
    public  class MJProceduralMassInputs : S3Args
    
    {
        [Newtonsoft.Json.JsonConstructor]
        
        public MJProceduralMassInputs(double @cellSize, Polygon @siteBoundary, double @startingLocation, double @minHeight, double @maxHeight, IList<Polygon> @obstaclePolygons, double @targetCellCount, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey):
        base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<MJProceduralMassInputs>();
            if(validator != null)
            {
                validator.PreConstruct(new object[]{ @cellSize, @siteBoundary, @startingLocation, @minHeight, @maxHeight, @obstaclePolygons, @targetCellCount});
            }
        
            this.CellSize = @cellSize;
            this.SiteBoundary = @siteBoundary;
            this.StartingLocation = @startingLocation;
            this.MinHeight = @minHeight;
            this.MaxHeight = @maxHeight;
            this.ObstaclePolygons = @obstaclePolygons;
            this.TargetCellCount = @targetCellCount;
        
            if(validator != null)
            {
                validator.PostConstruct(this);
            }
        }
    
        /// <summary>Range for size of cell</summary>
        [Newtonsoft.Json.JsonProperty("CellSize", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(8D, 100D)]
        public double CellSize { get; set; } = 30D;
    
        /// <summary>A closed planar polygon.</summary>
        [Newtonsoft.Json.JsonProperty("SiteBoundary", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Polygon SiteBoundary { get; set; }
    
        /// <summary>Starting cell parameter (from 0.0-1.0)</summary>
        [Newtonsoft.Json.JsonProperty("StartingLocation", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double StartingLocation { get; set; } = 0.5D;
    
        /// <summary>Min Height for procedural mass.</summary>
        [Newtonsoft.Json.JsonProperty("MinHeight", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(20D, 150D)]
        public double MinHeight { get; set; } = 50D;
    
        /// <summary>Max Height to procedural mass.</summary>
        [Newtonsoft.Json.JsonProperty("MaxHeight", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0D, 100D)]
        public double MaxHeight { get; set; } = 50D;
    
        /// <summary>List of polygons describing no-go zones.</summary>
        [Newtonsoft.Json.JsonProperty("ObstaclePolygons", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Polygon> ObstaclePolygons { get; set; }
    
        /// <summary>Target cell count to cover.</summary>
        [Newtonsoft.Json.JsonProperty("TargetCellCount", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(1D, 100D)]
        public double TargetCellCount { get; set; } = 50D;
    
        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();
    
        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    
    
    }
}