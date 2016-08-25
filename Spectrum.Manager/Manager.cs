using System;
using System.IO;
using Spectrum.API;
using Spectrum.API.Configuration;
using Spectrum.API.Game;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Input;
using Spectrum.Manager.Managed;

namespace Spectrum.Manager
{
    public class Manager : IManager
    {
        private PluginContainer ManagedPluginContainer { get; set; }
        private PluginLoader ManagedPluginLoader { get; set; }
        private ExternalDependencyResolver ManagedDependencyResolver { get; set; }

        public IHotkeyManager Hotkeys { get; set; }

        public bool IsEnabled { get; set; }
        public bool CanLoadPlugins => Directory.Exists(Defaults.PluginDirectory);

        public Manager()
        {
            IsEnabled = true;

            CheckPaths();
            InitializeSettings();

            if (!Global.Settings.GetValue<bool>("Enabled"))
            {
                Console.WriteLine("Manager: Spectrum is disabled. Set 'Enabled' entry to 'true' in settings to restore extension framework functionality.");
                IsEnabled = false;
                return;
            }
            ManagedDependencyResolver = new ExternalDependencyResolver();
            Hotkeys = new HotkeyManager(this);
            
            Scene.Loaded += (sender, args) =>
            {
                Game.ShowWatermark = Global.Settings.GetValue<bool>("ShowWatermark");

                if (Game.ShowWatermark)
                {
                    Game.WatermarkText = $"Distance {SystemVersion.DistanceBuild} ([00AADD]Spectrum[-] {SystemVersion.APILevel.ToString()})";
                }
            };

            if (Global.Settings.GetValue<bool>("LoadPlugins"))
            {
                LoadExtensions();
                StartExtensions();
            }
        }

        public void CheckPaths()
        {
            if (!Directory.Exists(Defaults.SettingsDirectory))
            {
                Console.WriteLine("Settings directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.SettingsDirectory);
            }

            if (!Directory.Exists(Defaults.LogDirectory))
            {
                Console.WriteLine("Log directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.LogDirectory);
            }

            if (!Directory.Exists(Defaults.PluginDataDirectory))
            {
                Console.WriteLine("Plugin data directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.PluginDataDirectory);
            }

            if (!Directory.Exists(Defaults.PluginDirectory))
            {
                Console.WriteLine("Plugin directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.PluginDirectory);
            }

            if (!Directory.Exists(Defaults.ResolverDirectory))
            {
                Console.WriteLine("External dependency resolver directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.ResolverDirectory);
            }
        }

        public void UpdateExtensions()
        {
            if (!IsEnabled)
                return;

            ((HotkeyManager)Hotkeys).Update();

            if (ManagedPluginContainer != null)
            {
                foreach (var pluginInfo in ManagedPluginContainer)
                {
                    if (pluginInfo.Enabled && pluginInfo.UpdatesEveryFrame)
                    {
                        var plugin = pluginInfo.Plugin as IUpdatable;
                        plugin?.Update();
                    }
                }
            }
        }

        private void InitializeSettings()
        {
            try
            {
                Global.Settings = new Settings(typeof(Manager));
                if (Global.Settings["FirstRun"] == string.Empty || Global.Settings.GetValue<bool>("FirstRun"))
                {
                    RecreateSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MANAGER: Couldn't load settings. Defaults loaded. Exception below.\n{ex}");
            }
        }

        private void RecreateSettings()
        {
            Global.Settings["FirstRun"] = "false";
            Global.Settings["LoadPlugins"] = "true";
            Global.Settings["LoadScripts"] = "true";
            Global.Settings["LogToConsole"] = "true";
            Global.Settings["ShowWatermark"] = "true";
            Global.Settings["Enabled"] = "true";

            Global.Settings.Save();
        }

        private void LoadExtensions()
        {
            ManagedPluginContainer = new PluginContainer();
            ManagedPluginLoader = new PluginLoader(Defaults.PluginDirectory, ManagedPluginContainer);
            ManagedPluginLoader.LoadPlugins();
        }

        private void StartExtensions()
        {
            foreach (var pluginInfo in ManagedPluginContainer)
            {
                pluginInfo.Plugin.Initialize(this);
            }
        }
    }
}
