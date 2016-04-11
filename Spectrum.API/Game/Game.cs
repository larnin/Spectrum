namespace Spectrum.API.Game
{
    public class Game
    {
        public static GameMode CurrentMode => (GameMode)G.Sys.GameManager_.Mode_.GameModeID_;
    }
}
