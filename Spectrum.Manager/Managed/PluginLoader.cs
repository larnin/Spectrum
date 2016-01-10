using System;
using System.IO;
using System.Reflection;
using Spectrum.API.PluginInterfaces;

namespace Spectrum.Manager.Managed
{
    class PluginLoader
    {
        private string PluginDirectory { get; }
        private PluginContainer PluginContainer { get; }

        public PluginLoader(string pluginDirectory, PluginContainer pluginContainer)
        {
            PluginDirectory = pluginDirectory;
            PluginContainer = pluginContainer;
        }

        public void LoadPlugins()
        {
            var filePaths = Directory.GetFiles(PluginDirectory, "*.plugin.dll");

            foreach (var path in filePaths)
            {
                var asm = Assembly.LoadFrom(path);
                foreach (var exportedType in asm.GetExportedTypes())
                {
                    // All plugins MUST have a type named Entry.
                    if (exportedType.Name == "Entry")
                    {
                        // All plugins MUST implement IPlugin interface.
                        if (typeof (IPlugin).IsAssignableFrom(exportedType))
                        {
                            Console.WriteLine("Plugin contains required type. Cool.");

                            var plugin = (IPlugin) Activator.CreateInstance(exportedType);
                            var pluginInfo = new PluginInfo
                            {
                                Name = plugin.FriendlyName,
                                Enabled = true,
                                Plugin = plugin,
                                IsUpdatable = false
                            };

                            // Plugin may also implement IUpdatable interface.
                            if (typeof (IUpdatable).IsAssignableFrom(exportedType))
                            {
                                Console.WriteLine("Plugin is updatable. Using more resources, huh?");
                                pluginInfo.IsUpdatable = true;
                            }

                            PluginContainer.Add(pluginInfo);
                            return;
                        }
                        Console.WriteLine($"'{path}' is not a valid plugin. Does not implement common IPlugin interface.");
                    }
                    else
                    {
                        Console.WriteLine($"'{path}' is not a valid plugin. No entry point.");
                    }
                }
            }
        }
    }
}
