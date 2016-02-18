using System;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;

namespace TestPlugin
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "TestPlugin";
        public string Author => "Ciastex";
        public string Contact => "No contact provided.";

        public int CompatibleAPILevel => 1;

        public void Initialize(IManager manager)
        {
            Console.WriteLine(nameof(Entry));
        }

        public void Shutdown()
        {
            Console.WriteLine("TestPlugin Shutting Down!");
        }
    }
}
