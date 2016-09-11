
namespace ServerMod.cmds
{
    class ForceStartCMD : cmd
    {
        public override string name { get { return "forcestart"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!forcestart: Forces the game to start regardless of the ready states of players in the lobby.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!Utilities.isOnLobby())
            {
                Utilities.sendMessage("You can't force the start here !");
                return;
            }

            G.Sys.GameManager_.GoToCurrentLevel();
            Utilities.sendMessage("Game started !");
        }
    }
}
