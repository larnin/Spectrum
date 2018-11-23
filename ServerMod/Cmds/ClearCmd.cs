using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class ClearCmd : Cmd
    {
        public override string name { get { return "clear"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!clear") + ": Remove everything on the playlist.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            LevelPlaylist playlist = new LevelPlaylist();
            playlist.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                MessageUtilities.sendMessage(p, "You can't manage the playlist in trackmogrify");
                return;
            }

            if (playlist.Count_ == 0)
            {
                MessageUtilities.sendMessage(p, "The playlist is empty !");
                return;
            }
            
            var item = playlist.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_];
            playlist.Playlist_.Clear();
            
            G.Sys.GameManager_.LevelPlaylist_.Clear();
            G.Sys.GameManager_.LevelPlaylist_.Add(item);

            G.Sys.GameManager_.NextLevelName_ = item.levelNameAndPath_.levelName_;
            G.Sys.GameManager_.NextLevelPath_ = item.levelNameAndPath_.levelPath_;
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
            
            MessageUtilities.sendMessage(p, "Playlist cleared !");
        }
    }
}
