using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterUpdated : LevelSortFilter
    {
        public override string[] options { get; } = {"su", "sortupdated"};

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            if (!SteamworksManager.IsSteamBuild_)
                return 0;
            var levelSetsManager = G.Sys.LevelSets_;
            var ugc = G.Sys.SteamworksManager_.UGC_;
            WorkshopLevelInfo aWorkshopLevelInfo;
            WorkshopLevelInfo bWorkshopLevelInfo;
            ugc.TryGetWorkshopLevelData(levelSetsManager.GetLevelInfo(a.level.levelNameAndPath_.levelPath_).relativePath_, out aWorkshopLevelInfo);
            ugc.TryGetWorkshopLevelData(levelSetsManager.GetLevelInfo(b.level.levelNameAndPath_.levelPath_).relativePath_, out bWorkshopLevelInfo);
            if (aWorkshopLevelInfo == null && bWorkshopLevelInfo == null)
                return 0;
            else if (aWorkshopLevelInfo == null)
                return isPositive ? 1 : -1;
            else if (bWorkshopLevelInfo == null)
                return isPositive ? -1 : 1;
            else
            {
                if (aWorkshopLevelInfo.timeUpdated_ < bWorkshopLevelInfo.timeUpdated_)
                    return -1;
                else if (bWorkshopLevelInfo.timeUpdated_ < aWorkshopLevelInfo.timeUpdated_)
                    return 1;
                else
                    return 0;
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterUpdated());
        }
    }
}
