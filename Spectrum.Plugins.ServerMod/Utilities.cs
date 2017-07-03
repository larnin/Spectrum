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
    }
}
