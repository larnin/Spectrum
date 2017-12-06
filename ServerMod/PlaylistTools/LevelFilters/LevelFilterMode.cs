using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterMode : LevelFilter
    {
        public override string[] options { get; } = new string[] {"m", "mode"};

        private static Dictionary<string, GameModeID> modes = new Dictionary<string, GameModeID>
        {
            {"none",      GameModeID.None},
            {"sprint",    GameModeID.Sprint},
            {"challenge", GameModeID.Challenge},
            {"tag",       GameModeID.ReverseTag},
            {"soccer",    GameModeID.Soccer},
            {"style",     GameModeID.SpeedAndStyle},
            {"stunt",     GameModeID.Stunt}
        };

        public GameModeID gameMode = GameModeID.None;

        public LevelFilterMode() { }

        public LevelFilterMode(GameModeID gameMode)
        {
            this.gameMode = gameMode;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            foreach (var level in levels)
            {
                level.Mode(mode, level.level.mode_ == gameMode);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            GameModeID gameMode = GameModeID.None;
            if (modes.TryGetValue(chatString.ToLower().Trim(), out gameMode))
                return new LevelFilterResult(new LevelFilterMode(gameMode));
            else
                return new LevelFilterResult($"{chatString.ToLower().Trim()} is not a valid game mode.");
        }
    }
}
