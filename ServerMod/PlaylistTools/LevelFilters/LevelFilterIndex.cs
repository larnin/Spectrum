using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterIndex : LevelFilter
    {
        public override string[] options { get; } = new string[] {"i", "index"};

        IntComparison comparison;

        public LevelFilterIndex()
        {
            comparison = new IntComparison(0);
        }

        public LevelFilterIndex(IntComparison comparison)
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
                    level.Mode(mode, comparison.Compare(currentIndex));
                    currentIndex++;
                }
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            IntComparison comparison = IntComparison.ParseString(chatString);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterIndex(comparison));
            return new LevelFilterResult("Invalid comparison/number for -index");
        }
    }
}
