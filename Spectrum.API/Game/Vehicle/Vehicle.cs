using System;
using Spectrum.API.Game.EventArgs.Vehicle;
using UnityEngine;

namespace Spectrum.API.Game.Vehicle
{
    public class Vehicle
    {
        private static GameObject VehicleObject { get; set; }
        private static CarLogic VehicleLogic { get; set; }
        private static bool CanOperateOnVehicle => VehicleLogic != null;

        public static Screen Screen { get; private set; }
        public static HUD HUD { get; private set; }

        public static event EventHandler CheckpointPassed;
        public static event EventHandler<TrickCompleteEventArgs> TrickCompleted;

        static Vehicle()
        {
            RenewVehicleObjectReferences();
            Screen = new Screen();

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

        private static void RenewVehicleObjectReferences()
        {
            VehicleObject = GameObject.Find("LocalCar");

            if (VehicleObject != null)
            {
                VehicleLogic = VehicleObject.GetComponent<CarLogic>();
                HUD = new HUD(VehicleObject);
            }
        }
    }
}
