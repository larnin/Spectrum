namespace Spectrum.API.Game.Vehicle
{
    public class Stats
    {
        private CarStats CarStats { get; set; }
        private bool CanOperateOnCarStats => CarStats != null;

        internal Stats() { }

        internal void UpdateCarReference(CarStats carStats)
        {
            CarStats = carStats;
        }
    }
}
