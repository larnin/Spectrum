using Spectrum.API.Game.Vehicle;

namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class DestroyedEventArgs : System.EventArgs
    {
        public DestructionCause Cause { get; private set; }

        public DestroyedEventArgs(DestructionCause cause)
        {
            Cause = cause;
        }
    }
}
