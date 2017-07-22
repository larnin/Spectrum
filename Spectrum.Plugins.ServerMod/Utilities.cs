using Events;
using Events.ClientToAllClients;
using System;
using System.Collections.Generic;
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

        public static List<ClientPlayerInfo> getClientsBySearch(string search)
        {
            var clients = new List<ClientPlayerInfo>();
            int index;
            if (!int.TryParse(search, out index))
                index = -1;
            search = Regex.Escape(search).Replace("\\*", ".*").Replace("\\$", "$").Replace("\\^", "^");

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

        public static string getUniquePlayerString(ClientPlayerInfo p)
        {
            return $"{p.Username_}:{p.NetworkPlayer_.externalIP}:{p.NetworkPlayer_.externalPort}";
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
                Console.WriteLine(levelInfo.relativePath_);
                WorkshopLevelInfo workshopLevelInfo = null;
                G.Sys.SteamworksManager_.UGC_.TryGetWorkshopLevelData(levelInfo.relativePath_, out workshopLevelInfo);
                resText = resText
                    .Replace("%NAME%", levelInfo.levelName_)
                    .Replace("%DIFFICULTY%", levelInfo.difficulty_.ToString())
                    .Replace("%AUTHOR%", getAuthorName(levelInfo))
                    .Replace("%MODE%", mode.ToString());
                if (levelInfo.SupportsMedals(G.Sys.GameManager_.ModeID_))
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
                        .Replace("%MBRONZE%", "No bronze")
                        .Replace("%MSILVER%", "No silver")
                        .Replace("%MGOLD%", "No gold")
                        .Replace("%MDIAMOND%", "No diamond");
                }
                if (workshopLevelInfo != null)
                {

                    resText = resText
                        .Replace("%STARS%", SteamworksUGC.GetWorkshopRatingText(workshopLevelInfo))
                        .Replace("%STARSINT%", ((int)(workshopLevelInfo.voteScore_ / 0.2f)).ToString())
                        .Replace("%STARSDEC%", (workshopLevelInfo.voteScore_ / 0.2f).ToString("F2"));
                }
                else
                {
                    resText = resText
                        .Replace("%STARS%", "No rating")
                        .Replace("%STARSINT%", "No rating")
                        .Replace("%STARSDEC%", "No rating");
                }
            });
            return resText;
        }
    }
}
