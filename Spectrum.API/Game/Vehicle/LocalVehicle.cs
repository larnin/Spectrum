using System;
using Spectrum.API.Game.EventArgs.Vehicle;

namespace Spectrum.API.Game.Vehicle
{
    public class LocalVehicle
    {
        private static CarLogic VehicleLogic { get; set; }
        private static bool CanOperateOnVehicle => VehicleLogic != null;

        public static Screen Screen { get; private set; }
        public static HUD HUD { get; private set; }

        public static float HeatLevel
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.Heat_;

                return 0f;
            }
        }

        public static float VelocityKPH
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.CarStats_.GetKilometersPerHour();

                return 0f;
            }
        }

        public static float VelocityMPH
        {
            get
            {
                UpdateObjectReferences();
                if (CanOperateOnVehicle)
                    return VehicleLogic.CarStats_.GetMilesPerHour();

                return 0f;
            }
        }

        public static event EventHandler CheckpointPassed;
        public static event EventHandler<TrickCompleteEventArgs> TrickCompleted;

        static LocalVehicle()
        {
            Screen = new Screen();
            HUD = new HUD();

            Events.Car.CheckpointHit.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    CheckpointPassed?.Invoke(null, System.EventArgs.Empty);
                }
            });

            Events.Car.TrickComplete.SubscribeAll((sender, data) =>
            {
                if (sender.name == "LocalCar")
                {
                    var eventArgs = new TrickCompleteEventArgs(data.boost_, data.boostPercent_, data.boostTime_, data.cooldownPercent_, data.points_, data.rechargeAmount_);
                    TrickCompleted?.Invoke(null, eventArgs);
                }
            });
        }

        private static void UpdateObjectReferences()
        {
            VehicleLogic = Utilities.FindLocalCar().GetComponent<CarLogic>();
        }
    }
}
