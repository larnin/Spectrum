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
    class CmdSettingPlaylistLevelFormat : CmdSettingString
    {
        public override string FileId { get; } = "playlistLevelFormat";
        public override string SettingsId { get; } = "playlistLevelFormat";

        public override string DisplayName { get; } = "!playlist Level Format";
        public override string HelpShort { get; } = "!playlist: Formatted text to display for each level";
        public override string HelpLong { get; } = "The text to display for each level. Formatting options: "
            + "%NAME%, %DIFFICULTY%, %MODE%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%, %CREATED%, %UPDATED%";

        public override string Default { get; } = "%INDEX%: %NAME%";
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.7.4.0");
    }
    class PlaylistCmd : Cmd
    {

        public string levelFormat
        {
            get { return getSetting<CmdSettingPlaylistLevelFormat>().Value; }
            set { getSetting<CmdSettingPlaylistLevelFormat>().Value = value; }
        }

        public override string name { get { return "playlist"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingPlaylistLevelFormat()
        };

        Dictionary<string, LevelPlaylist> selectedPlaylists = new Dictionary<string, LevelPlaylist>();
        Dictionary<string, List<string>> deleteConfirmation = new Dictionary<string, List<string>>();

        public PlaylistCmd()
        {

        }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist") + " saves, loads, creates, deletes, and filters playlists");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist list [search]") + ": List all playlists");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist new [filter]") + ": Creates a new playlist.");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist load <name>") + ": Load playlist, [FFFFFF]current[-] is the one being played and isn't saved, [FFFFFF]upcoming[-] is the next levels and isn't saved");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist save [name]") + ": Save playlist, [FFFFFF]current[-] is the one being played, [FFFFFF]upcoming[-] is the next levels");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist active") + ": Show the name of the loaded playlist.");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist del <name>") + ": Delete a playlist");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist show [filter]") + ": Show the levels in the loaded playlist");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist filter <filter>") + ": Filter the loaded playlist");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist add <filter>") + ": Add levels to the end of the loaded playlist");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!playlist clear") + ": Clear the loaded playlist");
        }

        public bool canUseCurrentPlaylist
        {
            get
            {
                return GeneralUtilities.isHost();
            }
        }

        public List<string> getPlaylists(string search)
        {
            var searchRegex = GeneralUtilities.getSearchRegex(search);
            List<string> playlists = GeneralUtilities.playlists();
            playlists.RemoveAll((string s) => !Resource.FileExist(s));
            playlists = playlists.ConvertAll(playlist => Resource.GetFileNameWithoutExtension(playlist));
            if (canUseCurrentPlaylist)
            {
                playlists.Add("current");
                playlists.Add("upcoming");
                playlists.Add("active");
            }
            playlists.RemoveAll(playlist => !Regex.IsMatch(playlist, searchRegex, RegexOptions.IgnoreCase));
            return playlists;
        }

        public LevelPlaylist getActivePlaylist(ClientPlayerInfo p)
        {
            if (p == null)
                return null;
            return getActivePlaylist(GeneralUtilities.getUniquePlayerString(p));
        }

        public LevelPlaylist getActivePlaylist(string uniquePlayerString)
        {
            if (uniquePlayerString == null)
                return null;
            LevelPlaylist selectedPlaylist;
            selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist);
            return selectedPlaylist;
        }

        public LevelPlaylist getPlaylistLevels(string search, LevelPlaylist activePlaylist)
        {
            int ignored;
            return getPlaylistLevels(search, out ignored, activePlaylist);
        }

        public LevelPlaylist getPlaylistLevels(string search, out int count, LevelPlaylist activePlaylist)
        {
            var searchRegex = GeneralUtilities.getSearchRegex(search);
            List<string> playlists = GeneralUtilities.playlists();
            playlists.RemoveAll((string s) => !Resource.FileExist(s));
            playlists.RemoveAll(playlist => !Regex.IsMatch(Resource.GetFileNameWithoutExtension(playlist), searchRegex, RegexOptions.IgnoreCase));

            if (canUseCurrentPlaylist)
            {
                List<string> miniList = new List<string>();
                miniList.Add("current");
                miniList.Add("upcoming");
                miniList.Add("active");
                miniList.RemoveAll(playlist => !Regex.IsMatch(playlist, searchRegex, RegexOptions.IgnoreCase));
                playlists.AddRange(miniList);
            }
            count = playlists.Count;
            if (playlists.Count == 0)
                return null;
            var selectedPlaylist = playlists[0];
            LevelPlaylist playlistComp;
            switch (selectedPlaylist)
            {
                case "current":
                    {
                        playlistComp = LevelPlaylist.Create(true);
                        playlistComp.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);
                        playlistComp.Name_ = "current";
                        break;
                    }
                case "upcoming":
                    {
                        playlistComp = LevelPlaylist.Create(true);
                        var currentList = G.Sys.GameManager_.LevelPlaylist_;
                        for (int i = currentList.Index_ + 1; i < currentList.Count_; i++)
                        {
                            playlistComp.Playlist_.Add(currentList.Playlist_[i]);
                        }
                        playlistComp.Name_ = "upcoming";
                        break;
                    }
                case "active":
                    {
                        if (activePlaylist == null)
                            return null;
                        playlistComp = LevelPlaylist.Create(true);
                        playlistComp.CopyFrom(activePlaylist);
                        playlistComp.Name_ = activePlaylist.Name_;
                        break;
                    }
                default:
                    {
                        var gameObject = LevelPlaylist.Load(selectedPlaylist);
                        playlistComp = gameObject.GetComponent<LevelPlaylist>();
                        break;
                    }
            }
            return playlistComp;
        }
        
        public bool savePlaylist(LevelPlaylist selectedPlaylist, string name)
        {
            // playlists are attached to game objects
            // when a new level loads, all existing game objects are destroyed so we cannot properly save them
            // instead, we make a new one and copy the old playlist into the new object.
            LevelPlaylist list = LevelPlaylist.Create(true);
            list.CopyFrom(selectedPlaylist);
            list.IsCustom_ = true;
            selectedPlaylist = list;
            switch (name)
            {
                case "current":
                    {
                        if (canUseCurrentPlaylist)
                        {
                            G.Sys.GameManager_.LevelPlaylist_.CopyFrom(selectedPlaylist);
                            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                            GeneralUtilities.updateGameManagerCurrentLevel();
                            StaticTargetedEvent<Events.ServerToClient.SetLevelName.Data>.Broadcast(RPCMode.All, G.Sys.GameManager_.CreateSetLevelNameEventData());
                            return true;
                        }
                        else
                            return false;
                    }
                case "upcoming":
                    {
                        if (canUseCurrentPlaylist)
                        {
                            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_;
                            if (currentPlaylist.Count_ > currentPlaylist.Index_ + 1)
                                currentPlaylist.Playlist_.RemoveRange(currentPlaylist.Index_ + 1, currentPlaylist.Count_ - currentPlaylist.Index_ - 1);
                            foreach (LevelPlaylist.ModeAndLevelInfo level in selectedPlaylist.Playlist_)
                            {
                                currentPlaylist.Add(level);
                            }
                            currentPlaylist.IsCustom_ = true;
                            GeneralUtilities.updateGameManagerCurrentLevel();
                            StaticTargetedEvent<Events.ServerToClient.SetLevelName.Data>.Broadcast(RPCMode.All, G.Sys.GameManager_.CreateSetLevelNameEventData());
                            return true;
                        }
                        else
                            return false;
                    }
                case "active":
                    {
                        return true;
                    }
                default:
                    {
                        selectedPlaylist.Name_ = name;
                        selectedPlaylist.Save();
                        return true;
                    }
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
            MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
            string uniquePlayerString = GeneralUtilities.getUniquePlayerString(p);
            string playlistCmd = playlistCmdMatch.Groups[1].Value.ToLower();
            string playlistCmdData = playlistCmdMatch.Groups[2].Value;
            switch (playlistCmd)
            {
                default:
                    MessageUtilities.sendMessage($"[A00000]Invalid sub-command `{playlistCmd}`[-]");
                    help(p);
                    break;
                case "list":
                    {
                        List<string> playlists = getPlaylists(playlistCmdData);
                        var results = "";
                        foreach (string playlist in playlists)
                            results += "\n" + (playlist == "current" ? playlist : Resource.GetFileNameWithoutExtension(playlist));
                        if (results == "")
                            results = "None";
                        MessageUtilities.sendMessage("[FFFFFF]Playlists: [-]" + results);
                        break;
                    }
                case "new":
                    {
                        LevelPlaylist list = LevelPlaylist.Create(true);
                        list.Name_ = "New Playlist";
                        FilteredPlaylist levels = new FilteredPlaylist(GeneralUtilities.getAllLevelsAndModes());
                        GeneralUtilities.addFiltersToPlaylist(levels, p, playlistCmdData, true);
                        list.Playlist_.AddRange(levels.Calculate().levelList);
                        selectedPlaylists[uniquePlayerString] = list;
                        MessageUtilities.sendMessage("[FFFFFF]New playlist with...[-]");
                        MessageUtilities.sendMessage(GeneralUtilities.getPlaylistText(levels, GeneralUtilities.IndexMode.Final, levelFormat));
                        break;
                    }
                case "load":
                    {
                        int matchingCount = 0;
                        LevelPlaylist selectedPlaylist = getPlaylistLevels(playlistCmdData, out matchingCount, getActivePlaylist(p));
                        if (matchingCount == 0)
                        {
                            MessageUtilities.sendMessage("[A00000]Could not find any playlists with that search[-]");
                            break;
                        }
                        else if (matchingCount == 1)
                            MessageUtilities.sendMessage($"{selectedPlaylist.Name_} is now active");
                        else
                            MessageUtilities.sendMessage($"{selectedPlaylist.Name_} is now active, but {matchingCount - 1} others matched too");
                        
                        selectedPlaylists[uniquePlayerString] = selectedPlaylist;
                        break;
                    }
                case "save":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        if (selectedPlaylist.Playlist_.Count == 0)
                        {
                            MessageUtilities.sendMessage("[A00000]Your active playlist is empty[-]");
                            break;
                        }
                        if (playlistCmdData == "")
                        {
                            MessageUtilities.sendMessage($"[A05000]No name given. Using existing name: {selectedPlaylist.Name_}");
                            playlistCmdData = selectedPlaylist.Name_;
                        }
                        bool result = savePlaylist(selectedPlaylist, playlistCmdData);
                        switch (playlistCmdData)
                        {
                            case "current":
                                if (result)
                                    MessageUtilities.sendMessage("Set current playlist to active playlist.");
                                else
                                    MessageUtilities.sendMessage("You cannot save to the current playlist right now.");
                                break;
                            case "upcoming":
                                if (result)
                                    MessageUtilities.sendMessage("Set upcoming levels to active playlist.");
                                else
                                    MessageUtilities.sendMessage("You cannot save to the current playlist right now.");
                                break;
                            case "active":
                                MessageUtilities.sendMessage("No changes made.");
                                break;
                            default:
                                selectedPlaylist.Name_ = playlistCmdData;
                                MessageUtilities.sendMessage($"Saved playlist as {playlistCmdData}.");
                                break;
                        }
                        break;
                    }
                case "active":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        MessageUtilities.sendMessage(selectedPlaylist.Name_);
                        break;
                    }
                case "del":
                    {
                        if (playlistCmdData == "")
                        {
                            MessageUtilities.sendMessage("[A00000]You must enter a name[-]");
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
                                MessageUtilities.sendMessage($"Deleted {count} playlists.");
                                deleteConfirmation.Remove(uniquePlayerString);
                                break;
                            }
                            else if (playlistCmdData.ToLower() == "no")
                            {
                                deleteConfirmation.Remove(uniquePlayerString);
                                MessageUtilities.sendMessage("Cancelled deletion.");
                                break;
                            }
                        }
                        var searchRegex = GeneralUtilities.getSearchRegex(playlistCmdData);
                        List<string> playlists = GeneralUtilities.playlists();
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
                            MessageUtilities.sendMessage($"[FFFFFF]Use [A05000]!playlist del yes[-] to delete {count} playlists:[-] {results}");
                        }
                        else
                            MessageUtilities.sendMessage("[A00000]No playlists found[-]");
                    }
                    break;
                case "show":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = new FilteredPlaylist(selectedPlaylist.Playlist_);
                        GeneralUtilities.addFiltersToPlaylist(filterer, p, playlistCmdData, false);
                        MessageUtilities.sendMessage(GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Initial, levelFormat));
                        break;
                    }
                case "filter":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = new FilteredPlaylist(selectedPlaylist.Playlist_);
                        GeneralUtilities.addFiltersToPlaylist(filterer, p, playlistCmdData, false);
                        selectedPlaylist.Playlist_.Clear();
                        selectedPlaylist.Playlist_.AddRange(filterer.Calculate().levelList);
                        MessageUtilities.sendMessage("[FFFFFF]Filtered:[-]");
                        MessageUtilities.sendMessage(GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Final, levelFormat));
                        break;
                    }
                case "add":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        FilteredPlaylist filterer = new FilteredPlaylist(GeneralUtilities.getAllLevelsAndModes());
                        GeneralUtilities.addFiltersToPlaylist(filterer, p, playlistCmdData, true);
                        selectedPlaylist.Playlist_.AddRange(filterer.Calculate().levelList);
                        MessageUtilities.sendMessage("[FFFFFF]Added:[-]");
                        MessageUtilities.sendMessage(GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Final, levelFormat));
                        break;
                    }
                case "clear":
                    {
                        LevelPlaylist selectedPlaylist;
                        if (!selectedPlaylists.TryGetValue(uniquePlayerString, out selectedPlaylist))
                        {
                            MessageUtilities.sendMessage("[A00000]You have no active playlist[-]");
                            break;
                        }
                        selectedPlaylist.Playlist_.Clear();
                        MessageUtilities.sendMessage("[FFFFFF]Cleared[-]");
                        break;
                    }
            }
            MessageUtilities.popMessageOptions();
        }
    }
}
