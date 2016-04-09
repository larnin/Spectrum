namespace Spectrum.API.Game.EventArgs.Network
{
    public class ServerCreatedEventArgs : System.EventArgs
    {
        public string Name { get; private set; }
        public string Password { get; private set; }
        public int Capacity { get; private set; }

        public ServerCreatedEventArgs(string name, string password, int capacity)
        {
            Name = name;
            Password = password;
            Capacity = capacity;
        }
    }
}
