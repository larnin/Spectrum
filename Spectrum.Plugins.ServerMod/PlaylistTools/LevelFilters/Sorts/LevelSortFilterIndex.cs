using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterIndex : LevelSortFilter
    {
        public override string[] options { get; } = {"si", "sortindex"};

        Dictionary<PlaylistLevel, int> indexIndex = new Dictionary<PlaylistLevel, int>();
        int divider = 1;

        public LevelSortFilterIndex(int divider)
        {
            this.divider = divider;
        }

        public LevelSortFilterIndex() { }

        public override void Apply(List<PlaylistLevel> list)
        {
            int currentIndex = 0;
            foreach (PlaylistLevel level in list)
            {
                if (level.allowed)
                {
                    indexIndex[level] = currentIndex / divider;
                    currentIndex++;
                }
            }
        }

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            return indexIndex[a] - indexIndex[b];
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            if (chatString.Length > 0)
            {
                int divider;
                if (int.TryParse(chatString, out divider))
                    return new LevelFilterResult(new LevelSortFilterIndex(divider));
                else
                    return new LevelFilterResult("Invalid number to -shuffle");
            }
            else
                return new LevelFilterResult(new LevelSortFilterStars());
        }
    }
}
