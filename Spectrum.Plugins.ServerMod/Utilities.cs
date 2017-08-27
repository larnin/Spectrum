using Events;
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

namespace Spectrum.Plugins.ServerMod
{
    static class Utilities
    {
        public static void sendMessage(string message)
        {
            //StaticEvent<ChatSubmitMessage.Data>.Broadcast(new ChatSubmitMessage.Data(message));
            //Chat.SendAction(message);
#pragma warning disable CS0618 // Type or member is obsolete
            StaticTransceivedEvent<ChatMessage.Data>.Broadcast(new ChatMessage.Data((message).Colorize("[AAAAAA]")));
#pragma warning restore CS0618 // Type or member is obsolete
            //Console.WriteLine("Log : " + message);
        }

        public static string formatCmd(string commandString)
        {
            return "[D0D0D0]" + commandString + "[-]";
        }

        public delegate void TestFuncD();
        public static void testFunc(TestFuncD f)
        {
            try
            {
                f();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                try
                {
                    if (isHost())
                    {
                        sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                        sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                        sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    }
                }
                catch (Exception e2)
                {
                    Console.WriteLine($"Could not send message: {e2}");
                }
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

        public static void Shuffle<T>(this IList<T> list, Random rnd)
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
            return $"{p.Username_}:{p.NetworkPlayer_.externalIP}:{p.NetworkPlayer_.externalPort}";
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
            try
            {
                var methode = G.Sys.GameManager_.Mode_.GetType().GetMethod("GetSortedListOfModeInfos", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ModePlayerInfoBase> playersInfos = (List<ModePlayerInfoBase>)methode.Invoke(G.Sys.GameManager_.Mode_, new object[] { });
                foreach (var pI in playersInfos)
                {
                    if (pI.finishType_ != FinishType.Normal)
                        return false;
                }
            }
            catch (Exception e)
            {
                Utilities.sendMessage("Error !");
                Console.WriteLine(e);
                return false;
            }

            return true;
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
                    Utilities.sendMessage($"[A00000]and {failures.Count - count} more.[-]");
                    break;
                }
                Utilities.sendMessage("[A00000]" + failure + "[-]");
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
                    LevelFilterLast.SaveFilter(p, "-m sprint " + filters);
            }
            else if (shouldSave)
                LevelFilterLast.SaveFilter(p, filters);
            return failures;
        }

        public static FilteredPlaylist getFilteredPlaylist(ClientPlayerInfo p, List<LevelPlaylist.ModeAndLevelInfo> levels, string input, bool includeDefault, bool doSendFailures)
        {
            var playlist = new FilteredPlaylist(levels);
            var failures = addFiltersToPlaylist(playlist, p, input, includeDefault);
            if (doSendFailures)
                sendFailures(failures, 4);
            return playlist;
        }

        public static FilteredPlaylist getFilteredPlaylist(ClientPlayerInfo p, List<LevelPlaylist.ModeAndLevelInfo> levels, string input, bool includeDefault)
        {
            return getFilteredPlaylist(p, levels, input, includeDefault, true);
        }

        public static FilteredPlaylist getFilteredPlaylist(List<LevelPlaylist.ModeAndLevelInfo> levels, string input, bool includeDefault)
        {
            return getFilteredPlaylist(null, levels, input, includeDefault, true);
        }

        public static FilteredPlaylist getFilteredPlaylist(string input)
        {
            return getFilteredPlaylist(null, getAllLevelsAndModes(), input, true, true);
        }

        public static FilteredPlaylist getFilteredPlaylist(ClientPlayerInfo p, string input)
        {
            return getFilteredPlaylist(p, getAllLevelsAndModes(), input, true, true);
        }

        public static List<LevelPlaylist.ModeAndLevelInfo> getFilteredLevels(ClientPlayerInfo p, string input)
        {
            return getFilteredPlaylist(input).Calculate();
        }

        public static List<LevelPlaylist.ModeAndLevelInfo> getFilteredLevels(string input)
        {
            return getFilteredLevels(null, input);
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
            var totalPages = Math.Ceiling((double) unpagedList.Count / (double) FilteredPlaylist.pageSize);
            string currentPageString = "";
            foreach(var page in pages)
            {
                currentPageString += page + ",";
            }
            currentPageString = currentPageString.Substring(0, currentPageString.Length - 1);
            return currentPageString + "/" + totalPages;
        }

