using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterName : LevelFilter
    {
        public override string[] options { get; } = new string[] {"n", "name", "default"};

        string match = "";
        string matchRegex = "";

        public LevelFilterName() { }

        public LevelFilterName(string match)
        {
            this.match = match.ToLower();
            matchRegex = Utilities.getSearchRegex(match);
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
                    level.Mode(mode, Regex.Match(level.level.levelNameAndPath_.levelName_, matchRegex, RegexOptions.IgnoreCase).Success);
                }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelFilterName(chatString));
        }
    }
}
