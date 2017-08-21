using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelSortFilterTime : LevelSortFilter
    {
        public override string[] options { get; } = new string[] {"st", "sorttime"};

        MedalStatus medal;

        public LevelSortFilterTime()
        {
            medal = MedalStatus.None;
        }

        public LevelSortFilterTime(MedalStatus medal)
        {
            this.medal = medal;
        }

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var aLevelInfo = levelSetsManager.GetLevelInfo(a.level.levelNameAndPath_.levelPath_);
            var bLevelInfo = levelSetsManager.GetLevelInfo(b.level.levelNameAndPath_.levelPath_);
            float aTime = a.level.mode_.IsTimeBased() && aLevelInfo.SupportsMedals(a.level.mode_)
                ? aLevelInfo.GetMedalRequirementTime(medal)
                : -1;
            float bTime = b.level.mode_.IsTimeBased() && bLevelInfo.SupportsMedals(b.level.mode_)
                ? bLevelInfo.GetMedalRequirementTime(medal)
                : -1;
            if (aTime == -1 && bTime == -1)
                return 0;
            else if (aTime == -1)
                return isPositive ? 1 : -1;
            else if (bTime == -1)
                return isPositive ? -1 : 1;
            else
                return (int) ((aTime - bTime)*1000f);
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            MedalStatus medal;
            if (chatString.Length > 0)
            {
                if (!TryParseMedal(chatString, out medal))
                {
                    return new LevelFilterResult("Invalid medal for -sorttime");
                }
            }
            else
                medal = MedalStatus.Bronze;
            return new LevelFilterResult(new LevelSortFilterTime(medal));
        }

        bool TryParseMedal(string input, out MedalStatus value)
        {
            foreach (KeyValuePair<string, MedalStatus> pair in LevelFilterTime.medalLookup)
                if (pair.Key.Contains(input.ToLower().Trim()))
                {
                    value = pair.Value;
                    return true;
                }
            value = MedalStatus.None;
            return false;
        }
    }
}
