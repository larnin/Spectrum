using Spectrum.Plugins.ServerMod.Utilities;
using System.Linq;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class ServerCMD : cmd
    {
        public override string name { get { return "server"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!server") + ": Show the server name");
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!server [new name]") + ": Modify the server name");
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!server private [password]") + ": Set the server private");
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!server public") + ": Set the server public");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "" || !GeneralUtilities.isHost() || p == null)
            {
                MessageUtilities.sendMessage(G.Sys.NetworkingManager_.serverTitle_);
                return;
            }

            if(!p.IsLocal_)
            {
                MessageUtilities.sendMessage("You don't have the permission to do that !");
                return;
            }

            var words = message.Split(' ');
            if(words[0].ToLower() == "private")
            {
                if(words.Count() < 2)
                {
                    help(p);
                    return;
                }
                setServerPrivate(message.Substring(message.IndexOf(' ')+1));
                return;
            }

            if(words[0].ToLower() == "public")
            {
                setServerPublic();
                return;
            }

            setServerName(message);
        }

        private void setServerPrivate(string pass)
        {
            G.Sys.NetworkingManager_.password_ = pass;
            G.Sys.NetworkingManager_.privateServer_ = true;
            Network.incomingPassword = pass;
            updateMaster();
            MessageUtilities.sendMessage("The server is now private !");
        }

        private void setServerPublic()
        {
            G.Sys.NetworkingManager_.password_ = "";
            G.Sys.NetworkingManager_.privateServer_ = false;
            Network.incomingPassword = "";
            updateMaster();
            MessageUtilities.sendMessage("The server is now public !");
        }

        private void setServerName(string name)
        {
            G.Sys.NetworkingManager_.serverTitle_ = name;
            updateMaster();
            MessageUtilities.sendMessage("The server is renamed to " + name);
        }

        private void updateMaster()
        {
            G.Sys.NetworkingManager_.ReportToMasterServer();
        }
    }
}
