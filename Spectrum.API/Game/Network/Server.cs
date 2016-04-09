using System;
using Spectrum.API.Game.EventArgs.Network;
using UnityEngine;

namespace Spectrum.API.Game.Network
{
    public class Server
    {
        public static event EventHandler LobbyInitialized;
        public static event EventHandler<ServerCreatedEventArgs> ServerCreated;

        static Server()
        {
            Events.GameLobby.Initialized.Subscribe(data =>
            {
                LobbyInitialized?.Invoke(null, System.EventArgs.Empty);
            });
        }

        public static void Create(string serverTitle, string password, int maxPlayerCount)
        {
            UnityEngine.Network.InitializeSecurity();
            try
            {
                G.Sys.NetworkingManager_.password_ = password;
                G.Sys.NetworkingManager_.serverTitle_ = serverTitle;
                G.Sys.NetworkingManager_.maxPlayerCount_ = maxPlayerCount;

                G.Sys.GameData_.SetString("ServerTitleDefault", serverTitle);
                G.Sys.GameData_.SetInt("MaxPlayersDefault", maxPlayerCount);

                var ncError = UnityEngine.Network.InitializeServer(maxPlayerCount - 1, 32323, true);

                if (ncError == NetworkConnectionError.NoError)
                {
                    var eventArgs = new ServerCreatedEventArgs(serverTitle, password, maxPlayerCount);
                    ServerCreated?.Invoke(null, eventArgs);
                    return;
                }

                G.Sys.MenuPanelManager_.ShowError(ncError.ToString(), "Failed to start server");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
