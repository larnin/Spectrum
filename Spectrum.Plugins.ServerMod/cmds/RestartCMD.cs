using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Events.ClientToServer;
using Events;
using Events.ServerToClient;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class RestartCMD : cmd
    {
        List<NetworkPlayer> restartingPlayers;

        public override string name { get { return "restart"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public RestartCMD()
        {
            restartingPlayers = new List<NetworkPlayer>();
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
            

                if(data.request_ == ServerRequest.SubmitGameModeInfo && restartingPlayers.Contains(data.networkPlayer_))
                {
                    restartingPlayers.Remove(data.networkPlayer_);
                    Console.WriteLine("Player restarting !");

                    G.Sys.GameManager_.StartCoroutine(startCoroutine(data.networkPlayer_));
                    //StaticTargetedEvent<StartMode.Data>.Broadcast(data.networkPlayer_, new StartMode.Data(0, true));

                    /*var mode = G.Sys.GameManager_.Mode_;
                    if (mode == null)
                    {
                        Console.WriteLine("Game mode null");
                        StaticTargetedEvent<StartMode.Data>.Broadcast(data.networkPlayer_, new StartMode.Data(0, true));
                    }
                    else
                    {
                        try
                        {
                            double startTime = (double)(mode.GetType().GetMethod("CalcStartTime", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mode, new object[] { }));
                            Console.WriteLine("Send time !");
                            StaticTargetedEvent<StartMode.Data>.Broadcast(data.networkPlayer_, new StartMode.Data(startTime, true));
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Start time null !");
                            Console.WriteLine(e);
                        }
                    }*/
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        IEnumerator startCoroutine(NetworkPlayer player)
        {
            yield return new WaitForSeconds(1);
            StaticTargetedEvent<StartMode.Data>.Broadcast(player, new StartMode.Data(0, true));
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

            if(p.IsLocal_)
            {
                Utilities.sendMessage("Can't restart as Host !");
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
                    NetworkEx.SetSendingEnabled(p.NetworkPlayer_, NetworkGroup.GlobalGroup, true);
                    NetworkEx.SetSendingEnabled(p.NetworkPlayer_, NetworkGroup.LobbyGroup, true);
                    NetworkEx.SetSendingEnabled(p.NetworkPlayer_, NetworkGroup.GameModeGroup, true);

                    NetworkEx.SetReceivingEnabled(p.NetworkPlayer_, NetworkGroup.GlobalGroup, true);
                    NetworkEx.SetReceivingEnabled(p.NetworkPlayer_, NetworkGroup.LobbyGroup, true);
                    NetworkEx.SetReceivingEnabled(p.NetworkPlayer_, NetworkGroup.GameModeGroup, true);

                    restartingPlayers.Add(p.NetworkPlayer_);
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