using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools
{
    class LevelFilterResult
    {
        public LevelFilter filter = null;
        public string message = "";
        public bool success;

        public LevelFilterResult(string message)
        {
            this.message = message;
            success = false;
        }

        public LevelFilterResult(LevelFilter filter)
        {
            this.filter = filter;
            success = true;
        }
    }
}
