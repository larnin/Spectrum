using System;
using Spectrum.API.Game.EventArgs.Game;
using UnityEngine;

namespace Spectrum.API.Game
{
    public class Game
    {
        public static GameMode CurrentMode => (GameMode)G.Sys.GameManager_.Mode_.GameModeID_;

        public static string LevelName => G.Sys.GameManager_.LevelName_;
        public static string LevelPath => G.Sys.GameManager_.LevelPath_;
        public static string SceneName => G.Sys.GameManager_.SceneName_;

        public static string WatermarkText
        {
            get
            {
                var gameObject = GameObject.Find("AlphaVersion");
                if (gameObject == null)
                {
                    return string.Empty;
                }

                var labelComponent = gameObject.GetComponent<UILabel>();
                return labelComponent?.text;
            }

            set
            {
                var gameObject = GameObject.Find("AlphaVersion");
                if (gameObject == null)
                {
                    Console.WriteLine("API: Couldn't find AlphaVersion game object.");
                    return;
                }

                var labelComponent = gameObject.GetComponent<UILabel>();
                if (labelComponent != null)
                {
                    labelComponent.text = value;
                }
                else
                {
                    Console.WriteLine("API: AlphaVersion game object found, but no UILabel component exists.");
                }
            }
        }

        public static bool ShowWatermark
        {
            get
            {
                var gameObject = GameObject.Find("AlphaVersion");
                if (gameObject == null)
                    return false;

                return gameObject.activeSelf;
            }

            set
            {
                var gameObject = GameObject.Find("AlphaVersion");
                gameObject?.SetActive(value);
            }
        }

        public static event EventHandler<GameModeFinishedEventArgs> ModeFinished;
        public static event EventHandler ModeStarted;

        static Game()
        {
            Events.GameMode.Finished.Subscribe(data =>
            {
                var eventArgs = new GameModeFinishedEventArgs((Network.NetworkGroup)data.NetworkGroup_);
                ModeFinished?.Invoke(null, eventArgs);
            });

            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                ModeStarted?.Invoke(null, System.EventArgs.Empty);
            });
        }

        public static void RestartLevel()
        {
            if (G.Sys.GameManager_.IsModeCreated_ && !G.Sys.GameManager_.IsLevelEditorMode_)
            {
                G.Sys.GameManager_.RestartLevel();
            }
        }
    }
}
