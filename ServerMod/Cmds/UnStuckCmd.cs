using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class UnstuckCmd : Cmd
    {
        public override string name { get { return "unstuck"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override bool showChatPublic(ClientPlayerInfo p)
        {
            return true;
        }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingWinList()
        };

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck") + ": Fire some events that try to unstuck things");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck 1") + ": The default, fire local ReadyToStartMode");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck 1 all") + ": Fire ReadyToStartMode for players that are not ready or started");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck 2") + ": Send StartMode to all ready or started clients");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck 2 all") + ": Send StartMode to all clients");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!unstuck 3") + ": Destroy the loading screen if found");
        }

        static string[] allowedStates = new string[]
        {
            "LoadedGameModeScene",
            "SubmittedGameModeInfo",
            "StartedMode"
        };

        public override void use(ClientPlayerInfo p, string message)
        {
            var match = Regex.Match(message, @"^(\d*) ?(.*)$");
            var mode = 1;
            var param = "";
            if (match.Success)
            {
                if (match.Groups[1].Value != "")
                    mode = int.Parse(match.Groups[1].Value);
                param = match.Groups[2].Value;
            }
            if (mode == 1 || mode == 2)
            {
                var isAll = param == "all";
                var addon = isAll ? " on all" : "on loaded, submitted, and started";
                if (mode == 1)
                {
                    MessageUtilities.sendMessage("Trying mode 1 (ReadyToStartMode) " + addon);
                    Console.WriteLine("Trying mode 1 (ReadyToStartMode) " + addon);
                }
                else
                {
                    MessageUtilities.sendMessage("Trying mode 2 (StartMode) " + addon);
                    Console.WriteLine("Trying mode 2 (StartMode) " + addon);
                }
                var startPlayers = new List<UnityEngine.NetworkPlayer>();
                var failedAny = false;
                var serverLogic = G.Sys.GameManager_.GetComponent<ServerLogic>();
                var clientInfoList = (System.Collections.IEnumerable)PrivateUtilities.getPrivateField(serverLogic, "clientInfoList_");
                foreach (object clientInfo in clientInfoList)
                {
                    // clientInfo is a private class so we can't actually use it :(
                    // we can't even reference it for List<ClientInfo>
                    // instead we have to use reflection to get the values of everything
                    // (at least, i'm not aware of any other way to use it)
                    var networkPlayer = (UnityEngine.NetworkPlayer)PrivateUtilities.getPrivateField(clientInfo, "networkPlayer_");
                    var client = GeneralUtilities.getClientFromNetworkPlayer(networkPlayer);
                    string name = client == null ? "Unknown" : MessageUtilities.closeTags(client.GetChatName());
                    string rawName = client == null ? "Unknown" : client.Username_;
                    var stateEnum = (Enum)PrivateUtilities.getPrivateField(clientInfo, "state_");
                    string state = stateEnum.ToString();
                    if (allowedStates.Contains(state) || isAll)
                        startPlayers.Add(networkPlayer);
                    else
                    {
                        failedAny = true;
                        MessageUtilities.sendMessage($"Not starting {name} because their state is {state}");
                        Console.WriteLine($"Not starting {rawName} because their state is {state}");
                    }
                }
                if (failedAny)
                {
                    MessageUtilities.sendMessage("You can force start all players using !unstuck all");
                    Console.WriteLine("You can force start all players using !unstuck all");
                }
                if (mode == 1)
                    Events.Server.ReadyToStartGameMode.Broadcast(new Events.Server.ReadyToStartGameMode.Data(startPlayers.ToArray()));
                else
                {
                    var gameMode = G.Sys.GameManager_.Mode_;
                    double startTime;
                    if (gameMode == null)
                    {
                        MessageUtilities.sendMessage("Mode_ is null! Something is very wrong! Using 0 for start time.");
                        Console.WriteLine("Mode_ is null! Something is very wrong! Using 0 for start time.");
                        startTime = 0;
                    }
                    else
                    {
                        startTime = (double)PrivateUtilities.callPrivateMethod(typeof(GameMode), gameMode, "CalcStartTime");
                    }

                    foreach (UnityEngine.NetworkPlayer player in startPlayers)
                    {
                        Events.ServerToClient.StartMode.Broadcast(player, new Events.ServerToClient.StartMode.Data(startTime, false));
                    }
                }
                MessageUtilities.sendMessage($"Tried to start {startPlayers.Count} players.");
                Console.WriteLine($"Tried to start {startPlayers.Count} players.");
            }
            else if (mode == 3)
            {
                MessageUtilities.sendMessage("Trying mode 3 (Destroy OnlineMatchWaitingScreen)");
                Console.WriteLine("Trying mode 3 (Destroy OnlineMatchWaitingScreen)");
                var screens = PrivateUtilities.getComponents<OnlineMatchWaitingScreen>();
                MessageUtilities.sendMessage($"Found {screens.Count} screens.");
                Console.WriteLine($"Found {screens.Count} screens.");
                foreach (var screen in screens)
                {
                    UnityEngine.Object.Destroy(screen.transform.root.gameObject);
                }
                MessageUtilities.sendMessage($"Destroyed {screens.Count} screens.");
                Console.WriteLine($"Destroyed {screens.Count} screens.");
            }
            else
            {
                MessageUtilities.sendMessage($"Unknown mode: {mode}");
                Console.WriteLine($"Unknown mode: {mode}");
            }
        }
    }
}
