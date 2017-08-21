﻿using Events;
using Events.GameMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class KickCMD : cmd
    {
        public override string name { get { return "kick"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!kick <search string>") + ": Kick players using <search string>");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                help(p);
                return;
            }

            var affects = "";
            foreach (ClientPlayerInfo client in Utilities.getClientsBySearch(message))
            {
                affects += $"{ client.Username_}, ";
                G.Sys.NetworkingManager_.DisconnectPlayer(client.Index_);
            }
            if (affects == "")
                Utilities.sendMessage("Could not find any players with that name or index");
            else
                Utilities.sendMessage("Kicked " + affects);
        }
    }
}