using System;
using UnityEngine;

namespace Spectrum.API.Game.Vehicle
{
    public class HUD
    {
        private static HoverScreenEmitter HoverScreenEmitter { get; set; }
        private static bool CanOperateOnHoverScreen => HoverScreenEmitter != null;

        public HUD(GameObject parentCarObject)
        {
            if (parentCarObject != null)
            {
                HoverScreenEmitter = parentCarObject.GetComponent<HoverScreenEmitter>();
            }
            else
            {
                Console.WriteLine("API.Game.Vehicle.HUD: Tried to assign a null game object.");
            }
        }

        public void SetHoverScreenText(string text, float displayTime)
        {
            if (CanOperateOnHoverScreen)
            {
                HoverScreenEmitter.SetTrickText(new TrickyTextLogic.TrickText(displayTime, 0, TrickyTextLogic.TrickText.TextType.standard, text));
            }
        }

        public void SetHoverScreenText(string text)
        {
            SetHoverScreenText(text, 3.0f);
        }
    }
}
