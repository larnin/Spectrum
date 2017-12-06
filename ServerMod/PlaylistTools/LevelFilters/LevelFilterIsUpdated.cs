using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterIsUpdated : LevelFilter
    {
        public override string[] options { get; } = new string[] {"isupdated", "iu"};

        public LevelFilterIsUpdated() { }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var ugc = G.Sys.SteamworksManager_.UGC_;
            foreach (var level in levels)
            {
                WorkshopLevelInfo workshopLevelInfo = null;
                if (!SteamworksManager.IsSteamBuild_ ? false : ugc.TryGetWorkshopLevelData(levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_).relativePath_, out workshopLevelInfo))
                    level.Mode(mode, workshopLevelInfo.isUpdated_);
                else
                    level.Mode(mode, false);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelFilterIsUpdated());
        }
    }
}
