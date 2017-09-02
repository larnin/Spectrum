using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class LoadCMD : cmd
    {
        public override string name { get { return "load"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!load") + ": Show all the available playlists.");
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!load [playlist name]") + ": Load a playlist.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                printPlaylists();
                return;
            }

            var name = Resource.PersonalLevelPlaylistsDirPath_  + message + ".xml";
            if(!Resource.FileExist(name))
            {
                MessageUtilities.sendMessage("The playlist " + message + " don't exist !");
                return;
            }
            
            var item = G.Sys.GameManager_.LevelPlaylist_.Count_ == 0 ? null : G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_];

            var gameObject = LevelPlaylist.Load(name);
            var playlistComp = gameObject.GetComponent<LevelPlaylist>();
            G.Sys.GameManager_.LevelPlaylist_.Copy(playlistComp);

            MessageUtilities.sendMessage("Playlist Loaded : " + G.Sys.GameManager_.LevelPlaylist_.Count_ + " levels.");

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
