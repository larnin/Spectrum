using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class DelCmd : Cmd
    {
        public override string name { get { return "del"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!del <index>") + ": remove the map at the targeted index from the playlist");
            MessageUtilities.sendMessage(p, "The next map has an index of 0");
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
                MessageUtilities.sendMessage(p, "You can't manage the playlist in trackmogrify");
                return;
            }

            int id = 0;
            int.TryParse(message, out id);

            if (id < 0)
            {
                MessageUtilities.sendMessage(p, "The id must be >= 0");
                return;
            }
            
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            int playListSize = G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - index - 1;
            if(id >= playListSize)
            {
                MessageUtilities.sendMessage(p, "The playlist has only " + playListSize + " maps.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);
            var currentPlaylist = playlist.Playlist_;
            currentPlaylist.RemoveAt(index + id + 1);

            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            MessageUtilities.sendMessage(p, "Map removed !");
        }
    }
}
