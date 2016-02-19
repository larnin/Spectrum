namespace Spectrum.API.Game.EventArgs.Audio
{
    public class CustomMusicChangedEventArgs : System.EventArgs
    {
        public string NewTrackName { get; }

        public CustomMusicChangedEventArgs(string newTrackName)
        {
            NewTrackName = newTrackName;
        }
    }
}
