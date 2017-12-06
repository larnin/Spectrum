using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts
{
    abstract class LevelSortFilter : LevelFilter
    {
        public abstract int Sort(PlaylistLevel a, PlaylistLevel b);

        public override void Apply(List<PlaylistLevel> list) { }
    }
}
