using Spectrum.API.Game.Vehicle;

namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class FinishedEventArgs : System.EventArgs
    {
        public RaceEndType Type { get; private set; }
        public int FinalTime { get; private set; }

        public FinishedEventArgs(RaceEndType type, int finalTime)
        {
            Type = type;
            FinalTime = finalTime;
        }
    }
}
