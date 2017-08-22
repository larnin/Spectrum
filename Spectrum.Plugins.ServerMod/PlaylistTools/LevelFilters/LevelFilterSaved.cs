using Spectrum.Plugins.ServerMod.cmds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterSaved : LevelFilter
    {
        public override string[] options { get; } = new string[] {"f", "filter"};

        public static List<string> filterLoadStack = new List<string>();

        public LevelFilterSaved() { }

        public override void Apply(List<PlaylistLevel> levels)
        {

        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            FilterCMD filterCmd = (FilterCMD) cmd.all.getCommand("filter");
            string filterName = null;
            string filter = null;
            string searchRegex = Utilities.getSearchRegex(chatString);
            var count = 0;
            foreach (KeyValuePair<string, string> pair in filterCmd.savedFilters)
            {
                if (Regex.IsMatch(pair.Key, searchRegex, RegexOptions.IgnoreCase))
                {
                    if (count == 0)
                    {
                        filterName = pair.Key;
                        filter = pair.Value;
                    }
                    count++;
                }
            }
            if (count == 0)
                return new LevelFilterResult("Found no matching filters. Try !filter list");
            if (filterLoadStack.Any(filterTxt => filterTxt == filterName))
                return new LevelFilterResult("Your -filter contained itself");
            filterLoadStack.Add(filterName);
            try
            {
                var filters = FilteredPlaylist.ParseFiltersFromString(filter);
                filters.value.Insert(0, new LevelFilterSaved());  // Some commands search through filters to check stuff
                filterLoadStack.RemoveAt(filterLoadStack.Count - 1);
                return new LevelFilterResult(filters.value);
            }
            catch (Exception e)
            {
                filterLoadStack.RemoveAt(filterLoadStack.Count - 1);
                Console.WriteLine("-last error:\n" + e);
                return new LevelFilterResult("There was an error retrieving -last");
            }
        }
    }
}
