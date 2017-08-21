using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterRegex : LevelFilter
    {
        public override string[] options { get; } = new string[] {"r", "R", "regex", "Regex"};

        string match = "";
        RegexOptions regexOptions = RegexOptions.IgnoreCase;

        public LevelFilterRegex() { }

        public LevelFilterRegex(string match)
        {
            this.match = match;
        }

        public LevelFilterRegex(string match, bool ignoreCase)
        {
            this.match = match;
            this.regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            foreach (var level in levels)
            {
                level.Mode(mode, Regex.Match(level.level.levelNameAndPath_.levelName_, match, regexOptions).Success);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string options)
        {
            return new LevelFilterResult(new LevelFilterRegex(chatString, options.ToLower() == options));
        }
    }
}
