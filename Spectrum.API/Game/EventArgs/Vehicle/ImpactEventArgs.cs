using Spectrum.API.TypeWrappers;

namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class ImpactEventArgs : System.EventArgs
    {
        public float Speed { get; private set; }
        public Position Position { get; private set; }
        public string ImpactedObjectName { get; private set; }

        public ImpactEventArgs(float speed, Position position, string impactedObjectName)
        {
            Speed = speed;
            Position = position;
            ImpactedObjectName = impactedObjectName;
        }
    }
}
