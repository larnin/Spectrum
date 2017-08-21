using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools
{
    class LevelFilterResult
    {
        public List<LevelFilter> filters = null;
        public string message = "";
        public bool success;

        public LevelFilterResult(string message)
        {
            this.message = message;
            success = false;
        }

        public LevelFilterResult(LevelFilter filter)
        {
            filters = new List<LevelFilter>() { filter };
            success = true;
        }

        public LevelFilterResult(List<LevelFilter> filters)
        {
            this.filters = new List<LevelFilter>(filters);
            success = true;
        }
    }
}
