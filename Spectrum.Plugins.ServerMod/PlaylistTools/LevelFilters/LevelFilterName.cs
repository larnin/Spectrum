using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterName : LevelFilter
    {
        public override string[] options { get; } = new string[] {"n", "name", "default"};

        string match = "";

        public LevelFilterName() { }

        public LevelFilterName(string match)
        {
            this.match = match.ToLower();
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var exactMatch = false;
            foreach (var level in levels)
            {
                if (level.level.levelNameAndPath_.levelName_.ToLower() == match)
                {
                    exactMatch = true;
                    break;
                }
            }
            if (exactMatch)
                foreach (var level in levels)
                {
                    level.Mode(mode, level.level.levelNameAndPath_.levelName_.ToLower() == match);
                }
            else
                foreach (var level in levels)
                {
                    level.Mode(mode, level.level.levelNameAndPath_.levelName_.ToLower().Contains(match));
                }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelFilterName(chatString));
        }
    }
}
