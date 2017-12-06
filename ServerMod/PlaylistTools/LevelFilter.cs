using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools
{
    abstract class LevelFilter
    {
        public enum Mode {And, Or, AndNot, OrNot};

        public abstract string[] options { get; }

        public virtual Mode mode { get; set; } = Mode.And;

        public virtual bool isPositive
        {
            get
            {
                switch (mode)
                {
                    case Mode.And:
                        return true;
                    case Mode.Or:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public abstract LevelFilterResult FromChatString(string chatString, string option);

        public abstract void Apply(List<PlaylistLevel> levels);
    }
}
