using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Game.Network;
using System.Collections.Generic;
using System;
using Spectrum.API.Configuration;
using System.Linq;
using Events.MainMenu;
using Events.Network;
using Events.Player;
using Events.Local;
using UnityEngine;
using System.Collections;
using Events;
using System.Reflection;

namespace Spectrum.Plugins.AutoServer
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Auto server";
        public string Author => "Nico";
        public string Contact => "SteamID: larnin";
        public APILevel CompatibleAPILevel => APILevel.XRay;

        ClientPlayerInfo client;

        public void Initialize(IManager manager)
        {
            Initialized.Subscribe(data =>
            {
                onMainMenuInitialized(data);
            });
            ServerInitialized.Subscribe(data =>
            {
                onServerInitialized(data);
            });
            
            /*Server.Create("Test", "", 32);
            G.Sys.GameManager_.GoToCurrentLevel();*/
        }

        void onMainMenuInitialized(Initialized.Data data)
        {
            G.Sys.GameManager_.StartCoroutine(startServerCoroutine());
        }
        
        IEnumerator startServerCoroutine()
        {
            int state = 0;
            while(true)
            {
                yield return new WaitForEndOfFrame();
                switch(state)
                {
                    case 0:
                        var obj1 = MonoBehaviour.FindObjectOfType<MainMenuLogic>();
                        if(obj1 != null)
                        {
                            yield return new WaitForSeconds(0.1f);
                            obj1.OnMultiplayerClicked();
                            state++;
                        }
                        break;
                    case 1:
                        var obj2 = MonoBehaviour.FindObjectOfType<MainMenuLogic>();
                        if (obj2 != null)
                        {
                            yield return new WaitForSeconds(0.1f);
                            obj2.OnOnlineMPScreenClicked();
                            state++;
                        }
                        break;
                    case 2:
                        var obj3 = MonoBehaviour.FindObjectOfType<OnlineMenuLogic>();
                        if(obj3 != null)
                        {
                            yield return new WaitForSeconds(0.1f);
                            obj3.OnHostAGameClicked();
                            state++;
                        }
                        break;
                    case 3:
                        var obj4 = MonoBehaviour.FindObjectOfType<OnlineMenuLogic>();
                        if(obj4 != null)
                        {
                            yield return new WaitForSeconds(0.1f);
                            obj4.StartConnectionTimer();
                            Server.Create("Test server", "", 32);
                            state++;
                        }
                        break;
                    default:
                        break;
                }

                if (state > 3)
                    break;
                /*var obj = MonoBehaviour.FindObjectOfType<OnlineMenuLogic>();
                if (obj == null)
                    continue;

                Console.WriteLine("OnlineMenuLogic found !");
                obj.StartConnectionTimer();
                G.Sys.NetworkingManager_.CreateServer("Test server", "", 32);
                break;*/
            }

        }

        void onServerInitialized(ServerInitialized.Data data)
        {
            G.Sys.GameManager_.StartCoroutine(cleanCoroutine());
        }

        IEnumerator cleanCoroutine()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1);
            var clientLogic = G.Sys.PlayerManager_.GetComponent<ClientLogic>(); ;
            if (clientLogic != null)
            {
                var clientList = clientLogic.ClientPlayerList_;
                int i = 0;
                while (i < clientList.Count)
                {
                    var clientPlayerInfo = clientList[i];
                    if (clientPlayerInfo.NetworkPlayer_ == Network.player)
                    {
                        client = clientPlayerInfo;
                        clientList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                StaticEvent<PlayerListChanged.Data>.Broadcast(new PlayerListChanged.Data(clientList));
            }
            /*StaticEvent<ClientDisconnected.Data>.Broadcast(new ClientDisconnected.Data(Network.player, DisconnectionType.Quit));*/
            Console.WriteLine("serverInitialized");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1);
            G.Sys.GameManager_.GoToCurrentLevel();
        }

        public void Shutdown()
        {
            
        }
    }
}
