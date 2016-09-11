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

            if(playlist.Count_ == 0)
            {
                Utilities.sendMessage("The playlist is empty !");
                return;
            }

            var shuffledList = playlist.Playlist_;
            Utilities.Shuffle(playlist.Playlist_, new Random());
            G.Sys.GameManager_.LevelPlaylist_.Clear();
            foreach (var lvl in shuffledList)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.NextLevelName_ = shuffledList[0].levelNameAndPath_.levelName_;
            G.Sys.GameManager_.NextLevelPath_ = shuffledList[0].levelNameAndPath_.levelPath_;
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);

            Utilities.sendMessage("Playlist shuffled !");
        }
    }
}
