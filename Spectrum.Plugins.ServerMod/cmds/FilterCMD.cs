using Events;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class FilterCmd : Cmd
    {

        public Dictionary<string, string> savedFilters
        {
            get { return (Dictionary<string, string>)getSetting("filters").Value; }
        }

        public override string name { get { return "filter"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingFilters()
        };
        
        Dictionary<string, List<string>> deleteConfirmation = new Dictionary<string, List<string>>();

        public FilterCmd()
        {

        }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter") + " save, list, and delete playlists");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter list [search]") + ": List all filters");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter save <name> <filter>") + ": Saves a new filter");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter show <name>") + ": Show what the filter looks like");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter del <name>") + ": Delete a filter");
            // MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter current <filter>") + ": Filter the currently-playing levels.");
            // Not advertising `!filter current` because it's too difficult to show the pros/cons between current and upcoming here.
            //  Most of the time, players will want to use upcoming. If they're using current, they might end up replaying levels.
            //  If a player wants to replay levels, they probably want to use the `!playlist` commands instead, which resets index to 0.
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!filter upcoming <filter>") + ": Filter the upcoming levels.");
            MessageUtilities.sendMessage(p, "Use the [FFFFFF]-filter <name>[-] filter to use saved filters.");
        }

        public bool canUseCurrentPlaylist
        {
            get
            {
                return GeneralUtilities.isHost();
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Match playlistCmdMatch = Regex.Match(message, @"^(\w+) ?(.*)$");
            if (!playlistCmdMatch.Success)
            {
                help(p);
                return;
            }
            string uniquePlayerString = GeneralUtilities.getUniquePlayerString(p);
            string filterCmd = playlistCmdMatch.Groups[1].Value.ToLower();
            string filterCmdData = playlistCmdMatch.Groups[2].Value;
            switch (filterCmd)
            {
                default:
                    MessageUtilities.sendMessage(p, $"[A00000]Invalid sub-command `{filterCmd}`[-]");
                    help(p);
                    break;
                case "list":
                    {
                        var searchRegex = GeneralUtilities.getSearchRegex(filterCmdData);
                        var results = "";
                        foreach (KeyValuePair<string, string> pair in savedFilters)
                        {
                            if (Regex.IsMatch(pair.Key, searchRegex, RegexOptions.IgnoreCase))
                            {
                                results += "\n" + pair.Key;
                            }
                        }
                        if (results.Length == 0)
                            results = " None";
                        MessageUtilities.sendMessage(p, "[FFFFFF]Found:[-]" + results);
                        break;
                    }
                case "save":
                    {
                        Match nameFilterMatch = Regex.Match(filterCmdData, @"^(.+?)\s+(.*)$");
                        if (!playlistCmdMatch.Success)
                        {
                            MessageUtilities.sendMessage(p, "[A00000]Bad format for save[-]");
                            help(p);
                            return;
                        }
                        var saveAt = nameFilterMatch.Groups[1].Value;
                        var saveFilter = nameFilterMatch.Groups[2].Value;
                        if (saveAt == "")
                        {
                            MessageUtilities.sendMessage(p, "[A00000]Name required[-]");
                            help(p);
                            return;
                        }
                        else if (saveFilter == "")
                        {
                            MessageUtilities.sendMessage(p, "[A00000]Filter required[-]");
                            help(p);
                            return;
                        }
                        savedFilters[saveAt] = saveFilter;
                        Entry.save();
                        PlaylistTools.LevelFilters.LevelFilterLast.SetActivePlayer(p);
                        var result = FilteredPlaylist.ParseFiltersFromString(saveFilter);
                        if (!result.value.Any(filter => filter is PlaylistTools.LevelFilters.LevelFilterLast))
                            PlaylistTools.LevelFilters.LevelFilterLast.SaveFilter(p, saveFilter);
                        else
                            MessageUtilities.sendMessage(p, "[FFA000]The filter -last will always pull from your last-used filter "
                                + "and does not have a consistent value. If you want to save your last used filter, "
                                + "you will need to re-type it.");
                        MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                        GeneralUtilities.sendFailures(result.failures, 4);
                        MessageUtilities.popMessageOptions();
                        MessageUtilities.sendMessage(p, $"Saved filter to [FFFFFF]{saveAt}[-]:\n[FFFFFF]{saveFilter}[-]");
                        break;
                    }
                case "del":
                    {
                        if (filterCmdData == "")
                        {
                            MessageUtilities.sendMessage(p, "[A00000]You must enter a name[-]");
                            break;
                        }
                        List<string> toDelete;
                        var count = 0;
                        if (deleteConfirmation.TryGetValue(uniquePlayerString, out toDelete))
                        {
                            if (filterCmdData.ToLower() == "yes")
                            {
                                foreach (string filterName in toDelete)
                                    {
                                        savedFilters.Remove(filterName);
                                        count++;
                                    }
                                MessageUtilities.sendMessage(p, $"Deleted {count} filters.");
                                deleteConfirmation.Remove(uniquePlayerString);
                                Entry.save();
                                break;
                            }
                            else if (filterCmdData.ToLower() == "no")
                            {
                                deleteConfirmation.Remove(uniquePlayerString);
                                MessageUtilities.sendMessage(p, "Cancelled deletion.");
                                break;
                            }
                        }
                        var searchRegex = GeneralUtilities.getSearchRegex(filterCmdData);
                        toDelete = new List<string>();
                        var results = "";
                        foreach (KeyValuePair<string, string> pair in savedFilters)
                            if (Regex.IsMatch(pair.Key, searchRegex, RegexOptions.IgnoreCase))
                            {
                                toDelete.Add(pair.Key);
                                results += "\n" + pair.Key;
                                count++;
                            }
                        if (count > 0)
                        {
                            deleteConfirmation[uniquePlayerString] = toDelete;
                            MessageUtilities.sendMessage(p, $"[FFFFFF]Use [A05000]!filter del yes[-] to delete {count} filters:[-] {results}");
                        }
                        else
                            MessageUtilities.sendMessage(p, "[A00000]No filters found[-]");
                    }
                    break;
                case "show":
                    {
                        var searchRegex = GeneralUtilities.getSearchRegex(filterCmdData);
                        var results = "";
                        foreach (KeyValuePair<string, string> pair in savedFilters)
                        {
                            if (Regex.IsMatch(pair.Key, searchRegex, RegexOptions.IgnoreCase))
                            {
                                results += $"\n{pair.Key}[FFFFFF]:[-] {pair.Value}";
                            }
                        }
                        if (results.Length == 0)
                            results = " None";
                        MessageUtilities.sendMessage(p, "[FFFFFF]Found:[-]" + results);
                        break;
                    }
                case "current":
                    {
                        PlaylistCmd playlistCmd = Cmd.all.getCommand<PlaylistCmd>("playlist");
                        if (!playlistCmd.canUseCurrentPlaylist)
                        {
                            MessageUtilities.sendMessage(p, "Cannot modify current playlist right now.");
                            break;
                        }
                        Console.WriteLine($"Filter txt: {filterCmdData}");
                        // 1. load current playlist into filter
                        LevelPlaylist currentList = G.Sys.GameManager_.LevelPlaylist_;
                        FilteredPlaylist preFilterer = new FilteredPlaylist(currentList.Playlist_);
                        MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                        GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(preFilterer, p, filterCmdData, false), 4);
                        MessageUtilities.popMessageOptions();
                        // 2. add filter that always allows the current level and helps us find it after calculation.
                        var indexFilter = new ForceCurrentIndexFilter(currentList.Index_);
                        preFilterer.AddFilter(indexFilter);
                        // 3. Calculate filter results.
                        CalculateResult levels = preFilterer.Calculate();
                        // 4. Update current playlist
                        currentList.Playlist_.Clear();
                        currentList.Playlist_.AddRange(levels.levelList);
                        // 4. Get current level index, set playlist index to current level index
                        if (indexFilter.level != null)
                        {
                            int index = levels.levelList.IndexOf(indexFilter.level);
                            if (index >= 0)
                                currentList.SetIndex(index);
                            else
                            {
                                currentList.SetIndex(0);
                                MessageUtilities.sendMessage(p, "[A05000]Warning: could not find current level in new playlist (2). Reset to beginning.[-]");
                            }
                        }
                        else
                        {
                            currentList.SetIndex(0);
                            MessageUtilities.sendMessage(p, "[A05000]Warning: could not find current level in new playlist. Reset to beginning.[-]");
                        }
                        MessageUtilities.sendMessage(p, "Filtered current playlist. Upcoming:");
                        FilteredPlaylist filterer = new FilteredPlaylist(currentList.Playlist_, -currentList.Index_ - 1);
                        filterer.AddFilter(new LevelFilterAll());
                        filterer.AddFilter(new LevelFilterIndex(new IntComparison(currentList.Index_, IntComparison.Comparison.Greater)));
                        MessageUtilities.sendMessage(p, GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Initial, playlistCmd.levelFormat));
                        break;
                    }
                case "upcoming":
                    {
                        PlaylistCmd playlistCmd = Cmd.all.getCommand<PlaylistCmd>("playlist");
                        if (!playlistCmd.canUseCurrentPlaylist)
                        {
                            MessageUtilities.sendMessage(p, "Cannot modify current playlist right now.");
                            break;
                        }
                        // 1. load current playlist into filter
                        LevelPlaylist currentList = G.Sys.GameManager_.LevelPlaylist_;
                        if (currentList.Index_ == currentList.Count_ - 1)
                        {
                            MessageUtilities.sendMessage(p, "Cannot filter upcoming because you are on the last item of the list.");
                            break;
                        }
                        var levelsUpcoming = currentList.Playlist_.GetRange(currentList.Index_ + 1, currentList.Count_ - currentList.Index_ - 1);
                        FilteredPlaylist preFilterer = new FilteredPlaylist(levelsUpcoming);
                        MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                        GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(preFilterer, p, filterCmdData, false), 4);
                        MessageUtilities.popMessageOptions();
                        // 2. Calculate filter results.
                        List<LevelPlaylist.ModeAndLevelInfo> levels = preFilterer.Calculate().levelList;
                        // 3. Update current playlist
                        currentList.Playlist_.RemoveRange(currentList.Index_ + 1, currentList.Count_ - currentList.Index_ - 1);
                        currentList.Playlist_.AddRange(levels);
                        // 4. Print results
                        MessageUtilities.sendMessage(p, "Filtered current playlist. Upcoming:");
                        FilteredPlaylist filterer = new FilteredPlaylist(currentList.Playlist_, -currentList.Index_ - 1);
                        filterer.AddFilter(new LevelFilterAll());
                        filterer.AddFilter(new LevelFilterIndex(new IntComparison(currentList.Index_, IntComparison.Comparison.Greater)));
                        MessageUtilities.sendMessage(p, GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Initial, playlistCmd.levelFormat));
                        break;
                    }
            }
        }
    }
    class ForceCurrentIndexFilter : LevelFilter
    {
        public override string[] options { get; } = new string[] { };

        int index;
        public LevelPlaylist.ModeAndLevelInfo level = null;

        public ForceCurrentIndexFilter(int index)
        {
            this.index = index;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            if (levels.Count > index)
            {
                levels[index].and = PlaylistLevel.Accept.Allow;
                levels[index].or = PlaylistLevel.Accept.Allow;
                level = levels[index].level;
            }
            else
                Console.WriteLine($"levels.Count is {levels.Count} but index is {index}");
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            return new LevelFilterResult("Cannot create by chat");
        }
    }
    class CmdSettingFilters : CmdSetting
    {
        public override string FileId { get; } = "filters";
        public override string SettingsId { get; } = "";

        public override string DisplayName { get; } = "!filter Saved filters";
        public override string HelpShort { get; } = "!filter: List of saved filters";
        public override string HelpLong { get { return HelpShort; } }

        public override UpdateResult UpdateFromString(string input)
        {
            throw new NotImplementedException();
        }

        public override UpdateResult UpdateFromObject(object input)
        {
            if (input.GetType() != typeof(Dictionary<string, object>))
            {
                return new UpdateResult(false, Default, "Invalid dictionary. Resetting to default.");
            }
            try
            {
                var data = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)input)
                {
                    data[entry.Key] = (string)entry.Value;
                }
                return new UpdateResult(true, data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading dictionary: {e}");
                return new UpdateResult(false, Default, "Error reading dictionary. Resetting to default.");
            }
        }

        public override object Default
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
