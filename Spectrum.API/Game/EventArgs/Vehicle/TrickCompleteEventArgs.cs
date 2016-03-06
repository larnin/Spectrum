namespace Spectrum.API.Game.EventArgs.Vehicle
{
    public class TrickCompleteEventArgs : System.EventArgs
    {
        public bool BoostActive { get; }
        public float BoostPercent { get; }
        public float BoostTime { get; }

        public float CooldownPercent { get; }
        public float PointsEarned { get; }

        public float RechargeAmount { get; }

        public TrickCompleteEventArgs(bool boostActive, float boostPercent, float boostTime, float cooldownPercent, float pointsEarned, float rechargeAmount)
        {
            BoostActive = boostActive;
            BoostPercent = boostPercent;
            BoostTime = boostTime;
            CooldownPercent = cooldownPercent;
            PointsEarned = pointsEarned;
            RechargeAmount = rechargeAmount;
        }
    }
}
