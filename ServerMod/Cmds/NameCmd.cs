
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Reflection;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class NameCmd : Cmd
    {
        public override string name { get { return "name"; } }
        public override PermType perm { get { return PermType.LOCAL; } }
        public override bool canUseLocal { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!name [newName]") + ": Allow you to change your name");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            var clientLogic = G.Sys.PlayerManager_.GetComponent<ClientLogic>();
            if(clientLogic == null)
            {
                MessageUtilities.sendMessage(p, "Error : Client logic null !");
                return;
            }

            try
            {
                var client = clientLogic.GetLocalPlayerInfo();
                var oldName = client.Username_;

                PrivateUtilities.setPrivateField(client, "username_", message);

                MessageUtilities.sendMessage(p, oldName + " renamed to " + client.Username_);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                MessageUtilities.sendMessage(p, "Error : can't change your name !");
            }
        }
    }
}
