
namespace ServerMod.cmds
{
    class ListCMD : cmd
    {
        public override string name { get { return "list"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!list: list all the players in the server.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!Utilities.isHost())
            {
                listAsClient();
                return;
            }

            string list = "";

            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                list += current.Index_ + " : " + current.Username_;
                if (current.IsLocal_)
                    list += " (HOST)";
                list += ", ";
            }

            list = list.Remove(list.Length - 2);
            Utilities.sendMessage(list);
        }

        public void listAsClient()
        {
            string text = string.Empty;
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                text += current.Index_ + ": " + current.Username_ + ", ";

            Utilities.sendMessage(text.Remove(text.Length - 2));
        }

    }
}
