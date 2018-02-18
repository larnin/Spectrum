using Events;
using Events.ChatLog;
using Events.ClientToAllClients;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters;
using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    static class GeneralUtilities
    {
        public static string formatCmd(string commandString)
        {
            return "[D0D0D0]" + commandString + "[-]";
        }

        public delegate void TestFuncD();
        public static bool logExceptions(TestFuncD f)
        {
            try
            {
                f();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                if (isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.popMessageOptions();
                }
                return false;
            }
        }
        public static void logExceptionsThrow(TestFuncD f)
        {
            try
            {
                f();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                if (isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }

        public static bool isOnline()
        {
            return G.Sys.NetworkingManager_.IsOnline_;
        }

        public static bool isHost()
        {
            /*foreach(var player in G.Sys.PlayerManager_.PlayerList_)
                if (player.IsLocal_ && player.Index_ == 0)
                    return true;
            return false;*/
            return G.Sys.NetworkingManager_.IsServer_;
        }

        public static void Shuffle<T>(this IList<T> list, System.Random rnd)
        {
            for (var i = 0; i < list.Count; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static bool isOnLobby()
        {
            return GameManager.SceneName_.Equals("MainMenu");
        }

        public static bool isOnGamemode()
        {
            return GameManager.SceneName_.Equals("GameMode");
        }

        public static string getUniquePlayerString(ClientPlayerInfo p)
        {
            return p.NetworkPlayer_.guid;
        }

        public static string getUniquePlayerString(NetworkPlayer p)
        {
            return p.guid;
        }

        public static string getSearchRegex(string search)
        {
            return Regex.Escape(search).Replace("\\*", ".*").Replace("\\$", "$").Replace("\\^", "^");
        }

        public static List<ClientPlayerInfo> getClientsBySearch(string search)
        {
            var clients = new List<ClientPlayerInfo>();
            int index;
            if (!int.TryParse(search, out index))
                index = -1;
            search = getSearchRegex(search);

            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                if (index != -1 ? current.Index_ == index : Regex.Match(current.Username_, search, RegexOptions.IgnoreCase).Success)
                    clients.Add(current);
            }
            return clients;
        }

        public static ClientPlayerInfo clientFromName(string name)
        {
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                if (current.Username_ == name)
                    return current;
            }
            var noFormatClientname = NGUIText.StripSymbols(name);
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                if (NGUIText.StripSymbols(current.Username_) == noFormatClientname)
                    return current;
            }
            return null;
        }

        public static ClientPlayerInfo clientFromID(int id)
        {
            foreach(ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                if (current.Index_ == id)
                    return current;
            }
            return null;
        }

        public static ClientPlayerInfo localClient()
        {
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                if(current.IsLocal_)
                    return current;
            }
            return null;
        }

        public static ClientPlayerInfo getClientFromNetworkPlayer(NetworkPlayer player)
        {
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                if (current.NetworkPlayer_ == player)
                    return current;
            return null;
        }

        public static List<PlayerDataBase> getPlayerInfos()
        {
            var result = new List<PlayerDataBase>();
            foreach (PlayerInfo info in Entry.Instance.playerInfos)
            {
                result.Add(info.playerData);
            }
            return result;
        }

        public static PlayerDataBase getPlayerDataFromClient(ClientPlayerInfo player)
        {
            foreach (PlayerInfo info in Entry.Instance.playerInfos)
            {
                if (info.playerData.PlayerInfo_ == player)
                    return info.playerData;
            }
            return null;
        }

        public static LevelNameAndPathPair getLevel(int index)
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;

            if (index < currentPlaylist.Count)
                return currentPlaylist[index].levelNameAndPath_;
            else
                return null;
        }

        public static void updateGameManagerCurrentLevel()
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            var level = currentPlaylist[index];
            var levelSetsManager = G.Sys.LevelSets_;
            var levelInfo = levelSetsManager.GetLevelInfo(level.levelNameAndPath_.levelPath_);
            G.Sys.GameManager_.NextLevelPath_ = level.levelNameAndPath_.levelPath_;
            G.Sys.GameManager_.NextLevelName_ = level.levelNameAndPath_.levelName_;
            G.Sys.GameManager_.NextGameModeName_ = G.Sys.GameManager_.GetModeName(level.mode_);
        }

        public static LevelNameAndPathPair getCurrentLevel()
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            return currentPlaylist[index].levelNameAndPath_;
        }

        public static LevelNameAndPathPair getNextLevel()
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            if (index < currentPlaylist.Count - 1)
                return currentPlaylist[index + 1].levelNameAndPath_;
            else
                return null;
        }

        public static string getNextLevelName()
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            if (index < currentPlaylist.Count - 1)
                return currentPlaylist[index + 1].levelNameAndPath_.levelName_;
            return "Return to lobby";
        }

        public static bool isCurrentLastLevel()
        {
            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            return index == currentPlaylist.Count - 1;
        }

        public static bool isModeFinished()
        {
            var playerInfos = new List<PlayerDataBase>();
            G.Sys.GameManager_.Mode_.GetSortedListOfUnfinishedPlayers(playerInfos);
            return playerInfos.Count == 0;
        }

        public static List<string> playlists()
        {
            return DirectoryEx.GetFiles(Resource.PersonalLevelPlaylistsDirPath_).ToList();
        }
        public static List<LevelPlaylist.ModeAndLevelInfo> getAllLevelsAndModes()
        {
            var levelSetsManager = G.Sys.LevelSets_;
            var levelsReturn = new List<LevelPlaylist.ModeAndLevelInfo>();
            foreach (LevelSet set in levelSetsManager.levelSets_)
            {
                var mode = set.gameModeID_;
                var levels = set.GetAllLevelNameAndPathExceptMyLevelsPairs();
                foreach (var level in levels)
                {
                    levelsReturn.Add(new LevelPlaylist.ModeAndLevelInfo(mode, level));
                }
            }
            return levelsReturn;
        }

        public static void sendFailures(List<string> failures, int max)
        {
            var count = 0;
            foreach (string failure in failures)
            {
                if (count >= max - 1 && failures.Count > max)
                {
                    MessageUtilities.sendMessage($"[A00000]and {failures.Count - count} more.[-]");
                    break;
                }
                MessageUtilities.sendMessage("[A00000]" + failure + "[-]");
                count++;
            }
        }

        public static List<string> addFiltersToPlaylist(FilteredPlaylist playlist, ClientPlayerInfo p, string filters, bool includeDefault)
        {
            LevelFilterLast.SetActivePlayer(p);
            var failures = playlist.AddFiltersFromString(filters);
            var shouldSave = !playlist.filters.Any(filter => filter is LevelFilterLast);
            if (includeDefault && !playlist.filters.Any(filter => filter is LevelFilterMode))
            {
                playlist.filters.Insert(0, new LevelFilterMode(GameModeID.Sprint));
                if (shouldSave)
                    LevelFilterLast.SaveFilter(p, filters + " -m sprint");
            }
            else if (shouldSave)
                LevelFilterLast.SaveFilter(p, filters);
            return failures;
        }

        public static List<LevelPlaylist.ModeAndLevelInfo> getFilteredLevels(ClientPlayerInfo p, string input)
        {
            var playlist = new FilteredPlaylist(getAllLevelsAndModes());
            var failures = addFiltersToPlaylist(playlist, p, input, true);
            sendFailures(failures, 4);
            return playlist.Calculate().levelList;
        }

        public static string getPlaylistPageText(FilteredPlaylist playlist)
        {
            List<int> removeIndexes = new List<int>();
            List<int> pages = new List<int>();
            for(int i = playlist.filters.Count - 1; i >= 0; i--)
            {
                if (playlist.filters[i] is LevelFilterPage)
                {
                    var filter = (LevelFilterPage)playlist.filters[i];
                    if (filter.comparison.comparison == IntComparison.Comparison.Equal)
                        if (filter.mode == LevelFilter.Mode.And)
                        {
                            pages.Clear();
                            pages.Add(filter.comparison.number);
                            removeIndexes.Add(i);
                            break;
                        }
                        else if (filter.mode == LevelFilter.Mode.Or)
                        {
                            pages.Insert(0, filter.comparison.number);
                            removeIndexes.Add(i);
                        }
                        else
                            break; // using `Not` will make things too unpredictable
                    else
                        return null;
                }
                else if (playlist.filters[i] is LevelSortFilter)
                    break; // anything before a sort filter is not guaranteed to be in the same place now
            }
            if (pages.Count == 0)
                pages.Add(1);
            var newFilters = new List<LevelFilter>(playlist.filters);
            foreach (int index in removeIndexes)
                newFilters.RemoveAt(index);
            var newPlaylist = new FilteredPlaylist(playlist.CopyModeAndLevelInfos(), newFilters);
            var unpagedList = newPlaylist.Calculate();
            var totalPages = Math.Ceiling((double) unpagedList.levelList.Count / (double) FilteredPlaylist.pageSize);
            string currentPageString = "";
            foreach(var page in pages)
            {
                currentPageString += page + ",";
            }
            currentPageString = currentPageString.Substring(0, currentPageString.Length - 1);
            return currentPageString + "/" + totalPages;
        }

        public enum IndexMode { Initial, Final }
        public static string getPlaylistText(FilteredPlaylist playlist, IndexMode indexMode, string levelFormat)
        {
            string pageString = getPlaylistPageText(playlist);
            CalculateResult levels = playlist.Calculate();
            string levelList = "";
            for (int i = 0; i < Math.Min(levels.allowedList.Count, FilteredPlaylist.pageSize); i++)
            {
                var levelIndex = indexMode == IndexMode.Initial ? levels.allowedList[i].index : i;
                levelList += GeneralUtilities.formatLevelInfoText(levels.allowedList[i].level, levelIndex, levelFormat) + "\n";
            }
            if (pageString != null)
                levelList += $"[FFFFFF]Page {pageString}[-]";
            else if (levels.allowedList.Count > FilteredPlaylist.pageSize)
                levelList += $"[FFFFFF]and {levels.allowedList.Count - FilteredPlaylist.pageSize} more[-]";
            else
                levelList = levelList.Substring(0, levelList.Length - 1);
            return levelList;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static bool isLevelOnline(LevelNameAndPathPair TestLevel)
        {
            if (!SteamworksManager.IsSteamBuild_)
                return true;
            var levelSetsManager = G.Sys.LevelSets_;
            foreach (var Level in levelSetsManager.OfficialLevelNameAndPathPairs_)
            {
                if (Level.levelPath_ == TestLevel.levelPath_)
                {
                    return true;
                }
            }
            // Checking the private field appears to be the only way to go about this :(
            var retrievedPublishedFileIds =  (List<ulong>) PrivateUtilities.getPrivateField(G.Sys.SteamworksManager_.UGC_, "retrievedPublishedFileIds_");
            foreach (var Level in levelSetsManager.WorkshopLevelNameAndPathPairs_)
            {
                if (Level.levelPath_ == TestLevel.levelPath_)
                {
                    var relativePath = levelSetsManager.GetLevelInfo(Level.levelPath_).relativePath_;
                    WorkshopLevelInfo levelInfo = null;
                    G.Sys.SteamworksManager_.UGC_.TryGetWorkshopLevelData(relativePath, out levelInfo);
                    if (levelInfo != null) {
                        var hasLevelId = retrievedPublishedFileIds.Contains((ulong)levelInfo.levelID_);
                        return hasLevelId;
                    }
                }
            }
            return false;
        }

        public static string getAuthorName(LevelInfo levelInfo)
        {
            if (SteamworksManager.IsSteamBuild_ && levelInfo.levelType_ == LevelType.Workshop)
            {
                return SteamworksManager.GetSteamName(levelInfo.workshopCreatorID_);
            }
            else if (levelInfo.levelType_ == LevelType.Official)
            {
                return "Refract";
            }
            else if (levelInfo.levelType_ == LevelType.My)
            {
                return "Local";
            }
            else
            {
                return "Unknown";
            }
        }

        public static string formatLevelInfoText(LevelPlaylist.ModeAndLevelInfo level, int index, string levelInfoText)
        {
            return formatLevelInfoText(level.levelNameAndPath_, level.mode_, index, levelInfoText);
        }

        public static string formatLevelInfoText(LevelNameAndPathPair level, int index, string levelInfoText)
        {
            return formatLevelInfoText(level, GameModeID.None, index, levelInfoText);
        }

        public static string formatLevelInfoText(LevelNameAndPathPair level, GameModeID mode, int index, string levelInfoText)
        {
            var resText = levelInfoText;
            var success = GeneralUtilities.logExceptions(() =>
            {
                bool isPointsMode = G.Sys.GameManager_.Mode_ is PointsBasedMode;
                var levelSetsManager = G.Sys.LevelSets_;
                var levelInfo = levelSetsManager.GetLevelInfo(level.levelPath_);
                WorkshopLevelInfo workshopLevelInfo = null;
                if (SteamworksManager.IsSteamBuild_)
                    G.Sys.SteamworksManager_.UGC_.TryGetWorkshopLevelData(levelInfo.relativePath_, out workshopLevelInfo);
                resText = resText
                    .Replace("%NAME%", levelInfo.levelName_)
                    .Replace("%DIFFICULTY%", levelInfo.difficulty_.ToString())
                    .Replace("%AUTHOR%", getAuthorName(levelInfo))
                    .Replace("%MODE%", mode.ToString())
                    .Replace("%INDEX%", index.ToString());
                if (levelInfo.SupportsMedals(mode))
                {
                    resText = resText
                        .Replace("%MBRONZE%", levelInfo.GetMedalRequirementString(MedalStatus.Bronze, isPointsMode))
                        .Replace("%MSILVER%", levelInfo.GetMedalRequirementString(MedalStatus.Silver, isPointsMode))
                        .Replace("%MGOLD%", levelInfo.GetMedalRequirementString(MedalStatus.Gold, isPointsMode))
                        .Replace("%MDIAMOND%", levelInfo.GetMedalRequirementString(MedalStatus.Diamond, isPointsMode));
                }
                else
                {
                    resText = resText
                        .Replace("%MBRONZE%", "None")
                        .Replace("%MSILVER%", "None")
                        .Replace("%MGOLD%", "None")
                        .Replace("%MDIAMOND%", "None");
                }
                if (workshopLevelInfo != null)
                {
                    resText = resText
                        .Replace("%STARS%", SteamworksUGC.GetWorkshopRatingText(workshopLevelInfo))
                        .Replace("%STARSINT%", ((int)(workshopLevelInfo.voteScore_ / 0.2f + 0.5f)).ToString())
                        .Replace("%STARSDEC%", (workshopLevelInfo.voteScore_ / 0.2f).ToString("F2"))
                        .Replace("%STARSPCT%", ((int)(workshopLevelInfo.voteScore_*100f)).ToString())
                        .Replace("%CREATED%", GeneralUtilities.ConvertFromUnixTimestamp(workshopLevelInfo.timeCreated_).ToString("d", CultureInfo.CurrentCulture))
                        .Replace("%UPDATED%", GeneralUtilities.ConvertFromUnixTimestamp(workshopLevelInfo.timeUpdated_).ToString("d", CultureInfo.CurrentCulture));
                }
                else
                {
                    resText = resText
                        .Replace("%STARS%", "None")
                        .Replace("%STARSINT%", "X") 
                        .Replace("%STARSDEC%", "X")
                        .Replace("%STARSPCT%", "X")
                        .Replace("%CREATED%", "")
                        .Replace("%UPDATED%", "");
                }
                var replacements = Cmds.Cmd.all.getCommand<Cmds.LevelCmd>().levelFormatReplacements;
                foreach (var pair in replacements)
                {
                    try
                    {
                        resText = Regex.Replace(resText, pair.Key, pair.Value);
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine($"Invalid regex replace ({pair.Key}) => ({pair.Value}). You can test your regex at regex101.com.");
                        MessageUtilities.sendMessage(GeneralUtilities.localClient(), $"Invalid regex replace ({pair.Key}) => ({pair.Value}).\nYou can test your regex at regex101.com.");
                    }
                }
            });
            if (!success)
            {
                logExceptions(() =>
                {
                    MessageUtilities.sendMessage(GeneralUtilities.localClient(), "Please check your console and report the level format debug info to a ServerMod developer.");
                    Console.WriteLine("Level format debug info:");
                    Console.WriteLine($"    produced string: {resText}");
                    Console.WriteLine($"    level: {level}; {level?.levelName_}; {level?.levelPath_}");
                    Console.WriteLine($"    mode: {mode};");
                    Console.WriteLine($"    index: {index};");
                    Console.WriteLine($"    levelInfoText: {levelInfoText};");
                    Console.WriteLine($"    G.Sys.GameManager_.Mode_: {G.Sys?.GameManager_?.Mode_};");
                    Console.WriteLine($"    G.Sys.LevelSets_: {G.Sys?.LevelSets_};");
                    var levelSetsManager = G.Sys?.LevelSets_;
                    var levelInfo = levelSetsManager?.GetLevelInfo(level.levelPath_);
                    Console.WriteLine($"    levelInfo: {levelInfo};");
                    WorkshopLevelInfo workshopLevelInfo = null;
                    if (SteamworksManager.IsSteamBuild_)
                        G.Sys.SteamworksManager_.UGC_.TryGetWorkshopLevelData(levelInfo.relativePath_, out workshopLevelInfo);
                    Console.WriteLine($"    workshopLevelInfo: {workshopLevelInfo};");
                });
            }
            return resText;
        }
    }
}
