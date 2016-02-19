using System;

namespace Spectrum.API.Game
{
    public class Race
    {
        public static event EventHandler Finished;
        public static event EventHandler Started;
        public static event EventHandler Loaded;

        static Race()
        {
            Events.ServerToClient.ModeFinished.Subscribe(data =>
            {
                Finished?.Invoke(default(object), System.EventArgs.Empty);
            });

            Events.GameMode.Go.Subscribe(data =>
            {
                Started?.Invoke(default(object), System.EventArgs.Empty);
            });

            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                Loaded?.Invoke(default(object), System.EventArgs.Empty);
            });
        }
    }
}
