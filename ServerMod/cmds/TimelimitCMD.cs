using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod.cmds
{
    class TimelimitCMD : cmd
    {
        public override string name { get { return "timelimit"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }


        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!timelimit [time]: Change the max time for the reverse tag map");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                help(p);
                return;
            }

            int time = 0;
            int.TryParse(message, out time);

            if(time < 30 || time > 1800)
            {
                Utilities.sendMessage("The time must be between 30 and 1800 sec (30min).");
                return;
            }

            G.Sys.GameManager_.NextModeTimeLimit_ = time;
            Utilities.sendMessage("Next time gamemode will be " + time + " seconds lenght.");
        }
    }
}
