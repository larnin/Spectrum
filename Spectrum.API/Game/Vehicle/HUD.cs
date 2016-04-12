using UnityEngine;

namespace Spectrum.API.Game.Vehicle
{
    public class HUD
    {
        private static HoverScreenEmitter HoverScreenEmitter { get; set; }
        private static bool CanOperateOnHoverScreen => HoverScreenEmitter != null;

        internal HUD() { }

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

        public void Clear()
        {
            HoverScreenEmitter hse = GameObject.Find("LocalCar")?.GetComponent<HoverScreenEmitter>();
            HoverScreenParent hsp = Utilities.Utilities.GetPrivate<HoverScreenParent>(hse, "hoverScreenParent_");
            TrickyTextLogic ttl = hsp.trickyTextObj_.GetComponent<TrickyTextLogic>();
            Utilities.Utilities.GetPrivate<PriorityQueue<TrickyTextLogic.TrickText>>(ttl, "textList_").Clear();
        }

        private void UpdateParentObject()
        {
            var localCar = Utilities.Utilities.FindLocalCar();
            HoverScreenEmitter = localCar?.GetComponent<HoverScreenEmitter>();
        }
    }
}
