using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class PluginCMD : cmd
    {
        public override string name { get { return "plugin"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!plugin: Shows all players who have the plugin and versions");
        }

        public override void use(ClientPlayerInfo p, string message)
        {

        }
    }
}
