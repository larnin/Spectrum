using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class StuckCmd : Cmd
    {
        public override string name { get { return "stuck"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseLocal { get { return true; } }

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
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!stuck") + ": Print server stuck debugging information");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            var gameMode = G.Sys.GameManager_.Mode_;
            if (gameMode == null)
            {
                MessageUtilities.sendMessage("Mode_ is null! Something is very wrong!");
                Console.WriteLine("Mode_ is null! Something is very wrong!");
            }
            else
            {
                MessageUtilities.sendMessage($"GameMode Name_: {gameMode.Name_}");
                Console.WriteLine($"GameMode Name_: {gameMode.Name_}");
                MessageUtilities.sendMessage($"IsStarted_: {gameMode.IsStarted_}");
                Console.WriteLine($"IsStarted_: {gameMode.IsStarted_}");
            }
            MessageUtilities.sendMessage("Connected players:");
            MessageUtilities.sendMessage("gui : name : state : ready");
            Console.WriteLine("guid : name : state : ready : levelVersion");
            // more information is provided in the console. the message is meant to be sort-of compact.
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
                bool ready = (bool)PrivateUtilities.getPrivateProperty(clientInfo, "IsOkWithTheModeBeingStarted_");
                string levelVersion = (string)PrivateUtilities.getPrivateField(clientInfo, "levelVersion_");
                MessageUtilities.sendMessage($"{networkPlayer.guid} : {name} : {state} : {ready}");
                Console.WriteLine($"{networkPlayer.guid} : {rawName} : {state} : {ready} : {levelVersion}");
            }
            MessageUtilities.sendMessage("Try using !unstuck");
            Console.WriteLine("Try using !unstuck");
        }
    }
}
