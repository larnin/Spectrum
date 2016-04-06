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
            GameManager gm = GameObject.Find("GameManager")?.GetComponent<GameManager>();
            PlayerManager pm = GameObject.Find("PlayerManager")?.GetComponent<PlayerManager>();

            if (gm && pm) {
                int p = pm.Current_.inGameData_.LocalPlayerIndex_;
                GameMode mode = gm.Mode_;
                return TimeSpan.FromSeconds(mode.GetDisplayTime(p));
            }

            return TimeSpan.Zero;
        }
    }
}
