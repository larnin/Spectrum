using Spectrum.Plugins.ServerMod.Cmds;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterLast : LevelFilter
    {
        public static Dictionary<string, string> lastFilter = new Dictionary<string, string>();
        public static string activePlayerString = "";

        public static void SetActivePlayer(ClientPlayerInfo p)
        {
            if (p == null)
                activePlayerString = "";
            else
                activePlayerString = GeneralUtilities.getUniquePlayerString(p);
        }

        public static void SaveFilter(ClientPlayerInfo p, string filterText)
        {
            if (p != null)
                lastFilter[GeneralUtilities.getUniquePlayerString(p)] = filterText;
        }

        public override string[] options { get; } = new string[] {"l", "last"};

        public LevelFilterLast() { }

        public override void Apply(List<PlaylistLevel> levels)
        {

        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            string filter;
            if (!lastFilter.TryGetValue(activePlayerString, out filter))
                return new LevelFilterResult("No last filter from you.");
            if (filter == "no recursion")
                return new LevelFilterResult("Your -last contained a -last.");
            lastFilter[activePlayerString] = "no recursion";
            try
            {
                var filters = FilteredPlaylist.ParseFiltersFromString(filter);
                filters.value.Insert(0, new LevelFilterLast());  // some commands search through filters to check stuff
                lastFilter[activePlayerString] = filter;
                return new LevelFilterResult(filters.value);
            }
            catch (Exception e)
            {
                lastFilter.Remove(activePlayerString);
                Console.WriteLine("-last error:\n" + e);
                return new LevelFilterResult("There was an error retrieving -last");
            }
        }
    }
}
