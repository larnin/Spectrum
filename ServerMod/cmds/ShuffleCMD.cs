using System;

namespace ServerMod.cmds
{
    class ShuffleCMD : cmd
    {
        public override string name { get { return "shuffle"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!shuffle: Shuffle the current playlist");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                Utilities.sendMessage("You can't manage the playlist in trackmogrify");
                return;
            }

            if (playlist.Count_ == 0)
            {
                Utilities.sendMessage("The playlist is empty !");
                return;
            }

            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            var item = playlist.Playlist_[index];
            playlist.Playlist_.RemoveAt(index);

            var shuffledList = playlist.Playlist_;
            Utilities.Shuffle(playlist.Playlist_, new Random());
            G.Sys.GameManager_.LevelPlaylist_.Clear();
            G.Sys.GameManager_.LevelPlaylist_.Add(item);
            foreach (var lvl in shuffledList)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);

            G.Sys.GameManager_.NextLevelName_ = item.levelNameAndPath_.levelName_;
            G.Sys.GameManager_.NextLevelPath_ = item.levelNameAndPath_.levelPath_;
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);

            Utilities.sendMessage("Playlist shuffled !");
        }
    }
}
