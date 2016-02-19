namespace Spectrum.API.Game.EventArgs.Audio
{
    public class MusicGridEventArgs : System.EventArgs
    {
        public int Bar { get; }
        public int Beat { get; }

        public MusicGridEventArgs(int bar, int beat)
        {
            Bar = bar;
            Beat = beat;
        }
    }
}
