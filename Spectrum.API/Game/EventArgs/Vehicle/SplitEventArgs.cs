namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class SplitEventArgs : System.EventArgs
    {
        public float Penetration { get; private set; }
        public float SeparationSpeed { get; private set; }

        public SplitEventArgs(float penetration, float separationSpeed)
        {
            Penetration = penetration;
            SeparationSpeed = separationSpeed;
        }
    }
}
