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

        public void Initialize(IManager manager)
        {
            Events.Local.ChatSubmitMessage.Subscribe(data =>
            {
                Chat_MessageSent(G.Sys.PlayerManager_.Current_.profile_.Name_, data.message_);
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                var author = ExtractMessageAuthor(data.message_);

                if (author != G.Sys.PlayerManager_.Current_.profile_.Name_ && !IsSystemMessage(data.message_))
                    Chat_MessageReceived(author, ExtractMessageBody(data.message_));
            });
        }

        private void Chat_MessageSent(string author, string message)
        {
            if (!message.StartsWith("!"))
                return;

            var client = Utilities.localClient();
            if(client == null)
            {
                Console.WriteLine("Error: Local client can't be found !");
                return;
            }

            int pos = message.IndexOf(' ');
            string commandName = (pos > 0 ? message.Substring(1, message.IndexOf(' ')) : message).Trim();
            cmd c = cmd.all.getCommand(commandName);
            if (c == null)
                return;

            if(!Utilities.isHost() && !c.canUseAsClient && c.perm != PermType.LOCAL)
                return;

            exec(c, client, pos > 0 ? message.Substring(pos + 1).Trim() : "");
        }

        private void Chat_MessageReceived(string author, string message)
        {
            if (!message.StartsWith("!"))
                return;

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

                return withoutSecondColorTag;
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
    }
}