namespace Spectrum.API.Game.EventArgs.Network
{
    public class ChatMessageEventArgs : System.EventArgs
    {
        public string Author { get; private set; }
        public string Message { get; private set; }

        public ChatMessageEventArgs(string author, string message)
        {
            Author = author;
            Message = message;
        }
    }
}
