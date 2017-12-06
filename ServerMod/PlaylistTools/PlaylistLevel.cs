using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools
{
    class PlaylistLevel
    {
        public enum Accept { Neutral, Allow, Deny}

        public LevelPlaylist.ModeAndLevelInfo level;
        public int index;

        public Accept and = Accept.Neutral;
        public Accept or = Accept.Neutral;

        public bool allowed
        {
            get
            {
                if (and == Accept.Deny || or == Accept.Deny)
                    return false;
                else if (and == Accept.Allow || or == Accept.Allow)
                    return true;
                return false;
            }
        }

        public PlaylistLevel(LevelPlaylist.ModeAndLevelInfo level)
        {
            this.level = level;
            index = 0;
        }

        public PlaylistLevel(LevelPlaylist.ModeAndLevelInfo level, int index)
        {
            this.level = level;
            this.index = index;
        }

        public void And(bool accept)
        {
            if (and != Accept.Deny)
               and = accept ? Accept.Allow : Accept.Deny;
        }
        public void Or(bool accept)
        {
            if (or != Accept.Allow)
                or = accept ? Accept.Allow : Accept.Deny;
        }
        public void Mode(LevelFilter.Mode mode, bool accept)
        {
            switch (mode)
            {
                case LevelFilter.Mode.And:
                    And(accept);
                    break;
                case LevelFilter.Mode.AndNot:
                    And(!accept);
                    break;
                case LevelFilter.Mode.Or:
                    Or(accept);
                    break;
                case LevelFilter.Mode.OrNot:
                    Or(!accept);
                    break;
                default:
                    break;
            }
        }
    }
}
