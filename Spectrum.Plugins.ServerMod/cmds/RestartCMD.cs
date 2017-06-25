using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class RestartCMD : cmd
    {
        public override string name { get { return "restart"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!restart") + ": Restart the current map");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!Utilities.isOnGamemode())
            {
                Utilities.sendMessage("Can't restart on lobby !");
                return;
            }

            var serverLogic = G.Sys.GameManager_.GetComponent<ServerLogic>();
            if(serverLogic == null)
            {
                Utilities.sendMessage("ServerLogic null !");
                return;
            }

            var clients = (IEnumerable)(serverLogic.GetType().GetField("clientInfoList_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serverLogic));
            
            foreach(var c in clients)
            {
                if (p.NetworkPlayer_.Equals(c.GetType().GetField("networkPlayer_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c)))
                {
                    exec(serverLogic, c);
                    break;
                }
            }
        }

        void exec(ServerLogic server, object client)
        {
            server.GetType().GetMethod("SendClientToCurrentLevel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(server, new object[] { client });
        }
    }
}