
using Spectrum.Plugins.ServerMod.Utilities;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class HelpCmd : Cmd
    {
        public override string name { get { return "help"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseLocal { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, "Really? You're stuck at the bottom of a well?");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message.Length == 0 || (p.IsLocal_ && message.ToLower() == "all"))
            {
                listCmd(p, message.ToLower() == "all");
                return;
            }

            int pos = message.IndexOf(' ');
            string commandName = (pos > 0 ? message.Substring(1, message.IndexOf(' ')) : message).Trim();
            Cmd c = Cmd.all.getCommand(commandName);
            if (c == null)
            {
                MessageUtilities.sendMessage(p, "The command '" + commandName + "' don't exist.");
                return;
            }
            c.help(p);
            MessageUtilities.sendMessage(p, "Permission level: " + c.perm);
            if (c.perm == PermType.LOCAL)
                MessageUtilities.sendMessage(p, "This command can only be used by the local player");

        }

        private void listCmd(ClientPlayerInfo p, bool showAll)
        {
            var playerIsLocal = p.IsLocal_;
            var playerIsHost = p.IsLocal_ && GeneralUtilities.isHost();
            var playerIsClient = p.IsLocal_ && !GeneralUtilities.isHost();
            var playerIsConnectedClient = !p.IsLocal_ && GeneralUtilities.isHost();
            MessageUtilities.sendMessage(p, "Available commands:");
            string list = "";
            foreach(var cName in Cmd.all.commands())
            {
                Cmd c = Cmd.all.getCommand(cName);

                var allowed = showAll;
                if (playerIsLocal)
                {
                    if (c.perm == PermType.LOCAL || c.canUseLocal)
                    {
                        allowed = true;
                    }
                }
                if (!playerIsClient)
                {
                    if (c.perm == PermType.HOST)
                    {
                        if (playerIsHost)
                        {
                            allowed = true;
                        }
                    }
                    else
                    {
                        allowed = true;
                    }
                }
                if (allowed)
                {
                    list += "!" + cName;

                    if (c.perm == PermType.HOST)
                        list += "(H)";

                    if (p.IsLocal_ && (c.perm == PermType.LOCAL || c.canUseLocal))
                        list += "(L)";

                    list += ", ";   
                }
            }
            MessageUtilities.sendMessage(p, list.Remove(list.Length - 2));
            if (p.IsLocal_ || (p.IsLocal_ && GeneralUtilities.isHost())) 
                MessageUtilities.sendMessage(p, "(H) = host only / (L) = local client");
            MessageUtilities.sendMessage(p, "Use !help <command> for more information on the command.");
            if (playerIsLocal && !playerIsHost)
                MessageUtilities.sendMessage(p, "Use !help all to see every command, including ones you cannot use right now.");
        }
    }
}
