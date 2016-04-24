namespace Spectrum.API.Game.EventArgs.Game
{
    public class GameModeFinishedEventArgs : System.EventArgs
    {
        public API.Game.Network.NetworkGroup NetworkGroup { get; }

        public GameModeFinishedEventArgs(API.Game.Network.NetworkGroup networkGroup)
        {
            NetworkGroup = networkGroup;
        }
    }
}
