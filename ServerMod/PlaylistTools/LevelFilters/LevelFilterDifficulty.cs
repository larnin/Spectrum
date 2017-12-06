using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterDifficulty : LevelFilter
    {
        public override string[] options { get; } = new string[] {"d", "diff", "difficulty"};

        public static Dictionary<string, int> difficultyLookup = new Dictionary<string, int>
        {
            {"none",      0},
            {"casual",    1},
            {"normal",    2},
            {"advanced",  3},
            {"expert",    4},
            {"nightmare", 5}
        };

        public static List<LevelDifficulty> difficultyRanks = new List<LevelDifficulty>
        {
            LevelDifficulty.None,
            LevelDifficulty.Casual,
            LevelDifficulty.Normal,
            LevelDifficulty.Advanced,
            LevelDifficulty.Expert,
            LevelDifficulty.Nightmare
        };

        IntComparison comparison;

        public LevelFilterDifficulty()
        {
            comparison = new IntComparison(0);
        }

        public LevelFilterDifficulty(IntComparison comparison)
        {
            this.comparison = comparison;
        }

        public LevelFilterDifficulty(LevelDifficulty difficulty)
        {
            this.comparison = new IntComparison(difficultyRanks.IndexOf(difficulty));
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            foreach (var level in levels)
            {
                var levelInfo = levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_);
                level.Mode(mode, comparison.Compare(difficultyRanks.IndexOf(levelInfo.difficulty_)));
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            IntComparison comparison = IntComparison.ParseString(chatString, TryParseDifficultyToInt);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterDifficulty(comparison));
            return new LevelFilterResult("Invalid comparison/number/difficulty for -difficulty");
        }

        bool TryParseDifficultyToInt(string input, out int value)
        {
            foreach (KeyValuePair<string, int> pair in difficultyLookup)
                if (pair.Key.Contains(input.ToLower().Trim()))
                {
                    value = pair.Value;
                    return true;
                }
            return int.TryParse(input, out value);
        }
    }
}
