using Events;
using Events.GameMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class SpecCMD : cmd
    {
        public override string name { get { return "spec"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!spec [id/name]: Forces a player to spectate the game.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            int index = 0;
            bool parsed = int.TryParse(message, out index);

            ClientPlayerInfo player = parsed ? Utilities.clientFromID(index) : Utilities.clientFromName(message);
            if(player == null)
            {
                Utilities.sendMessage("Can't find the player '" + message + "'");
                return;
            }

            StaticTargetedEvent<Finished.Data>.Broadcast(player.NetworkPlayer_, default(Finished.Data));

            Utilities.sendMessage("Player " + message + " is now spectating.");
        }
    }
}
