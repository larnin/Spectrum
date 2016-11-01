using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Game.Network;
using System.Collections.Generic;
using ServerMod.cmds;
using System;

namespace ServerMod
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Server commands Mod";
        public string Author => "Nico";
        public string Contact => "SteamID: larnin";
        public APILevel CompatibleAPILevel => APILevel.MicroWave;
        public string PluginVersion = "V0.2";

        //private List<string> hostCommands = new List<string>();

        public void Initialize(IManager manager)
        {
            Events.Local.ChatSubmitMessage.Subscribe(data =>
            {
                Chat_MessageSent(G.Sys.PlayerManager_.Current_.profile_.Name_, data.message_);
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                var author = ExtractMessageAuthor(data.message_);
                var steamName = G.Sys.Steamworks_.GetUserName().ToLower().Trim();
                var profileName = G.Sys.PlayerManager_.Current_.profile_.Name_.ToLower().Trim();

                if (!IsSystemMessage(data.message_) && (author.ToLower().Trim() != steamName && author.ToLower().Trim() != profileName))
                    Chat_MessageReceived(author, ExtractMessageBody(data.message_));
            });
        }

        private void Chat_MessageSent(string author, string message)
        {
            var client = Utilities.localClient();
            if (client == null)
                Console.WriteLine("Error: Local client can't be found !");

            if (message.StartsWith("%"))
            {
                if (Utilities.isHost())
                    return;

                int pos = message.IndexOf(' ');
                string commandName = (pos > 0 ? message.Substring(1, message.IndexOf(' ')) : message.Substring(1).Trim());
                cmd c = cmd.all.getCommand(commandName);
                if (!c.canUseAsClient && c.perm != PermType.LOCAL)
                {
                    Utilities.sendMessage("You can't use that command as client");
                    return;
                }

                exec(c, client, pos > 0 ? message.Substring(pos + 1).Trim() : "");
            }
            else
            {
                if (!message.StartsWith("!"))
                    return;

                if (message.ToLower().Trim() == "!plugin")
                {
                    printClient();
                    return;
                }

                if (!Utilities.isHost())
                    return;

                int pos = message.IndexOf(' ');
                string commandName = (pos > 0 ? message.Substring(1, message.IndexOf(' ')) : message.Substring(1)).Trim();
                cmd c = cmd.all.getCommand(commandName);
                if (c == null)
                {
                    Utilities.sendMessage("The command '" + commandName + "' don't exist.");
                    return;
                }

                exec(c, client, pos > 0 ? message.Substring(pos + 1).Trim() : "");
            }
        }

        private void Chat_MessageReceived(string author, string message)
        {
            if (!message.StartsWith("!"))
                return;

            if (message.ToLower().Trim() == "!plugin")
            {
                printClient();
                return;
            }

            var client = Utilities.clientFromName(author);
            if (client == null)
            {
                Utilities.sendMessage("Error: client can't be found");
                return;
            }
                
            int pos = message.IndexOf(' ');
            string commandName = (pos > 0 ? message.Substring(1, message.IndexOf(' ')) : message).Trim();
            cmd c = cmd.all.getCommand(commandName);

            if (c == null)
            {
                Utilities.sendMessage("The command '" + commandName + "' don't exist.");
                return;
            }

            if (c.perm == PermType.LOCAL)
                return;

            if(c.perm != PermType.ALL)
            {
                Utilities.sendMessage("You don't have the permission to do that !");
                return;
            }

            exec(c, client, pos > 0 ? message.Substring(pos + 1).Trim() : "");
        }

        private void exec(cmd c, ClientPlayerInfo p, string message)
        {
            try
            {
                c.use(p, message);
            }
            catch (Exception error)
            {
                Utilities.sendMessage("Error");
                Console.WriteLine(error);
            }
        }

        public void Shutdown()
        {
            
        }

        //take from newer spectrum version (stable can't use messages events)
        private static string ExtractMessageAuthor(string message)
        {
            try
            {
                // 1. [xxxxxx]user[xxxxxx]: adfsafasf
                var withoutFirstColorTag = message.Substring(message.IndexOf(']') + 1, message.Length - message.IndexOf(']') - 1);
                // 2. user[xxxxxx]: adfsafasf
                var withoutSecondColorTag = withoutFirstColorTag.Substring(0, withoutFirstColorTag.IndexOf('['));
                // 3. user

                return withoutSecondColorTag.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsSystemMessage(string message)
        {
            return message.Contains("[c]") && message.Contains("[/c]");
        }

        private static string ExtractMessageBody(string message)
        {
            try
            {
                // 1. [xxxxxx]user[xxxxxx]: body
                return message.Substring(message.IndexOf(':') + 1).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private void printClient()
        {
            Utilities.sendMessage(Utilities.localClient().GetChatName() + " " + PluginVersion);
        }
    }
}