        public static string getPlaylistText(FilteredPlaylist playlist, string levelFormat)
        {
            string pageString = getPlaylistPageText(playlist);
            List<LevelPlaylist.ModeAndLevelInfo> levels = playlist.Calculate();
            string levelList = "";
            for (int i = 0; i < Math.Min(levels.Count, FilteredPlaylist.pageSize); i++)
            {
                levelList += Utilities.formatLevelInfoText(levels[i], levelFormat) + "\n";
            }
            if (pageString != null)
                levelList += $"[FFFFFF]Page {pageString}[-]";
            else if (levels.Count > FilteredPlaylist.pageSize)
                levelList += $"[FFFFFF]and {levels.Count - FilteredPlaylist.pageSize} more[-]";
            else
                levelList = levelList.Substring(0, levelList.Length - 1);
            return levelList;
        }

        static string authorMessageRegex = @"^\[[A-Fa-f0-9]{6}\](.+?)\[[A-Fa-f0-9]{6}\]: (.*)$";
        //take from newer spectrum version (stable can't use messages events)
        public static string ExtractMessageAuthor(string message)
        {
            try
            {
                Match msgMatch = Regex.Match(message, authorMessageRegex);

                return NGUIText.StripSymbols(msgMatch.Groups[1].Value).Trim();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error getting player name: {e}");
                return string.Empty;
            }
        }

        public static bool IsSystemMessage(string message)
        {
            return message.Contains("[c]") && message.Contains("[/c]");
        }

        public static string ExtractMessageBody(string message)
        {
            try
            {
                Match msgMatch = Regex.Match(message, authorMessageRegex);
                if (!msgMatch.Success)
                    return string.Empty;
                return msgMatch.Groups[2].Value;
            }
            catch
            {
                return string.Empty;
            }
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

        public static object getPrivateField(object obj, string fieldName)
        {
            return obj
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                )
                .GetValue(obj);
        }

        public static bool isLevelOnline(LevelNameAndPathPair TestLevel)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            foreach (var Level in levelSetsManager.OfficialLevelNameAndPathPairs_)
            {
                if (Level.levelPath_ == TestLevel.levelPath_)
                {
                    return true;
                }
            }
            // Checking the private field appears to be the only way to go about this :(
            var retrievedPublishedFileIds =  (List<ulong>) getPrivateField(G.Sys.SteamworksManager_.UGC_, "retrievedPublishedFileIds_");
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

        public static string formatLevelInfoText(LevelPlaylist.ModeAndLevelInfo level, string levelInfoText)
        {
            return formatLevelInfoText(level.levelNameAndPath_, level.mode_, levelInfoText);
        }

        public static string formatLevelInfoText(LevelNameAndPathPair level, string levelInfoText)
        {
            return formatLevelInfoText(level, GameModeID.None, levelInfoText);
        }

        public static string formatLevelInfoText(LevelNameAndPathPair level, GameModeID mode, string levelInfoText)
        {
            var resText = levelInfoText;
            Utilities.testFunc(() =>
            {
                bool isPointsMode = G.Sys.GameManager_.Mode_ is PointsBasedMode;
                var levelSetsManager = G.Sys.LevelSets_;
                var levelInfo = levelSetsManager.GetLevelInfo(level.levelPath_);
                WorkshopLevelInfo workshopLevelInfo = null;
                G.Sys.SteamworksManager_.UGC_.TryGetWorkshopLevelData(levelInfo.relativePath_, out workshopLevelInfo);
                resText = resText
                    .Replace("%NAME%", levelInfo.levelName_)
                    .Replace("%DIFFICULTY%", levelInfo.difficulty_.ToString())
                    .Replace("%AUTHOR%", getAuthorName(levelInfo))
                    .Replace("%MODE%", mode.ToString());
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
                    Console.WriteLine(
                        Utilities.ConvertToUnixTimestamp(Utilities.ConvertFromUnixTimestamp(workshopLevelInfo.timeCreated_))
                        +" "
                        +workshopLevelInfo.timeCreated_
                       );
                    resText = resText
                        .Replace("%STARS%", SteamworksUGC.GetWorkshopRatingText(workshopLevelInfo))
                        .Replace("%STARSINT%", ((int)(workshopLevelInfo.voteScore_ / 0.2f)).ToString())
                        .Replace("%STARSDEC%", (workshopLevelInfo.voteScore_ / 0.2f).ToString("F2"))
                        .Replace("%CREATED%", Utilities.ConvertFromUnixTimestamp(workshopLevelInfo.timeCreated_).ToString("d", CultureInfo.CurrentCulture))
                        .Replace("%UPDATED%", Utilities.ConvertFromUnixTimestamp(workshopLevelInfo.timeUpdated_).ToString("d", CultureInfo.CurrentCulture));
                }
                else
                {
                    resText = resText
                        .Replace("%STARS%", "None")
                        .Replace("%STARSINT%", "X")
                        .Replace("%STARSDEC%", "X")
                        .Replace("%CREATED%", "")
                        .Replace("%UPDATED%", "");
                }
            });
            return resText;
        }
    }
}
