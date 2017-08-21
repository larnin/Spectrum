using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterStars : LevelSortFilter
    {
        public override string[] options { get; } = {"ss", "sortstars"};

        float roundTo;

        public LevelSortFilterStars(float roundTo)
        {
            this.roundTo = roundTo;
        }

        public LevelSortFilterStars()
        {
            roundTo = 0.001f;
        }

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
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
                int aVoteScore = (int)Math.Floor((aWorkshopLevelInfo.voteScore_ / 0.2f) / roundTo + 0.5f);
                int bVoteScore = (int)Math.Floor((bWorkshopLevelInfo.voteScore_ / 0.2f) / roundTo + 0.5f);
                return bVoteScore - aVoteScore;
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            if (chatString.Length > 0)
            {
                float roundTo;
                if (float.TryParse(chatString, out roundTo))
                    return new LevelFilterResult(new LevelSortFilterStars(roundTo));
                else
                    return new LevelFilterResult("Invalid number to -sortstars");
            }
            else
                return new LevelFilterResult(new LevelSortFilterStars());
        }
    }
}
