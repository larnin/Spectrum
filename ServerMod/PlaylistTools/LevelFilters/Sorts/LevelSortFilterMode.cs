using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    class LevelSortFilterMode : LevelSortFilter
    {
        public override string[] options { get; } = {"sm", "sortmode"};

        private static List<GameModeID> modes = new List<GameModeID>
        {
            GameModeID.None,
            GameModeID.Sprint,
            GameModeID.Challenge,
            GameModeID.ReverseTag,
            GameModeID.Soccer,
            GameModeID.SpeedAndStyle,
            GameModeID.Stunt
        };

        public override int Sort(PlaylistLevel a, PlaylistLevel b)
        {
            var aIndex = modes.IndexOf(a.level.mode_);
            var bIndex = modes.IndexOf(a.level.mode_);
            if (aIndex == bIndex)
                return 0;
            else if (aIndex == -1)
                return isPositive ? 1 : -1;
            else if (bIndex == -1)
                return isPositive ? -1 : 1;
            else
                return aIndex - bIndex;
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult(new LevelSortFilterMode());
        }
    }
}
