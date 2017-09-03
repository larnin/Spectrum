using Events;
using Events.GameMode;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class PlayersCmd : Cmd
    {
        public override string name { get { return "players"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!players <search string>") + ": Find players using <search string>");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            var affects = "";
            foreach (ClientPlayerInfo client in GeneralUtilities.getClientsBySearch(message))
            {
                affects += $"{ client.Username_} ({client.Index_})";
                if (GeneralUtilities.isHost() && client.IsLocal_)
                    affects += " (HOST)";
                affects += ", ";
            }
            if (affects == "")
                MessageUtilities.sendMessage(p, "Could not find any players with that name or index");
            else
                MessageUtilities.sendMessage(p, "Found: " + affects);
        }
    }
}
