

# MJ_ProceduralMass

Generates a procedural massing for a residential/office building. See sample workflow: https://hypar.io/workflows/0c29a4f5-4e8e-4f55-8101-14eafb8a2c8e.

|Input Name|Type|Description|
|---|---|---|
|TargetCellCount|number|Target cell count to cover.|
|MinHeight|number|Min Height for procedural mass.|
|StartingLocation|number|Starting cell parameter (from 0.0-1.0)|
|HeightJitter|number|Height randomness|
|CellSize|number|Range for size of cell|
|MaxHeight|number|Max Height to procedural mass.|
|ObstaclePolygons|array|List of polygons describing no-go zones.|
|SiteSetback|number|Setback of the site|


<br>

|Output Name|Type|Description|
|---|---|---|
|Cells|Number|Cell count covered|
|Site Cover|String|Site cover|
|Cell Size|Number|Length of cell.|

