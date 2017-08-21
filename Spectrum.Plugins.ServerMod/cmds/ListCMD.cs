using System;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class ListCMD : cmd
    {
        public override string name { get { return "list"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!list") + ": Show next levels in the current playlist");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            const int maxLvls = 10;

            var currentPlaylist = G.Sys.GameManager_.LevelPlaylist_.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            if(index >= currentPlaylist.Count)
            {
                Utilities.sendMessage("There are no levels in the playlist!");
                return;
            }

            Utilities.sendMessage("Current and next levels:");
            for(int i = 0; i < maxLvls ; i++)
            {
                if (i + index >= currentPlaylist.Count)
                    break;
                var map = currentPlaylist[i + index];
                Utilities.sendMessage(map.levelNameAndPath_.levelName_ + " (" + G.Sys.GameManager_.GetModeName(map.mode_) + ")");
            }

            int moreCount = currentPlaylist.Count - (index + maxLvls);
            if (moreCount > 0)
                Utilities.sendMessage("And " + moreCount + " more ...");
        }
    }
}
