using System;
using System.Reflection;
using Spectrum.API.Game.EventArgs.Game;

namespace Spectrum.API.Game
{
    public class Game
    {
        public static GameMode CurrentMode => (GameMode)G.Sys.GameManager_.Mode_.GameModeID_;

        public static bool IsDevelopmentModeActive
        {
            get { return G.Sys.GameManager_.IsDevBuild_; }
            set
            {
                var fieldInfo = G.Sys.GameManager_.GetType().GetField("isDevBuild_", BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo?.SetValue(G.Sys.GameManager_, value);
            }
        }

        public static string LevelName => G.Sys.GameManager_.LevelName_;
        public static string LevelPath => G.Sys.GameManager_.LevelPath_;
        public static string SceneName => G.Sys.GameManager_.SceneName_;

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
