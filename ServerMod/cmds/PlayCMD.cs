using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class PlayCMD : cmd
    {
        public static bool playersCanAddMap = false;

        public override string name { get { return "play"; } }
        public override PermType perm { get { return playersCanAddMap ? PermType.ALL : PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!play [lvl name]: Adds a level to the playlist as the next to be played.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                help(p);
                return;
            }

            if (Utilities.isOnLobby())
            {
                Utilities.sendMessage("You can't set the next level here");
                return;
            }

            var set = G.Sys.LevelSets_.GetSet(G.Sys.GameManager_.ModeID_);
            var list = set.GetAllLevelNameAndPathPairs();

            List<LevelNameAndPathPair> validLevels = new List<LevelNameAndPathPair>();

            foreach(var lvl in list)
            {
                if(lvl.levelName_.ToLower() == message.ToLower())
                {
                    validLevels.Clear();
                    validLevels.Add(lvl);
                    break;
                }
                if (lvl.levelName_.ToLower().Contains(message.ToLower()))
                    validLevels.Add(lvl);
            }

            if(validLevels.Count == 0)
            {
                Utilities.sendMessage("Can't find a level with the name '" + message + "'.");
                return;
            }

            if(validLevels.Count > 1)
            {
                Utilities.sendMessage(validLevels.Count + " levels found :");

                const int maxLvl = 10;
                for (int i = 0 ; i < Math.Min(validLevels.Count, maxLvl) ; i++)
                    Utilities.sendMessage(validLevels[i].levelName_);
                if (validLevels.Count > maxLvl)
                    Utilities.sendMessage("And " + (validLevels.Count - maxLvl) + " more ...");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);

            var currentPlaylist = playlist.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            currentPlaylist.Insert(index+1, new LevelPlaylist.ModeAndLevelInfo(G.Sys.GameManager_.ModeID_, validLevels[0]));
            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            Utilities.sendMessage("Level " + validLevels[0].levelName_ + " added to the playlist !");
        }
    }
}
