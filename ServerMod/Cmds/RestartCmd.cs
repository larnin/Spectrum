using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class RestartCmd : Cmd
    {
        public override string name { get { return "restart"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!restart") + ": Restart the current map");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!GeneralUtilities.isOnGamemode())
            {
                MessageUtilities.sendMessage(p, "Can't restart on lobby !");
                return;
            }

            var serverLogic = G.Sys.GameManager_.GetComponent<ServerLogic>();
            if(serverLogic == null)
            {
                MessageUtilities.sendMessage(p, "ServerLogic null !");
                return;
            }

            var clients = (IEnumerable)PrivateUtilities.getPrivateField(serverLogic, "clientInfoList_");
            
            foreach(var c in clients)
            {
                if (p.NetworkPlayer_ == (UnityEngine.NetworkPlayer)PrivateUtilities.getPrivateField(c, "networkPlayer_"))
                {
                    exec(serverLogic, c);
                    break;
                }
            }
        }

        void exec(ServerLogic server, object client)
        {
            PrivateUtilities.callPrivateMethod(server, "SendClientToCurrentLevel", client);
        }
    }
}