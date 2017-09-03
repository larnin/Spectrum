using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class SaveCmd : Cmd
    {
        public override string name { get { return "save"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!save [name]") + ": Save the current playlist.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                MessageUtilities.sendMessage(p, "You can't save a playlist in trackmogrify");
                return;
            }

            if (message == "")
            {
                help(p);
                return;
            }

            var name = G.Sys.GameManager_.LevelPlaylist_.name;
            G.Sys.GameManager_.LevelPlaylist_.name = "LevelPlaylist";
            G.Sys.GameManager_.LevelPlaylist_.Name_ = message;
            G.Sys.GameManager_.LevelPlaylist_.Save();
            G.Sys.GameManager_.LevelPlaylist_.name = name;

            MessageUtilities.sendMessage(p, "Playlist saved as :" + message);
        }
    }
}
