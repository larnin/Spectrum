using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class StartvoteCMD : cmd
    {
        public override string name { get { return "startvote"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!startvote: ");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            //not implemented yet
        }
    }
}
