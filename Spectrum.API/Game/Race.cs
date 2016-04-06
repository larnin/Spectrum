using System;
using UnityEngine;

namespace Spectrum.API.Game
{
    public class Race
    {
        public static event EventHandler Finished;
        public static event EventHandler Started;
        public static event EventHandler Loaded;

        private static TimeSpan _started = TimeSpan.Zero;

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

            Started += (sender, e) => {
                _started = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
            };
        }

        public static TimeSpan Elapsed()
        {
            return TimeSpan.FromSeconds(Time.timeSinceLevelLoad) - _started;
        }
    }
}
