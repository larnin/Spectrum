using Spectrum.API.Helpers;

namespace Spectrum.API.Game.Vehicle
{
    public class Screen
    {
        private static CarScreenLogic VehicleScreenLogic { get; set; }
        private static bool CanOperateOnScreen => VehicleScreenLogic != null;

        public int LineLength { get; set; } = 20;

        public Screen()
        {
            VehicleScreenLogic = Utilities.FindLocalVehicleScreen();
        }

        public void Clear()
        {
            Vehicle.RenewObjectReferences();
            VehicleScreenLogic = Utilities.FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.ClearDecodeText();
            }
        }

        public void SetTimeBarText(string text, string hexColor, float time)
        {
            Vehicle.RenewObjectReferences();
            VehicleScreenLogic = Utilities.FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.timeWidget_?.SetTimeText(text, hexColor.ToColor(), time);
            }
        }

        public void StartFinalCountdown(float timeLeft)
        {
            Vehicle.RenewObjectReferences();
            VehicleScreenLogic = Utilities.FindLocalVehicleScreen();

            if (CanOperateOnScreen)
            {
                VehicleScreenLogic.SetFinalCountdown(timeLeft);
            }
        }

        public void WriteText(string text, float perCharacterInterval, int clearDelayUnits, float displayDelay, bool clearOnEnd, string timeBarText)
        {
            Vehicle.RenewObjectReferences();
            Utilities.FindLocalVehicleScreen();

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
    }
}
