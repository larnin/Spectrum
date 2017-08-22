
namespace Spectrum.Plugins.ServerMod.cmds
{
    class HelpCMD : cmd
    {
        public override string name { get { return "help"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("Realy ? You're stuck at the bottom of a well ?");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message.Length == 0)
            {
                listCmd(p);
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
            c.help(p);
            Utilities.sendMessage("Permission level : " + c.perm);
            if (c.perm == PermType.LOCAL)
                Utilities.sendMessage("This command can only be used by the local player");

        }

        private void listCmd(ClientPlayerInfo p)
        {
            Utilities.sendMessage("Available commands :");
            string list = "";
            foreach(var cName in cmd.all.commands())
            {
                cmd c = cmd.all.getCommand(cName);
                if (c.perm == PermType.HOST && (p.IsLocal_ && Utilities.isHost()))
                    list += cName+"(H), ";
                if (c.perm == PermType.LOCAL && p.IsLocal_)
                    list += cName+"(L), ";
                if (c.perm == PermType.ALL)
                    list += cName+", ";
            }
            Utilities.sendMessage(list.Remove(list.Length - 2));
            if (p.IsLocal_ || (p.IsLocal_ && Utilities.isHost())) 
                Utilities.sendMessage("(H) = host only / (L) = local client only");
            Utilities.sendMessage("Use !help <command> for more information on the command");
        }
    }
}
