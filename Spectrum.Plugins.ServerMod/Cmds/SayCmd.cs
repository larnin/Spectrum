
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Reflection;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class SayCmd : Cmd
    {
        public override string name { get { return "say"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!say <formatted text>") + ": Say text with formatting.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {

            var client = GeneralUtilities.localClient();
            if (client == null)
            {
                Console.WriteLine("Error: Local client can't be found (SayCmd)!");
                return;
            }
            MessageUtilities.sendMessage(client.GetChatName() + ": " + message);
        }
    }
}
