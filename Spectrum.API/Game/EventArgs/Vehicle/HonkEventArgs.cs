using Spectrum.API.TypeWrappers;

namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class HonkEventArgs : System.EventArgs
    {
        public float HornPower { get; private set; }
        public Position Position { get; private set; }

        public HonkEventArgs(float hornPower, Position position)
        {
            HornPower = hornPower;
            Position = position;
        }
    }
}
