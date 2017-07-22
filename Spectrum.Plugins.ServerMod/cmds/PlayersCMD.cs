using Events;
using Events.GameMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class PlayersCMD : cmd
    {
        public override string name { get { return "players"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!players <search string>") + ": Find players using <search string>");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Console.WriteLine("in players");
            if (message == "")
            {
                help(p);
                return;
            }

            var affects = "";
            foreach (ClientPlayerInfo client in Utilities.getClientsBySearch(message))
            {
                affects += $"{ client.Username_} ({client.Index_}), ";
            }
            if (affects == "")
                Utilities.sendMessage("Could not find any players with that name or index");
            else
                Utilities.sendMessage("Found: " + affects);
        }
    }
}
