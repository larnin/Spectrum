using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterPage : LevelFilter
    {
        public override string[] options { get; } = new string[] {"p", "page"};

        public IntComparison comparison;

        public LevelFilterPage()
        {
            comparison = new IntComparison(0);
        }

        public LevelFilterPage(IntComparison comparison)
        {
            this.comparison = comparison;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var currentIndex = 0;
            foreach (var level in levels)
            {
                if (level.allowed)
                {
                    level.Mode(mode, comparison.Compare(currentIndex / FilteredPlaylist.pageSize + 1));
                    currentIndex++;
                }
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            IntComparison comparison = IntComparison.ParseString(chatString);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterPage(comparison));
            return new LevelFilterResult("Invalid comparison/number for -page");
        }
    }
}
