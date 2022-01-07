    public class sPolygon
    {
        public Polygon polygon;
        public double height;
        public int index { get; set; }
        public Vector2dInt vIndex { get; set; }


        public sPolygon(Polygon polygon, double height)
        {
            this.polygon = polygon;
            this.height = height;
        }
    }