using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterAuthor : LevelSortFilter
    {
        public override string[] options { get; } = {"sa", "sortauthor"};

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var aAuthor = GeneralUtilities.getAuthorName(levelSetsManager.GetLevelInfo(a.level.levelNameAndPath_.levelPath_));
            var bAuthor = GeneralUtilities.getAuthorName(levelSetsManager.GetLevelInfo(b.level.levelNameAndPath_.levelPath_));
            return aAuthor.ToLower().CompareTo(bAuthor.ToLower());
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterAuthor());
        }
    }
}
