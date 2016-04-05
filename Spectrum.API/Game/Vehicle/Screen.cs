using System;
using Spectrum.API.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spectrum.API.Game.Vehicle
{
    public class Screen
    {
        private static GameObject VehicleScreenObject { get; set; }
        private static CarScreenLogic VehicleScreenLogic { get; set; }
        private static bool CanOperateOnScreen => VehicleScreenLogic != null;

        public int LineLength { get; set; } = 20;

        public Screen()
        {
            FindLocalVehicleScreen();
        }

        public void Clear()
        {
            FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.ClearDecodeText();
            }
        }

        public void SetTimeBarText(string text, string hexColor, float time)
        {
            FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.timeWidget_?.SetTimeText(text, hexColor.ToColor(), time);
            }
        }

        public void StartFinalCountdown(float timeLeft)
        {
            FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.SetFinalCountdown(timeLeft);
            }
        }

        public void WriteText(string text, float perCharacterInterval, int clearDelayUnits, float displayDelay, bool clearOnEnd, string timeBarText)
        {
            FindLocalVehicleScreen();

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

        public void WriteText(string text, float perCharacterInterval, int clearDelayUnits)
        {
            WriteText(text, perCharacterInterval, clearDelayUnits, 0.0f, true, string.Empty);
        }

        public void WriteScreenText(string text, string timeBarText)
        {
            WriteText(text, 0.0753f, 10, 0.0f, true, timeBarText);
        }

        public void WriteScreenText(string text)
        {
            WriteText(text, 0.0753f, 10, 0.0f, true, string.Empty);
        }

        private void FindLocalVehicleScreen()
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
