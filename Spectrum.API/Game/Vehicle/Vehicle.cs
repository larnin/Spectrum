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

        public Screen Screen { get; private set; }

        private static HoverScreenEmitter HoverScreenEmitter { get; set; }
        private static bool CanOperateOnHoverScreen => HoverScreenEmitter != null;

        public static event EventHandler CheckpointPassed;
        public static event EventHandler<TrickCompleteEventArgs> TrickCompleted;

        static Vehicle()
        {
            RenewVehicleObjectReferences();

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

        public static void SetHoverScreenText(string text, float displayTime)
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnHoverScreen)
            {
                HoverScreenEmitter.SetTrickText(new TrickyTextLogic.TrickText(displayTime, 0, TrickyTextLogic.TrickText.TextType.standard, text));
            }
        }

        public static void SetHoverScreenText(string text)
        {
            SetHoverScreenText(text, 3.0f);
        }

        private static void RenewVehicleObjectReferences()
        {
            VehicleObject = GameObject.Find("LocalCar");

            if (VehicleObject != null)
            {
                VehicleLogic = VehicleObject.GetComponent<CarLogic>();
                HoverScreenEmitter = VehicleObject.GetComponent<HoverScreenEmitter>();
            }
        }
    }
}
