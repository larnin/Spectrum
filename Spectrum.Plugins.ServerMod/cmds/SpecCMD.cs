﻿using Events;
using Events.GameMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class SpecCMD : cmd
    {
        public override string name { get { return "spec"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!spec [id/name]") + ": Forces a player to spectate the game.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            var affects = "";
            foreach (ClientPlayerInfo client in Utilities.getClientsBySearch(message))
            {
                StaticTargetedEvent<Finished.Data>.Broadcast(client.NetworkPlayer_, default(Finished.Data));
                affects += $"{ p.Username_}, ";
            }
            if (affects == "")
                Utilities.sendMessage("Could not find any players with that name or index");
            else
                Utilities.sendMessage(affects + "is now spectating.");
        }
    }
}
