namespace Spectrum.API.Game.Vehicle
{
    public class HUD
    {
        private static HoverScreenEmitter HoverScreenEmitter { get; set; }
        private static bool CanOperateOnHoverScreen => HoverScreenEmitter != null;

        public void SetHUDText(string text, float displayTime)
        {
            UpdateParentObject();

            if (CanOperateOnHoverScreen)
            {
                HoverScreenEmitter.SetTrickText(new TrickyTextLogic.TrickText(displayTime, 0, TrickyTextLogic.TrickText.TextType.standard, text));
            }
        }

        public void SetHUDText(string text)
        {
            SetHUDText(text, 3.0f);
        }

        private void UpdateParentObject()
        {
            var localCar = Utilities.FindLocalCar();
            HoverScreenEmitter = localCar?.GetComponent<HoverScreenEmitter>();
        }
    }
}
