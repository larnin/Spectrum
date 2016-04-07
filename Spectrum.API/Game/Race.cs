using System;
using UnityEngine;

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

        public static TimeSpan Elapsed()
        {
            var playerIndex = G.Sys.PlayerManager_.Current_.inGameData_.LocalPlayerIndex_;
            var mode = G.Sys.GameManager_.Mode_;

            if(mode != null) 
                return TimeSpan.FromSeconds(mode.GetDisplayTime(playerIndex));

            return TimeSpan.Zero;
        }
    }
}
