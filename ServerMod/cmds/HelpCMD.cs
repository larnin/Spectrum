
namespace ServerMod.cmds
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
                list += cName;
                if (c.perm == PermType.HOST)
                    list += "(H)";
                if (c.perm == PermType.LOCAL)
                    list += "(L)";
                list += ", ";
            }
            Utilities.sendMessage(list.Remove(list.Length - 2));
            Utilities.sendMessage("(H) = host only / (L) = local client only");
            Utilities.sendMessage("Use !help <command> for more information on the command");
        }
    }
}
