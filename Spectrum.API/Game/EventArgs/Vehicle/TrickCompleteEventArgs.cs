namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class TrickCompleteEventArgs : System.EventArgs
    {
        public float CooldownAmount { get; }
        public int PointsEarned { get; }

        public float WallRideMeters { get; }
        public float CeilingRideMeters { get; }
        public float GrindMeters { get; }

        public TrickCompleteEventArgs(float cooldownAmount, int pointsEarned, float wallRideMeters, float ceilingRideMeter, float grindMeters)
        {
            CooldownAmount = cooldownAmount;
            PointsEarned = pointsEarned;
            WallRideMeters = wallRideMeters;
            CeilingRideMeters = ceilingRideMeter;
            GrindMeters = grindMeters;
        }
    }
}
