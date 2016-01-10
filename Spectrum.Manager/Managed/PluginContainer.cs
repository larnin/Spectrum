using System.Collections.Generic;

namespace Spectrum.Manager.Managed
{
    class PluginContainer : List<PluginInfo>
    {
        public bool SetPluginState(string name, bool enabled)
        {
            var plugin = GetPluginByName(name);
            if (plugin == null)
            {
                return false;
            }
            plugin.Enabled = enabled;
            return true;
        }

        public PluginInfo GetPluginByName(string name)
        {
            foreach (var pluginInfo in this)
            {
                if (pluginInfo.Name == name)
                    return pluginInfo;
            }
            return null;
        }
    }
}
