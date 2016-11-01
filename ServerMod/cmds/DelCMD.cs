using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class DelCMD : cmd
    {
        public override string name { get { return "del"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!del <index>: remove the map at the targeted index from the playlist");
            Utilities.sendMessage("The next map have an index of 1");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                Utilities.sendMessage("You can't manage the playlist in trackmogrify");
                return;
            }

            int id = 0;
            int.TryParse(message, out id);

            if (id <= 0)
            {
                Utilities.sendMessage("The id must be a positive number.");
                return;
            }
            
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            int playListSize = G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - index;
            if(id > playListSize)
            {
                Utilities.sendMessage("The playlist have only " + playListSize + " maps.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);
            var currentPlaylist = playlist.Playlist_;
            currentPlaylist.RemoveAt(index + id);

            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            Utilities.sendMessage("Map removed !");
        }
    }
}
