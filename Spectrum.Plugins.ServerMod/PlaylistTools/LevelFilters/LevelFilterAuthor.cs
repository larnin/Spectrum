using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterAuthor : LevelFilter
    {
        public override string[] options { get; } = new string[] {"a", "author"};

        string match = "";

        public LevelFilterAuthor() { }

        public LevelFilterAuthor(string match)
        {
            this.match = match.ToLower().Trim();
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var levelSetsManager = G.Sys.LevelSets_;

            var exactMatch = false;
            foreach (var level in levels)
            {
                var authorName = Utilities.getAuthorName(levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_));
                if (authorName.ToLower().Trim() == match)
                {
                    exactMatch = true;
                }
            }
            if (exactMatch)
                foreach (var level in levels)
                {
                    var authorName = Utilities.getAuthorName(levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_));
                    level.Mode(mode, authorName.ToLower().Trim() == match);
                }
            else
                foreach (var level in levels)
                {
                    var authorName = Utilities.getAuthorName(levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_));
                    level.Mode(mode, authorName.ToLower().Trim().Contains(match));
                }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelFilterAuthor(chatString));
        }
    }
}
