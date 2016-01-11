using System;
using Spectrum.API.PluginInterfaces;

using Version = Spectrum.API.Version;

namespace TestPlugin
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "TestPlugin";
        public string Author => "Ciastex";
        public string Contact => "No contact provided.";

        public int CompatibleAPILevel => 1;

        public void Initialize(params object[] args)
        {
            Console.WriteLine($"TestPlugin running on Distance version {Version.DistanceBuild}");
        }

        public void Shutdown(params object[] args)
        {
            Console.WriteLine("TestPlugin Shutting Down!");
        }
    }
}
