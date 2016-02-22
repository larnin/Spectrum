using System;
using UnityEngine;

namespace Spectrum.API.Game.Network
{
    public class Server
    {
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
                    return;

                G.Sys.MenuPanelManager_.ShowError(ncError.ToString(), "Failed to start server");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }
}
