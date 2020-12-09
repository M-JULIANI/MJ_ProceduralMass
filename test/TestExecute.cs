 
using Xunit;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Serialization.glTF;
using System.IO;
using Newtonsoft.Json;
using Elements;

namespace ACADIAHeatMap
{
public class TestExecute
    {
        [Fact]
        public void RunTest()
        {
            // var envJson = File.ReadAllText("../../../envModel.json");
            // var modPtJson = File.ReadAllText("../../../modelPoints.json");

            // var envModel = JsonConvert.DeserializeObject<Model>(envJson);
            // var modPoints = JsonConvert.DeserializeObject<Model>(modPtJson);
            // //var polyline = new Polygon(new List<Vector3>{new Vector3(-2, -3), new Vector3(5,-4), new Vector3(3,4), new Vector3(-5,6)});
            // var input = new ACADIAHeatMapInputs(3.0, "", "", null, "", "", "");
            // var output = ACADIAHeatMap.Execute(new Dictionary<string, Model>{{"Envelope", envModel}, {"BlobData", modPoints}}, input);
            // output.Model.ToGlTF("../../../myOutput.gltf", false);
            // output.Model.ToGlTF("../../../myOutput.glb", true);

        }
    }
}