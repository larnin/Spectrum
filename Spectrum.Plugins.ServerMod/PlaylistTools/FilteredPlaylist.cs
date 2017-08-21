using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools
{
    class FilteredPlaylist
    {
        public static Dictionary<string, LevelFilter> filterTypes = new Dictionary<string, LevelFilter>();

        public const int pageSize = 10;

        public static void AddFilterType(LevelFilter filter)
        {
            foreach (string option in filter.options)
            {
                filterTypes.Add(option, filter);
            }
        }

        public static LevelFilterResult FilterFromOptionAndString(string option, string data)
        {
            LevelFilter filter = null;
            if (!filterTypes.TryGetValue(option, out filter))
            {
                return new LevelFilterResult($"The option {option} does not exist.");
            }
            return filter.FromChatString(data, option);
        }

        public static LevelFilterParseResult<LevelFilter> ParseFiltersFromString(string input)
        {
            List<LevelFilter> filters = new List<LevelFilter>();
            List<string> failures = new List<string>();

            // Regex note: (?=...) is a positive look-ahead
            //  We need it so that each option matches as little as possible *up to the next option*
            string inputMatch = input +" -f ";
            var matches = Regex.Matches(inputMatch, @"([\-\|\&\!])(\!?)(\S+) (.*?) ?(?=[\-\|\&\!]\!?\S+ )");

            if (filterTypes.ContainsKey("default"))
            {
                string defaultStr;
                if (matches.Count == 0)
                    defaultStr = input;
                else
                {
                    var defaultMatch = Regex.Match(inputMatch, @"^(.*?) ?[\-\|\&\!]\!?\S+ ?.*? ");
                    defaultStr = defaultMatch.Groups[1].Value;
                }
                LevelFilterResult filterResult = FilterFromOptionAndString("default", defaultStr);
                if (filterResult.success)
                    filters.AddRange(filterResult.filters);
                else
                    failures.Add(filterResult.message);
            }

            foreach (Match match in matches)
            {
                LevelFilterResult filterResult = FilterFromOptionAndString(match.Groups[3].Value, match.Groups[4].Value);
                if (filterResult.success)
                {
                    foreach (LevelFilter filter in filterResult.filters)
                    {
                        bool not = match.Groups[2].Value.Length > 0;
                        string oper = match.Groups[1].Value;
                        if (oper == "&")
                            filter.mode = LevelFilter.Mode.And;
                        else if (oper == "|")
                            filter.mode = LevelFilter.Mode.Or;
                        else if (oper == "!")
                            not = !not;
                        // else: uses default mode.
                        if (not)
                            switch (filter.mode)
                            {
                                case LevelFilter.Mode.And:
                                    filter.mode = LevelFilter.Mode.AndNot;
                                    break;
                                case LevelFilter.Mode.Or:
                                    filter.mode = LevelFilter.Mode.OrNot;
                                    break;
                                case LevelFilter.Mode.AndNot:
                                    filter.mode = LevelFilter.Mode.And;
                                    break;
                                case LevelFilter.Mode.OrNot:
                                    filter.mode = LevelFilter.Mode.Or;
                                    break;
                                default:
                                    break;
                            }
                        filters.Add(filter);
                    }
                }
                else
                    failures.Add(filterResult.message);
            }
            return new LevelFilterParseResult<LevelFilter>(filters, failures);
        }

        public static LevelFilterParseResult<LevelPlaylist.ModeAndLevelInfo> CalculateParsedFilters(List<LevelPlaylist.ModeAndLevelInfo> initialLevels, string toParse)
        {
            var playlist = new FilteredPlaylist(initialLevels);
            var failures = playlist.AddFiltersFromString(toParse);
            var finalLevels = playlist.Calculate();
            return new LevelFilterParseResult<LevelPlaylist.ModeAndLevelInfo>(finalLevels, failures);
        }

        public List<PlaylistLevel> levels;
        public List<LevelFilter> filters;
        
        public FilteredPlaylist() {
            this.levels = new List<PlaylistLevel>();
            this.filters = new List<LevelFilter>();
        }

        public FilteredPlaylist(List<LevelPlaylist.ModeAndLevelInfo> levels)
        {
            this.levels = new List<PlaylistLevel>();
            AddLevels(levels);
            this.filters = new List<LevelFilter>();
        }

        public FilteredPlaylist(List<LevelFilter> filters)
        {
            this.filters = new List<LevelFilter>(filters);
        }
        public FilteredPlaylist(List<LevelPlaylist.ModeAndLevelInfo> levels, List<LevelFilter> filters)
        {
            this.levels = new List<PlaylistLevel>();
            AddLevels(levels);
            this.filters = new List<LevelFilter>(filters);
        }

        public void AddFilter(LevelFilter filter)
        {
            filters.Add(filter);
        }

        public List<string> AddFiltersFromString(string input)
        {
            var parseResult = ParseFiltersFromString(input);
            foreach (LevelFilter filter in parseResult.value)
            {
                filters.Add(filter);
            }
            return parseResult.failures;
        }

        public void AddLevel(LevelPlaylist.ModeAndLevelInfo level)
        {
            levels.Add(new PlaylistLevel(level));
        }

        public void AddLevels(List<LevelPlaylist.ModeAndLevelInfo> levels)
        {
            foreach (var level in levels)
            {
                this.levels.Add(new PlaylistLevel(level));
            }
        }

        public List<PlaylistLevel> CopyLevels()
        {
            var levels = new List<PlaylistLevel>();
            foreach (var level in this.levels)
            {
                levels.Add(new PlaylistLevel(level.level));
            }
            return levels;
        }

        public List<LevelPlaylist.ModeAndLevelInfo> CopyModeAndLevelInfos()
        {
            var levels = new List<LevelPlaylist.ModeAndLevelInfo>();
            foreach (var level in this.levels)
            {
                levels.Add(level.level);
            }
            return levels;
        }

        public List<LevelPlaylist.ModeAndLevelInfo> Calculate()
        {
            List<LevelSortFilter> sortFilters = new List<LevelSortFilter>();
            Comparison<PlaylistLevel> sortLevels = (a, b) =>
            {
                foreach (var sortFilter in sortFilters)
                {
                    var value = sortFilter.Sort(a, b);
                    if (value != 0)
                        if (sortFilter.mode == LevelFilter.Mode.AndNot || sortFilter.mode == LevelFilter.Mode.OrNot)
                            return -value;
                        else
                            return value;
                }
                return 0;
            };
            List<PlaylistLevel> instanceLevels = CopyLevels();
            foreach (var filter in filters)
            {
                filter.Apply(instanceLevels);
                if (filter is LevelSortFilter)
                {
                    sortFilters.Add((LevelSortFilter)filter);
                    instanceLevels.Sort(sortLevels);
                }
            }
            List<LevelPlaylist.ModeAndLevelInfo> returnLevels = new List<LevelPlaylist.ModeAndLevelInfo>();
            foreach (var level in instanceLevels)
            {
                if (level.allowed)
                    returnLevels.Add(level.level);
            }
            return returnLevels;
        }
    }
    class LevelFilterParseResult<T>
    {
        public List<T> value;
        public List<string> failures;
        public LevelFilterParseResult(List<T> value, List<string> failures)
        {
            this.value = value;
            this.failures = failures;
        }
    }
}
