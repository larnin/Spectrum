namespace Spectrum.API.Game.EventArgs.Audio
{
    public class MIDINoteEventArgs : System.EventArgs
    {
        public int Note { get; }
        public int Velocity { get; }

        public MIDINoteEventArgs(int note, int velocity)
        {
            Note = note;
            Velocity = velocity;
        }
    }
}
