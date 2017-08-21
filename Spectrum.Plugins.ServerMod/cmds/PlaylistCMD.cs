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
    class CmdSettingPlaylistLevelFormat : CmdSettingString
    {
        public override string FileId { get; } = "playlistLevelFormat";
        public override string SettingsId { get; } = "playlistLevelFormat";

        public override string DisplayName { get; } = "!playlist Level Format";
        public override string HelpShort { get; } = "!playlist: Formatted text to display for each level";
        public override string HelpLong { get; } = "The text to display for each level. Formatting options: "
            + "%NAME%, %DIFFICULTY%, %MODE%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%, %CREATED%, %UPDATED%";

        public override object Default { get; } = "%NAME%";
    }
    class PlaylistCMD : cmd
    {

        public string levelFormat
        {
            get { return (string)getSetting("playlistLevelFormat").Value; }
            set { getSetting("playlistLevelFormat").Value = value; }
        }

        public override string name { get { return "playlist"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingPlaylistLevelFormat()
        };

        Dictionary<string, LevelPlaylist> selectedPlaylists = new Dictionary<string, LevelPlaylist>();
        Dictionary<string, List<string>> deleteConfirmation = new Dictionary<string, List<string>>();

        public PlaylistCMD()
        {

        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!playlist") + " saves, loads, creates, deletes, and filters playlists");
            Utilities.sendMessage(Utilities.formatCmd("!playlist list [search]") + ": List all playlists");
            Utilities.sendMessage(Utilities.formatCmd("!playlist new [filter]") + ": Creates a new playlist.");
            Utilities.sendMessage(Utilities.formatCmd("!playlist load <name>") + ": Load playlist, [FFFFFF]current[-] is the one being played and isn't saved");
            Utilities.sendMessage(Utilities.formatCmd("!playlist save <name>") + ": Save playlist, [FFFFFF]current[-] is the one being played");
            Utilities.sendMessage(Utilities.formatCmd("!playlist active") + ": Show the name of the loaded playlist.");
            Utilities.sendMessage(Utilities.formatCmd("!playlist del <name>") + ": Delete a playlist");
            Utilities.sendMessage(Utilities.formatCmd("!playlist show [filter]") + ": Show the levels in the loaded playlist");
            Utilities.sendMessage(Utilities.formatCmd("!playlist filter <filter>") + ": Filter the loaded playlist");
            Utilities.sendMessage(Utilities.formatCmd("!playlist add <filter>") + ": Add levels to the end of the loaded playlist");
            Utilities.sendMessage(Utilities.formatCmd("!playlist clear") + ": Clear the loaded playlist");
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
            string playlistCmd = playlistCmdMatch.Groups[1].Value.ToLower();
            string playlistCmdData = playlistCmdMatch.Groups[2].Value;
            switch (playlistCmd)
            {
                default:
                    Utilities.sendMessage($"[A00000]Invalid sub-command `{playlistCmd}`[-]");
                    help(p);
                    break;
                case "list":
                    {
                        var searchRegex = Utilities.getSearchRegex(playlistCmdData);
                        List<string> playlists = Utilities.playlists();
                        playlists.RemoveAll((string s) => !Resource.FileExist(s));
                        if (canUseCurrentPlaylist)
                            playlists.Add("current");
                        var results = "";
                        foreach (string playlist in playlists)
                            if (Regex.IsMatch(playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist), searchRegex, RegexOptions.IgnoreCase))
                                results += "\n" + (playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist));
                        if (results == "")
                            results = "None";
                        Utilities.sendMessage("[FFFFFF]Playlists: [-]" + results);
                        break;
                    }
                case "new":
                    {
                        LevelPlaylist list = LevelPlaylist.Create(true);
                        list.Name_ = "New Playlist";
                        FilteredPlaylist levels = Utilities.getFilteredPlaylist(playlistCmdData);
                        list.Playlist_.AddRange(levels.Calculate());
                        selectedPlaylists[uniquePlayerString] = list;
                        Utilities.sendMessage("[FFFFFF]New playlist with...[-]");
                        Utilities.sendMessage(Utilities.getPlaylistText(levels, levelFormat));
                        break;
                    }
                case "load":
                    {
                        var searchRegex = Utilities.getSearchRegex(playlistCmdData);
                        List<string> playlists = Utilities.playlists();
                        playlists.RemoveAll((string s) => !Resource.FileExist(s));
                        if (canUseCurrentPlaylist)
                            playlists.Add("current");
                        string selectedPlaylist = null;
                        int matchingCount = 0;
                        foreach (string playlist in playlists)
                            if (Regex.IsMatch(playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist), searchRegex, RegexOptions.IgnoreCase))
                            {
                                matchingCount++;
                                if (selectedPlaylist == null)
                                    selectedPlaylist = playlist;
                            }
                        string playlistName = selectedPlaylist == "current" ? selectedPlaylist : Resource.GetFileNameWithoutExtension(selectedPlaylist);
                        if (matchingCount == 0)
                        {
                            Utilities.sendMessage("[A00000]Could not find any playlists with that search[-]");
                            break;
                        }
                        else if (matchingCount == 1)
                            Utilities.sendMessage($"{playlistName} is now active");
                        else
                            Utilities.sendMessage($"{playlistName} is now active, but {matchingCount - 1} others matched too");

                        LevelPlaylist playlistComp;
                        if (selectedPlaylist != "current")
                        {
                            var gameObject = LevelPlaylist.Load(selectedPlaylist);
                            playlistComp = gameObject.GetComponent<LevelPlaylist>();
                        }
                        else
                        {
                            playlistComp = LevelPlaylist.Create(true);
                            playlistComp.Copy(G.Sys.GameManager_.LevelPlaylist_);
                        }
                        selectedPlaylists[uniquePlayerString] = playlistComp;
                        break;
                    }
                case "save":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        if (selectedPlaylist.Playlist_.Count == 0)
                        {
                            Utilities.sendMessage("[A00000]Your active playlist is empty[-]");
                            break;
                        }
                        if (playlistCmdData == "")
                        {
                            Utilities.sendMessage("[A00000]You must enter a name[-]");
                            break;
                        }
                        selectedPlaylist.IsCustom_ = true;
                        if (playlistCmdData == "current")
                        {
                            G.Sys.GameManager_.LevelPlaylist_.Copy(selectedPlaylist);
                            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                            Utilities.updateGameManagerCurrentLevel();
                            StaticTargetedEvent<Events.ServerToClient.SetLevelName.Data>.Broadcast(RPCMode.All, G.Sys.GameManager_.CreateSetLevelNameEventData());
                            Utilities.sendMessage("Set current playlist to active playlist.");
                        }
                        else
                        {
                            selectedPlaylist.Name_ = playlistCmdData;
                            selectedPlaylist.Save();
                            Utilities.sendMessage("Saved playlist.");
                        }
                        break;
                    }
                case "active":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        Utilities.sendMessage(selectedPlaylist.Name_);
                        break;
                    }
                case "del":
                    {
                        if (playlistCmdData == "")
                        {
                            Utilities.sendMessage("[A00000]You must enter a name[-]");
                            break;
                        }
                        List<string> toDelete;
                        var count = 0;
                        if (deleteConfirmation.TryGetValue(uniquePlayerString, out toDelete))
                        {
                            if (playlistCmdData.ToLower() == "yes")
                            {
                                foreach (string playlist in toDelete)
                                    {
                                        FileEx.Delete(playlist);
                                        count++;
                                    }
                                Utilities.sendMessage($"Deleted {count} playlists.");
                                deleteConfirmation.Remove(uniquePlayerString);
                                break;
                            }
                            else if (playlistCmdData.ToLower() == "no")
                            {
                                deleteConfirmation.Remove(uniquePlayerString);
                                Utilities.sendMessage("Cancelled deletion.");
                                break;
                            }
                        }
                        var searchRegex = Utilities.getSearchRegex(playlistCmdData);
                        List<string> playlists = Utilities.playlists();
                        playlists.RemoveAll((string s) => !Resource.FileExist(s));
                        toDelete = new List<string>();
                        var results = "";
                        foreach (string playlist in playlists)
                            if (Regex.IsMatch(playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist), searchRegex, RegexOptions.IgnoreCase))
                            {
                                toDelete.Add(playlist);
                                results += "\n" + (playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist));
                                count++;
                            }
                        if (count > 0)
                        {
                            deleteConfirmation[uniquePlayerString] = toDelete;
                            Utilities.sendMessage($"[FFFFFF]Use [A05000]!playlist del yes[-] to delete {count} levels:[-] {results}");
                        }
                        else
                            Utilities.sendMessage("[A00000]No playlists found[-]");
                    }
                    break;
                case "show":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = Utilities.getFilteredPlaylist(selectedPlaylist.Playlist_, playlistCmdData, false);
                        Utilities.sendMessage(Utilities.getPlaylistText(filterer, levelFormat));
                        break;
                    }
                case "filter":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = Utilities.getFilteredPlaylist(selectedPlaylist.Playlist_, playlistCmdData, false);
                        selectedPlaylist.Playlist_.Clear();
                        selectedPlaylist.Playlist_.AddRange(filterer.Calculate());
                        Utilities.sendMessage("[FFFFFF]Filtered:[-]");
                        Utilities.sendMessage(Utilities.getPlaylistText(filterer, levelFormat));
                        break;
                    }
                case "add":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = Utilities.getFilteredPlaylist(playlistCmdData);
                        selectedPlaylist.Playlist_.AddRange(filterer.Calculate());
                        Utilities.sendMessage("[FFFFFF]Added:[-]");
                        Utilities.sendMessage(Utilities.getPlaylistText(filterer, levelFormat));
                        break;
                    }
                case "clear":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            Utilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = Utilities.getFilteredPlaylist(selectedPlaylist.Playlist_, playlistCmdData, false);
                        selectedPlaylist.Playlist_.Clear();
                        Utilities.sendMessage("[FFFFFF]Cleared[-]");
                        break;
                    }
            }
        }
    }
}
