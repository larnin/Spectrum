using Spectrum.Plugins.ServerMod.PlaylistTools;
using System;
using System.Collections.Generic;
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
            Utilities.sendMessage(Utilities.formatCmd("!list [filter]") + ": Show next levels in the current playlist with optional filter");
        }

        public override void use(ClientPlayerInfo p, string message)
        {

            LevelPlaylist currentList = G.Sys.GameManager_.LevelPlaylist_;

            if(currentList.Index_ >= currentList.Count_)
            {
                Utilities.sendMessage("There are no levels in the playlist!");
                return;
            }
            else if (currentList.Index_ == currentList.Count_ - 1)
            {
                Utilities.sendMessage("You are on the last level of the playlist!");
                return;
            }

            LevelCMD levelCmd = (LevelCMD) cmd.all.getCommand("level");

            var levelsUpcoming = currentList.Playlist_.GetRange(currentList.Index_ + 1, currentList.Count_ - currentList.Index_ - 1);
            FilteredPlaylist filterer = Utilities.getFilteredPlaylist(p, levelsUpcoming, message, false);
            
            Utilities.sendMessage("[FFFFFF]Upcoming:[-]");
            Utilities.sendMessage(Utilities.getPlaylistText(filterer, levelCmd.levelFormat));
        }
    }
}
