using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterStars : LevelFilter
    {
        public override string[] options { get; } = new string[] {"s", "stars"};

        FloatComparison comparison;

        public LevelFilterStars()
        {
            comparison = new FloatComparison(0);
        }

        public LevelFilterStars(FloatComparison comparison)
        {
            this.comparison = comparison;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var ugc = G.Sys.SteamworksManager_.UGC_;
            foreach (var level in levels)
            {
                WorkshopLevelInfo workshopLevelInfo = null;
                if (ugc.TryGetWorkshopLevelData(levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_).relativePath_, out workshopLevelInfo))
                    level.Mode(mode, comparison.Compare(workshopLevelInfo.voteScore_ / 0.2f));
                else
                    level.Mode(mode, false);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            FloatComparison comparison = FloatComparison.ParseString(chatString);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterStars(comparison));
            return new LevelFilterResult("Invalid comparison/number for -index");
        }
    }
}
