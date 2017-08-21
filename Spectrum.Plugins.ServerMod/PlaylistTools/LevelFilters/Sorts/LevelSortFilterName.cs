using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterName : LevelSortFilter
    {
        public override string[] options { get; } = {"sn", "sortname"};

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            return a.level.levelNameAndPath_.levelName_.ToLower().CompareTo(b.level.levelNameAndPath_.levelName_.ToLower());
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterName());
        }
    }
}
