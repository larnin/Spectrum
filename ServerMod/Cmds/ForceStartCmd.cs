
using Spectrum.Plugins.ServerMod.Utilities;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class ForceStartCmd : Cmd
    {
        public override string name { get { return "forcestart"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!forcestart") + ": Forces the game to start regardless of the ready states of players in the lobby.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!GeneralUtilities.isOnLobby())
            {
                MessageUtilities.sendMessage(p, "You can't force the start here !");
                return;
            }

            G.Sys.GameManager_.GoToCurrentLevel();
            MessageUtilities.sendMessage("Game started !");
        }
    }
}
