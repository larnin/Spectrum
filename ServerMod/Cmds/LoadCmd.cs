using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class LoadCmd : Cmd
    {
        public override string name { get { return "load"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!load") + ": Show all the available playlists.");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!load [playlist name]") + ": Load a playlist.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                printPlaylists();
                MessageUtilities.popMessageOptions();
                return;
            }

            var name = Resource.PersonalLevelPlaylistsDirPath_  + message + ".xml";
            if(!Resource.FileExist(name))
            {
                MessageUtilities.sendMessage(p, "The playlist " + message + " don't exist !");
                return;
            }
            
            var item = G.Sys.GameManager_.LevelPlaylist_.Count_ == 0 ? null : G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_];

            var gameObject = LevelPlaylist.Load(name);
            var playlistComp = gameObject.GetComponent<LevelPlaylist>();
            G.Sys.GameManager_.LevelPlaylist_.CopyFrom(playlistComp);

            MessageUtilities.sendMessage(p, "Playlist Loaded : " + G.Sys.GameManager_.LevelPlaylist_.Count_ + " levels.");

            if (GeneralUtilities.isOnGamemode() && item != null)
                G.Sys.GameManager_.LevelPlaylist_.Playlist_.Insert(0, item);
        }

        void printPlaylists()
        {
            var liste = GeneralUtilities.playlists();
            liste.RemoveAll((string s) => !Resource.FileExist(s));
            MessageUtilities.sendMessage(liste.Count + " playlists found :");
            foreach (var v in liste)
                MessageUtilities.sendMessage(Resource.GetFileNameWithoutExtension(v));
        }
    }
}
