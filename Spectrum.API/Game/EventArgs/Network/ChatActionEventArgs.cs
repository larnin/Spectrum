namespace Spectrum.API.Game.EventArgs.Network
{
    public class ChatActionEventArgs : System.EventArgs
    {
        public int PlayerIndex { get; private set; }
        public string Nickname { get; private set; }
        public string ActionMessage { get; private set; }

        public ChatActionEventArgs(int playerIndex, string nickname, string actionMessage)
        {
            PlayerIndex = playerIndex;
            Nickname = nickname;
            ActionMessage = actionMessage;
        }
    }
}
