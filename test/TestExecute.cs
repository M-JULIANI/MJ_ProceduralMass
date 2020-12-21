 
using Xunit;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Serialization.glTF;
using System.IO;
using Newtonsoft.Json;
using Elements;

namespace MJProceduralMass
{
public class TestExecute
    {
        [Fact]
        public void RunTest()
        {

            var boundary = new Polygon(new List<Vector3>{new Vector3(-20, -30), new Vector3(50,-40), new Vector3(30,40), new Vector3(-50,60)});

            var input = new MJProceduralMassInputs(20, 12, 0.5, 45, 20, 80, null, 10, "", "", null, "", "", "");


            var output = MJProceduralMass.Execute(new Dictionary<string, Model>{}, input);

            output.Model.ToGlTF("../../../myOutput.gltf", false);
            // var input = new ACADIAHeatMapInputs(3.0, "", "", null, "", "", "");
            // var output = ACADIAHeatMap.Execute(new Dictionary<string, Model>{{"Envelope", envModel}, {"BlobData", modPoints}}, input);
            // output.Model.ToGlTF("../../../myOutput.gltf", false);
            // output.Model.ToGlTF("../../../myOutput.glb", true);

        }
    }
}