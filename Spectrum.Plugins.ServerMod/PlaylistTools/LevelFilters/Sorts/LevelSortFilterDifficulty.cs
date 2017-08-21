using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterDifficulty : LevelSortFilter
    {
        public override string[] options { get; } = {"sd", "sortdiff", "sortdifficulty"};

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var aDifficulty = levelSetsManager.GetLevelInfo(a.level.levelNameAndPath_.levelPath_).difficulty_;
            var bDifficulty = levelSetsManager.GetLevelInfo(b.level.levelNameAndPath_.levelPath_).difficulty_;
            var aIndex = aDifficulty != LevelDifficulty.None ? LevelFilterDifficulty.difficultyRanks.IndexOf(aDifficulty) : -1;
            var bIndex = bDifficulty != LevelDifficulty.None ? LevelFilterDifficulty.difficultyRanks.IndexOf(bDifficulty) : -1;
            if (aIndex == bIndex)
                return 0;
            else if (aIndex == -1)
                return isPositive ? 1 : -1;
            else if (bIndex == -1)
                return isPositive ? -1 : 1;
            else
                return aIndex - bIndex;
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterDifficulty());
        }
    }
}
