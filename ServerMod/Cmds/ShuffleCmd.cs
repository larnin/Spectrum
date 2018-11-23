using Spectrum.Plugins.ServerMod.Utilities;
using System;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class ShuffleCmd : Cmd
    {
        public override string name { get { return "shuffle"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!shuffle") + ": Shuffle the current playlist");
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

            if (GeneralUtilities.isOnGamemode())
                shuffleOnGame(playlist);
            else shuffleOnLobby(playlist);

            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
            G.Sys.GameManager_.NextLevelName_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelName_;
            G.Sys.GameManager_.NextLevelPath_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelPath_;

            MessageUtilities.sendMessage(p, "Playlist shuffled !");
        }

        void shuffleOnLobby(LevelPlaylist playlist)
        {
            var shuffledList = playlist.Playlist_;
            GeneralUtilities.Shuffle(playlist.Playlist_, new Random());
            G.Sys.GameManager_.LevelPlaylist_.Clear();
            foreach (var lvl in shuffledList)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
        }

        void shuffleOnGame(LevelPlaylist playlist)
        {
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            var item = playlist.Playlist_[index];
            playlist.Playlist_.RemoveAt(index);

            var shuffledList = playlist.Playlist_;
            GeneralUtilities.Shuffle(playlist.Playlist_, new Random());
            G.Sys.GameManager_.LevelPlaylist_.Clear();
            G.Sys.GameManager_.LevelPlaylist_.Add(item);
            foreach (var lvl in shuffledList)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
        }
    }
}
