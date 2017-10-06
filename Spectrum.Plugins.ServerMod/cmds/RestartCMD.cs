using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Events.ClientToServer;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class RestartCMD : cmd
    {
        public override string name { get { return "restart"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public RestartCMD()
        {
            CompletedRequest.Subscribe(data =>
            {
                onRequest(data);
            });
        }

        void onRequest(CompletedRequest.Data data)
        {
            Console.Out.WriteLine("Request :" + data.request_);

            try
            {
                var server = G.Sys.GameManager_.GetComponent<ServerLogic>();
                Console.Out.WriteLine("Name " + Utilities.clientFromNetworkPlayer(data.networkPlayer_).GetChatName());
                var client = server.GetType().GetMethod("GetClientInfo", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(server, new object[] { data.networkPlayer_, false });
                if (client == null)
                    Console.Out.WriteLine("Client null !");
                else Console.Out.WriteLine("Current state " + client.GetType().GetField("state_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(client));
                Console.Out.WriteLine((bool)server.GetType().GetMethod("HasModeStarted", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(server, new object[] {}));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!restart: Restart the current map");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!Utilities.isOnGamemode())
            {
                Utilities.sendMessage("Can't restart on lobby !");
                return;
            }

            var serverLogic = G.Sys.GameManager_.GetComponent<ServerLogic>();
            //Console.Out.WriteLine("waiting " + serverLogic.GetType().GetField("waitingForClientsToSwitchLevels_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serverLogic));

            if (serverLogic == null)
            {
                Utilities.sendMessage("ServerLogic null !");
                return;
            }

            var clients = (IEnumerable)(serverLogic.GetType().GetField("clientInfoList_", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(serverLogic));
            
            foreach(var c in clients)
            {
                //var init = (bool)c.GetType().GetProperty("IsInitialized_", BindingFlags.Public | BindingFlags.Instance).GetGetMethod().Invoke(c, null);
                //Console.Out.WriteLine("init " + init);
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