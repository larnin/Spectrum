using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterAll : LevelFilter
    {
        public override string[] options { get; } = new string[] {"all"};

        public LevelFilterAll() { }

        public override void Apply(List<PlaylistLevel> levels)
        {
            foreach (var level in levels)
            {
                level.Mode(mode, true);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelFilterAll());
        }
    }
}
