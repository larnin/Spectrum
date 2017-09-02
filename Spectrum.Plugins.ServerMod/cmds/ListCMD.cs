using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class ListCmd : Cmd
    {
        public override string name { get { return "list"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!list [filter]") + ": Show next levels in the current playlist with optional filter");
        }

        public override void use(ClientPlayerInfo p, string message)
        {

            LevelPlaylist currentList = G.Sys.GameManager_.LevelPlaylist_;

            if(currentList.Index_ >= currentList.Count_)
            {
                MessageUtilities.sendMessage("There are no levels in the playlist!");
                return;
            }
            else if (currentList.Index_ == currentList.Count_ - 1)
            {
                MessageUtilities.sendMessage("You are on the last level of the playlist!");
                return;
            }

            LevelCmd levelCmd = Cmd.all.getCommand<LevelCmd>("level");

            var levelsUpcoming = currentList.Playlist_.GetRange(currentList.Index_ + 1, currentList.Count_ - currentList.Index_ - 1);
            FilteredPlaylist filterer = new FilteredPlaylist(levelsUpcoming);
            GeneralUtilities.addFiltersToPlaylist(filterer, p, message, false);
            
            MessageUtilities.sendMessage("[FFFFFF]Upcoming:[-]");
            MessageUtilities.sendMessage(GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Initial, levelCmd.levelFormat));
        }
    }
}
