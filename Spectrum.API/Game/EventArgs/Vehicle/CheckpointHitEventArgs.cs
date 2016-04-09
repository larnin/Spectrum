namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class CheckpointHitEventArgs : System.EventArgs
    {
        public int CheckpointIndex { get; private set; }
        public float TrackT { get; private set; }

        public CheckpointHitEventArgs(int checkpointIndex, float trackT)
        {
            CheckpointIndex = checkpointIndex;
            TrackT = trackT;
        }
    }
}
