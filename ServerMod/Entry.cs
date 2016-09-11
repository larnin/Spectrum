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
            Chat.MessageSent += Chat_MessageSent;
            Chat.MessageReceived += Chat_MessageReceived;
        }

        private void Chat_MessageSent(object sender, Spectrum.API.Game.EventArgs.Network.ChatMessageEventArgs e)
        {
            if (!e.Message.StartsWith("!"))
                return;

            var client = Utilities.localClient();
            if(client == null)
            {
                Console.WriteLine("Error: Local client can't be found !");
                return;
            }

            int pos = e.Message.IndexOf(' ');
            string commandName = (pos > 0 ? e.Message.Substring(1, e.Message.IndexOf(' ')) : e.Message).Trim();
            cmd c = cmd.all.getCommand(commandName);
            if (c == null)
                return;

            if(!Utilities.isHost() && !c.canUseAsClient && c.perm != PermType.LOCAL)
                return;

            exec(c, client, pos > 0 ? e.Message.Substring(pos + 1).Trim() : "");
        }

        private void Chat_MessageReceived(object sender, Spectrum.API.Game.EventArgs.Network.ChatMessageEventArgs e)
        {
            if (!e.Message.StartsWith("!"))
                return;

            var client = Utilities.clientFromName(e.Author);
            if (client == null)
            {
                Utilities.sendMessage("Error: client can't be found");
                return;
            }
                
            int pos = e.Message.IndexOf(' ');
            string commandName = (pos > 0 ? e.Message.Substring(1, e.Message.IndexOf(' ')) : e.Message).Trim();
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

            exec(c, client, pos > 0 ? e.Message.Substring(pos + 1).Trim() : "");
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
    }
}