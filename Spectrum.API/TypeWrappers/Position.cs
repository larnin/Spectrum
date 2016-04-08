namespace Spectrum.API.TypeWrappers
{
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(float x, float y) : this(x, y, 0) { }
    }
}
