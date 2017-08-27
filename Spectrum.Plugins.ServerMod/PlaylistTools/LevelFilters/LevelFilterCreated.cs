using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterCreated : LevelFilter
    {
        public override string[] options { get; } = new string[] {"c", "created"};

        UIntComparison comparison;

        public LevelFilterCreated()
        {
            comparison = new UIntComparison(0);
        }

        public LevelFilterCreated(UIntComparison comparison)
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
                    level.Mode(mode, comparison.Compare(workshopLevelInfo.timeCreated_));
                else
                    level.Mode(mode, false);
            }
        }

        public bool TryParseDateToUnixTimestamp(string input, out uint value)
        {
            DateTime date;
            if (DateTime.TryParse(input.Trim(), out date))
            {
                value = (uint)Utilities.ConvertToUnixTimestamp(date);
                return true;
            }
            else
            {
                if (uint.TryParse(input.Trim(), out value))
                {
                    return true;
                }
            }
            value = 0;
            return false;
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            UIntComparison comparison = UIntComparison.ParseString(chatString, TryParseDateToUnixTimestamp);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterCreated(comparison));
            return new LevelFilterResult("Invalid comparison/number for -created");
        }
    }
}
