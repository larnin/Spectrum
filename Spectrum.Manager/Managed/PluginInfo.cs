using Spectrum.API.Interfaces.Plugins;

namespace Spectrum.Manager.Managed
{
    class PluginInfo
    {
        public string Name { get; internal set; }
        public bool Enabled { get; set; }
        public bool UpdatesEveryFrame { get; internal set; }

        public IPlugin Plugin { get; internal set; }

        public PluginInfo() { }

        public PluginInfo(string name, bool enabledByDefault, bool updatesEveryFrame, IPlugin plugin)
        {
            Name = name;
            Enabled = enabledByDefault;
            UpdatesEveryFrame = updatesEveryFrame;
            Plugin = plugin;
        }
    }
}
