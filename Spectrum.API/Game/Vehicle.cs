using System;
using System.Reflection;
using Spectrum.API.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spectrum.API.Game
{
    public class Vehicle
    {
        public static int LineLength { get; set; } = 20;

        private static GameObject VehicleObject { get; set; }
        private static CarLogic VehicleLogic { get; set; }
        private static bool CanOperateOnVehicle => VehicleLogic != null;

        private static GameObject VehicleScreenObject { get; set; }
        private static CarScreenLogic VehicleScreenLogic { get; set; }
        private static bool CanOperateOnScreen => VehicleScreenLogic != null;

        public static event EventHandler CheckpointPassed;

        static Vehicle()
        {
            RenewVehicleObjectReferences();

            Events.Car.CheckpointHit.SubscribeAll((sender, data) =>
            {
                CheckpointPassed?.Invoke(null, System.EventArgs.Empty);
            });
        }

        public static void ClearScreen()
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.ClearDecodeText();
            }
        }

        public static void HideScreensaver()
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                var field = VehicleScreenLogic.GetType().GetField("showingScreensaver_", BindingFlags.Instance | BindingFlags.NonPublic);
                field?.SetValue(VehicleScreenLogic, false);

                VehicleScreenLogic.screensaver_.SetActive(false);
            }
        }

        public static void HideTrackArrow()
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                var method = VehicleScreenLogic.GetType().GetMethod("SetCurrentModeVisual", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(VehicleScreenLogic, new object[] { VehicleScreenLogic.compass_ });
            }
        }

        public static void SetTimeBarText(string text, string hexColor, float time)
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.timeWidget_?.SetTimeText(text, hexColor.ToColor(), time);
            }
        }

        public static void ShowScreensaver()
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                var field = VehicleScreenLogic.GetType().GetField("showingScreensaver_", BindingFlags.Instance | BindingFlags.NonPublic);
                field?.SetValue(VehicleScreenLogic, true);
                VehicleScreenLogic.screensaver_.SetActive(true);
            }
        }

        public static void ShowTrackArrow()
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                var method = VehicleScreenLogic.GetType().GetMethod("SetCurrentModeVisual", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(VehicleScreenLogic, new object[] { VehicleScreenLogic.trackArrow_ });
            }
        }

        public static void WriteScreenText(string text, float perCharacterInterval, int clearDelayUnits, float displayDelay, bool clearOnEnd, string timeBarText)
        {
            RenewVehicleObjectReferences();
            if (CanOperateOnScreen)
            {
                var formattedForScreen = text.WordWrap(LineLength);

                for (var i = 0; i < clearDelayUnits; i++)
                {
                    formattedForScreen += " ";
                }
                VehicleScreenLogic.DecodeText(formattedForScreen, perCharacterInterval, displayDelay, clearOnEnd, timeBarText);
            }
        }

        public static void WriteScreenText(string text, float perCharacterInterval, int clearDelayUnits)
        {
            WriteScreenText(text, perCharacterInterval, clearDelayUnits, 0.0f, true, string.Empty);
        }

        public static void WriteScreenText(string text, string timeBarText)
        {
            WriteScreenText(text, 0.0753f, 10, 0.0f, true, timeBarText);
        }

        public static void WriteScreenText(string text)
        {
            WriteScreenText(text, 0.0753f, 10, 0.0f, true, string.Empty);
        }

        public static void StartFinalCountdown(float timeLeft)
        {
            RenewVehicleObjectReferences();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.SetFinalCountdown(timeLeft);
            }
        }

        public static void Test()
        {

        }

        private static void RenewVehicleObjectReferences()
        {
            VehicleObject = GameObject.Find("LocalCar");

            if (VehicleObject != null)
            {
                VehicleLogic = VehicleObject.GetComponent<CarLogic>();
            }
            FindLocalVehicleScreen();
        }

        private static void FindLocalVehicleScreen()
        {
            foreach (var gameObject in Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.name == "CarScreenGroup")
                {
                    var screenComponent = gameObject.GetComponent<CarScreenLogic>();
                    if (screenComponent.CarLogic_.IsLocalCar_)
                    {
                        VehicleScreenObject = gameObject;
                        VehicleScreenLogic = screenComponent;

                        return;
                    }
                    Console.WriteLine("API: Found CarScreenGroup but it is not local.");
                }
            }
        }
    }
}
