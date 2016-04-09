namespace Spectrum.API.Game.EventArgs.Network
{
    public class GameModeChangedEventArgs : System.EventArgs
    {
        public string Name { get; private set; }

        public GameModeChangedEventArgs(string name)
        {
            Name = name;
        }
    }
}
