
using System;
using System.Reflection;

namespace ServerMod.cmds
{
    class NameCMD : cmd
    {
        public override string name { get { return "name"; } }
        public override PermType perm { get { return PermType.LOCAL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!name [newName]: Allow you to change your name");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            var clientLogic = G.Sys.PlayerManager_.GetComponent<ClientLogic>();
            if(clientLogic == null)
            {
                Utilities.sendMessage("Error : Client logic null !");
                return;
            }

            try
            {
                var client = clientLogic.GetLocalPlayerInfo();
                var oldName = client.Username_;

                var name = client.GetType().GetField("username_", BindingFlags.NonPublic | BindingFlags.Instance);
                name.SetValue(client, message);

                Utilities.sendMessage(oldName + " renamed to " + client.Username_);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Utilities.sendMessage("Error : can't change your name !");
            }
        }
    }
}
