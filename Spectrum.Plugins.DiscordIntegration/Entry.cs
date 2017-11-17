using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Game.Network;
using System.Collections.Generic;
using System;
using Spectrum.API.Configuration;
using System.Linq;
using Events.Network;
using Events.Player;
using Events.Local;
using UnityEngine;
using System.Collections;
using Events;
using System.Reflection;
using Events.Game;
using Events.LevelEditor;
using System.IO;

namespace Spectrum.Plugins.DiscordIntegration
{
    public class Entry : IPlugin, IUpdatable
    {
        public string FriendlyName => "Discord integration";
        public string Author => "Nico";
        public string Contact => "SteamID: larnin";
        public APILevel CompatibleAPILevel => APILevel.XRay;

        DiscordRpc.RichPresence presence;
        public UnityEngine.Events.UnityEvent onConnect;
        public UnityEngine.Events.UnityEvent onDisconnect;
        public DiscordJoinEvent onJoin;
        public DiscordJoinEvent onSpectate;
        public DiscordJoinRequestEvent onJoinRequest;
        DiscordRpc.EventHandlers handlers;

        string uuidApp = "e3deb681-8334-4e90-bbeb-50f83230418e";

        public void Initialize(IManager manager)
        {
            initializeDiscord();

            LevelLoaded.Subscribe(data =>
            {
                onLevelLoaded(data);
            });

            EnterEditorMode.Subscribe(data =>
            {
                onEnterEditorMode(data);
            });

            Events.GameLobby.Initialized.Subscribe(data =>
            {
                onLobbyInitialized(data);
            });

            presence.details = "Some rpc integration test !";
            DiscordRpc.UpdatePresence(ref presence);
        }

        public void Shutdown()
        {
            DiscordRpc.Shutdown();
        }

        public void Update()
        {
            DiscordRpc.RunCallbacks();
        }

        void onLevelLoaded(LevelLoaded.Data e)
        {
            Console.WriteLine("Level Loaded ! " + G.Sys.GameManager_.LevelName_);
        }

        void onEnterEditorMode(EnterEditorMode.Data e)
        {
            Console.WriteLine("Enter editor mode !");
        }

        void onLobbyInitialized(Events.GameLobby.Initialized.Data e)
        {
            Console.WriteLine("game Lobby ! " + G.Sys.NetworkingManager_.serverTitle_);
        }

        void onMainInitialized(Events.MainMenu.Initialized.Data e)
        {
            Console.WriteLine("Main menu !");
        }

        void initializeDiscord()
        {
            Debug.Log("Discord: init");

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.spectateCallback += SpectateCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(uuidApp, ref handlers, true, SteamworksManager.AppID_.ToString());
        }

        public void ReadyCallback()
        {
            Console.WriteLine("Discord: ready");
            onConnect.Invoke();
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Console.WriteLine(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
            onDisconnect.Invoke();
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Console.WriteLine(string.Format("Discord: error {0}: {1}", errorCode, message));
        }

        public void JoinCallback(string secret)
        {
            Console.WriteLine(string.Format("Discord: join ({0})", secret));
            onJoin.Invoke(secret);
        }

        public void SpectateCallback(string secret)
        {
            Console.WriteLine(string.Format("Discord: spectate ({0})", secret));
            onSpectate.Invoke(secret);
        }

        public void RequestCallback(DiscordRpc.JoinRequest request)
        {
            Console.WriteLine(string.Format("Discord: join request {0}: {1}", request.username, request.userId));
            onJoinRequest.Invoke(request);
        }
    }
}
