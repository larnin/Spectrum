namespace Spectrum.API.Game.EventArgs.Network
{
    public class ChatMessageEventArgs : System.EventArgs
    {
        public string Message { get; private set; }

        public ChatMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
