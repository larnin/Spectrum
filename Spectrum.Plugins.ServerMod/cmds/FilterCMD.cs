using Events;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class FilterCMD : cmd
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

        public FilterCMD()
        {

        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!filter") + " save, list, and delete playlists");
            Utilities.sendMessage(Utilities.formatCmd("!filter list [search]") + ": List all filters");
            Utilities.sendMessage(Utilities.formatCmd("!filter save <name> <filter>") + ": Saves a new filter");
            Utilities.sendMessage(Utilities.formatCmd("!filter show <name>") + ": Show what the filter looks like");
            Utilities.sendMessage(Utilities.formatCmd("!filter del <name>") + ": Delete a filter");
            Utilities.sendMessage("Use the [FFFFFF]-filter <name>[-] filter to use saved filters.");
        }

        public bool canUseCurrentPlaylist
        {
            get
            {
                return Utilities.isHost();
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
            string uniquePlayerString = Utilities.getUniquePlayerString(p);
            string filterCmd = playlistCmdMatch.Groups[1].Value.ToLower();
            string filterCmdData = playlistCmdMatch.Groups[2].Value;
            switch (filterCmd)
            {
                default:
                    Utilities.sendMessage($"[A00000]Invalid sub-command `{filterCmd}`[-]");
                    help(p);
                    break;
                case "list":
                    {
                        var searchRegex = Utilities.getSearchRegex(filterCmdData);
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
                        Utilities.sendMessage("[FFFFFF]Found:[-]" + results);
                        break;
                    }
                case "save":
                    {
                        Match nameFilterMatch = Regex.Match(filterCmdData, @"^(.+) ?(.*)$");
                        if (!playlistCmdMatch.Success)
                        {
                            Utilities.sendMessage("[A00000]Bad format for save[-]");
                            help(p);
                            return;
                        }
                        var saveAt = nameFilterMatch.Groups[1].Value;
                        var saveFilter = nameFilterMatch.Groups[2].Value;
                        if (saveAt == "")
                        {
                            Utilities.sendMessage("[A00000]Name required[-]");
                            help(p);
                            return;
                        }
                        else if (saveFilter == "")
                        {
                            Utilities.sendMessage("[A00000]Filter required[-]");
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
                            Utilities.sendMessage("[FFA000]The filter -last will always pull from your last-used filter "
                                + "and does not have a consistent value. If you want to save your last used filter, "
                                + "you will need to re-type it.");
                        Utilities.sendFailures(result.failures, 4);
                        Utilities.sendMessage($"Saved filter to [FFFFFF]{saveAt}[-]:\n[FFFFFF]{saveFilter}[-]");
                        break;
                    }
                case "del":
                    {
                        if (filterCmdData == "")
                        {
                            Utilities.sendMessage("[A00000]You must enter a name[-]");
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
                                Utilities.sendMessage($"Deleted {count} filters.");
                                deleteConfirmation.Remove(uniquePlayerString);
                                Entry.save();
                                break;
                            }
                            else if (filterCmdData.ToLower() == "no")
                            {
                                deleteConfirmation.Remove(uniquePlayerString);
                                Utilities.sendMessage("Cancelled deletion.");
                                break;
                            }
                        }
                        var searchRegex = Utilities.getSearchRegex(filterCmdData);
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
                            Utilities.sendMessage($"[FFFFFF]Use [A05000]!filter del yes[-] to delete {count} filters:[-] {results}");
                        }
                        else
                            Utilities.sendMessage("[A00000]No filters found[-]");
                    }
                    break;
                case "show":
                    {
                        var searchRegex = Utilities.getSearchRegex(filterCmdData);
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
                        Utilities.sendMessage("[FFFFFF]Found:[-]" + results);
                        break;
                    }
            }
        }
    }
    class CmdSettingFilters : CmdSetting
    {
        public override string FileId { get; } = "filters";
        public override string SettingsId { get; } = "filters";

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
