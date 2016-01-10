using System;
using Spectrum.API.PluginInterfaces;

namespace TestPlugin
{
    public class Entry : IPlugin, IUpdatable
    {
        public string FriendlyName => "TestPluginNotUpdatable";
        public string Author => "Ciastex";
        public string Contact => "No contact provided.";

        public int CompatibleAPILevel => 1;

        public void Initialize(params object[] args)
        {
            Console.WriteLine("TestPlugin Initialized!");
        }

        public void Shutdown(params object[] args)
        {
            Console.WriteLine("TestPlugin Shutting Down!");
        }

        public void Update()
        {
            Console.WriteLine("TestPlugin updated!");
        }
    }
}
