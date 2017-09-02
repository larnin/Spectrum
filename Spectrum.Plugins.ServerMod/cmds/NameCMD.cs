
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Reflection;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class NameCMD : cmd
    {
        public override string name { get { return "name"; } }
        public override PermType perm { get { return PermType.LOCAL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!name [newName]") + ": Allow you to change your name");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            var clientLogic = G.Sys.PlayerManager_.GetComponent<ClientLogic>();
            if(clientLogic == null)
            {
                MessageUtilities.sendMessage("Error : Client logic null !");
                return;
            }

            try
            {
                var client = clientLogic.GetLocalPlayerInfo();
                var oldName = client.Username_;

                var name = client.GetType().GetField("username_", BindingFlags.NonPublic | BindingFlags.Instance);
                name.SetValue(client, message);

                MessageUtilities.sendMessage(oldName + " renamed to " + client.Username_);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                MessageUtilities.sendMessage("Error : can't change your name !");
            }
        }
    }
}
