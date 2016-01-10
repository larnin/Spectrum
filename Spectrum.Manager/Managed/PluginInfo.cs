using Spectrum.API.PluginInterfaces;

namespace Spectrum.Manager.Managed
{
    class PluginInfo
    {
        public string Name { get; internal set; }
        public bool Enabled { get; set; }
        public bool IsUpdatable { get; internal set; }

        public IPlugin Plugin { get; internal set; }

        public PluginInfo() { }

        public PluginInfo(string name, bool enabledByDefault, bool isUpdatable, IPlugin plugin)
        {
            Name = name;
            Enabled = enabledByDefault;
            IsUpdatable = isUpdatable;
            Plugin = plugin;
        }
    }
}
