using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterShuffle : LevelSortFilter
    {
        public override string[] options { get; } = {"sh", "shuffle"};

        Dictionary<PlaylistLevel, int> shuffleIndex = new Dictionary<PlaylistLevel, int>();

        public override void Apply(List<PlaylistLevel> list)
        {
            Random rnd = new Random();
            foreach (PlaylistLevel level in list)
            {
                shuffleIndex[level] = rnd.Next();
            }
        }

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            return shuffleIndex[a] - shuffleIndex[b];
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterShuffle());
        }
    }
}
