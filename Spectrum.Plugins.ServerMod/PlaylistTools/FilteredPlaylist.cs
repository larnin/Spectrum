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

            // Regex explanations:
            // ([\-\|\&\!])(\!?)(\S+)(?:\s+|\s+(.*?)\s+)(?=[\-\|\&\!]\!?\S+\s+)
            // ____________-> match `-`, `|`, `&`, or `!` to group 1
            //             _____-> match `!` or `` to group 2
            //                  _____-> match 1 or more non-whitespace characters to group 3
            //                       ___________________-> Non-capturing group with a regex or (|)
            //                          ___-> match 1 or more whitespace
            //                             _-> OR
            //                              ____________-> match...
            //                              ___-> 1 or more whitespace
            //                                 _____-> match 0 or more of any character, as few as possible, to group 4
            //                                      ___-> match 1 or more whitespace
            //                                          _______________________-> Non-consuming look-ahead that matches similar format to previous.
            //                                                                      Causes regex to match up to next option, without consuming the option.
            string inputMatch = input +" -f "; // this is used so that the look-ahead will match. the ` -f ` never gets matched alone, only as a look-ahead.
            var matches = Regex.Matches(inputMatch, @"([\-\|\&\!])(\!?)(\S+)(?:\s+|\s+(.*?)\s+)(?=[\-\|\&\!]\!?\S+\s+)");

            if (filterTypes.ContainsKey("default"))
            {
                string defaultStr;
                if (matches.Count == 0)
                    defaultStr = input;
                else
                {
                    var defaultMatch = Regex.Match(inputMatch, @"^(.*?)\s*[\-\|\&\!]\!?\S+\s+");
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

        public List<PlaylistLevel> levels;
        public List<LevelFilter> filters;
        
        public FilteredPlaylist() {
            this.levels = new List<PlaylistLevel>();
            this.filters = new List<LevelFilter>();
        }

        public FilteredPlaylist(List<LevelPlaylist.ModeAndLevelInfo> levels, int indexOffset)
        {
            this.levels = new List<PlaylistLevel>();
            AddLevels(levels, indexOffset);
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

        public void AddLevel(LevelPlaylist.ModeAndLevelInfo level, int index)
        {
            levels.Add(new PlaylistLevel(level, index));
        }

        public void AddLevel(LevelPlaylist.ModeAndLevelInfo level)
        {
            AddLevel(level, levels.Count);
        }

        public void AddLevels(List<LevelPlaylist.ModeAndLevelInfo> levels, int indexOffset)
        {
            var n = 0;
            foreach (var level in levels)
            {
                this.levels.Add(new PlaylistLevel(level, indexOffset + n));
                n++;
            }
        }

        public void AddLevels(List<LevelPlaylist.ModeAndLevelInfo> levels)
        {
            AddLevels(levels, this.levels.Count);
        }

        public List<PlaylistLevel> CopyLevels()
        {
            var levels = new List<PlaylistLevel>();
            foreach (var level in this.levels)
            {
                levels.Add(new PlaylistLevel(level.level, level.index));
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

        public CalculateResult Calculate()
        {
            List<LevelSortFilter> sortFilters = new List<LevelSortFilter>();
            Comparison<PlaylistLevel> sortLevels = (a, b) =>
            {
                var value = 0;
                foreach (var sortFilter in sortFilters)
                {
                    if (value == 0 || sortFilter.mode == LevelFilter.Mode.Or || sortFilter.mode == LevelFilter.Mode.OrNot)
                    {
                        value = sortFilter.Sort(a, b);
                        if (value != 0 && sortFilter.mode == LevelFilter.Mode.AndNot || sortFilter.mode == LevelFilter.Mode.OrNot)
                            value = -value;
                    }
                }
                return value;
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
            instanceLevels.RemoveAll(level => !level.allowed);
            return new CalculateResult(instanceLevels);
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
    class CalculateResult
    {
        public List<PlaylistLevel> playlistLevels;
        public List<PlaylistLevel> allowedList;
        public List<LevelPlaylist.ModeAndLevelInfo> levelList;
        public CalculateResult(List<PlaylistLevel> levels)
        {
            playlistLevels = levels;
            levelList = new List<LevelPlaylist.ModeAndLevelInfo>();
            allowedList = new List<PlaylistLevel>();
            foreach (var level in levels)
            {
                if (level.allowed)
                {
                    allowedList.Add(level);
                    levelList.Add(level.level);
                }
            }
        }
    }
}
