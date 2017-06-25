using Events;
using Events.RaceMode;
using System;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class WelcomeCMD : cmd
    {
        public static string welcomeMessage = "";

        public override string name { get { return "welcome"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public WelcomeCMD()
        {

            Events.Server.StartClientLate.Subscribe(data =>
            {
                if (Utilities.isOnline() && Utilities.isHost())
                    onClientJoin(data.client_);
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!welcome: Hear the welcome message.");
            if (Utilities.isHost())
            {
                Utilities.sendMessage("You can set the welcome message with !settings");
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", p.Username_));
        }

        

        private void onClientJoin(NetworkPlayer client)
        {
            if (welcomeMessage != "")
            {
                foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                {
                    if (current.NetworkPlayer_ == client)
                    {
                        Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", current.Username_));
                        return;
                    }
                }
                Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", "Player"));
            }
        }

       
    }
}
