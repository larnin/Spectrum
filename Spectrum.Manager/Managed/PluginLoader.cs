using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.Manager.Logging;
using SystemVersion = Spectrum.API.SystemVersion;

namespace Spectrum.Manager.Managed
{
    class PluginLoader
    {
        private string PluginDirectory { get; }
        private PluginContainer PluginContainer { get; }

        private SubsystemLog Log { get; }

        private int _loadedPlugins;

        public PluginLoader(string pluginDirectory, PluginContainer pluginContainer)
        {
            PluginDirectory = pluginDirectory;
            PluginContainer = pluginContainer;

            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.PluginLoaderLogFileName));
            Log.Info("Plugin loader starting up...");
        }

        public void LoadPlugins()
        {
            Log.Info("Starting load procedure.");
            var filePaths = Directory.GetFiles(PluginDirectory, "*.plugin.dll");

            foreach (var path in filePaths)
            {
                var fileName = Path.GetFileName(path);

                Assembly asm;
                try
                {
                    Log.Info($"Now loading library file: '{fileName}'");
                    asm = Assembly.LoadFrom(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occured while loading library file: '{fileName}'. Check the log for details.");
                    Log.Exception(e);

                    continue;
                }

                Type[] exportedTypes;
                try
                {
                    exportedTypes = asm.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException rtlex)
                {
                    Console.WriteLine($"Couldn't load the plugin '{fileName}'. Was it built for an earlier Spectrum version?");
                    Log.ExceptionSilent(rtlex);

                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occured while validating library file: '{fileName}'. Check the log for details.");
                    Log.Exception(e);

                    continue;
                }

                if (exportedTypes.FirstOrDefault(x => x.Name == "Entry") == null)
                {
                    Console.WriteLine($"No entry point detected. Skipping the file ${fileName}");
                    continue;
                }

                foreach (var exportedType in exportedTypes)
                {
                    // All plugins MUST have a type named Entry.
                    if (exportedType.Name == "Entry")
                    {
                        Log.Info("Plugin contains a valid entry point. Proceeding...");
                        try
                        {
                            Log.Info("Trying to validate the plugin...");
                            // All plugins MUST implement IPlugin interface.
                            if (typeof (IPlugin).IsAssignableFrom(exportedType))
                            {
                                IPlugin plugin;

                                try
                                {
                                    plugin = (IPlugin)Activator.CreateInstance(exportedType);
                                }
                                catch (TypeLoadException tlex)
                                {
                                    Log.Error($"Couldn't load the plugin '{fileName}'. Was it built for an earlier Spectrum version?");
                                    Log.ExceptionSilent(tlex);
                                    break;
                                }
                                catch(Exception ex)
                                {
                                    Log.Error($"An unexpected exception occured while loading '{fileName}'. The plugin wasn't loaded. Check the log for details.");
                                    Log.Exception(ex);
                                    break;
                                }

                                var pluginInfo = new PluginInfo
                                {
                                    Name = plugin.FriendlyName,
                                    Enabled = true,
                                    Plugin = plugin,
                                    UpdatesEveryFrame = false
                                };

                                if (pluginInfo.Plugin.CompatibleAPILevel != SystemVersion.APILevel)
                                {
                                    Log.Info("The plugin is not built for the current API level. It may have an unexpected behavior.");
                                }

                                // Plugin MAY also implement IUpdatable interface.
                                if (typeof (IUpdatable).IsAssignableFrom(exportedType))
                                {
                                    Log.Info("The plugin is going to be updated every frame.");
                                    pluginInfo.UpdatesEveryFrame = true;
                                }

                                if (PluginContainer.GetPluginByName(pluginInfo.Name) == null)
                                {
                                    PluginContainer.Add(pluginInfo);

                                    Log.Info("Succesfully loaded a new plugin:\n" +
                                             $"   Name: {pluginInfo.Name}\n" +
                                             $"   Author: {pluginInfo.Plugin.Author}\n" +
                                             $"   APILevel: {pluginInfo.Plugin.CompatibleAPILevel}\n" +
                                             $"   Contact: {pluginInfo.Plugin.Contact}\n" +
                                             $"   Updates every frame: {pluginInfo.UpdatesEveryFrame}");

                                    _loadedPlugins++;
                                }
                                else
                                {
                                    Log.Error($"Did NOT load the plugin: '{fileName}'. A plugin with this name has already been loaded once.");
                                }
                                continue;
                            }
                            Log.Error($"'{fileName}' is not a valid plugin. Does not implement common IPlugin interface.");
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                }
            }
            Log.Info($"Load complete. Loaded {_loadedPlugins} plugin(s).");
        }
    }
}
