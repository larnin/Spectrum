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

            if (!Global.Settings.GetSection("Execution").GetValue<bool>("Enabled"))
            {
                Console.WriteLine("Manager: Spectrum is disabled. Set 'Enabled' entry to 'true' in settings to restore extension framework functionality.");
                IsEnabled = false;
                return;
            }
            ManagedDependencyResolver = new ExternalDependencyResolver();
            Hotkeys = new HotkeyManager();

            Scene.Loaded += (sender, args) =>
            {
                Game.ShowWatermark = Global.Settings.GetSection("Output").GetValue<bool>("ShowWatermark");

                if (Game.ShowWatermark)
                {
                    Game.WatermarkText = $"Distance {SystemVersion.DistanceBuild} ([00AADD]Spectrum[-] {SystemVersion.APILevel.ToString()})";
                }
            };

            if (Global.Settings.GetSection("Execution").GetValue<bool>("LoadPlugins"))
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

                if (!Global.Settings.SectionExists("Output"))
                {
                    RecreateSettings();
                }
                else
                {
                    if (!Global.Settings.GetSection("Output").ValueExists("LogToConsole"))
                    {
                        Global.Settings.GetSection("Output")["LogToConsole"] = true;
                    }

                    if (!Global.Settings.GetSection("Output").ValueExists("ShowWatermark"))
                    {
                        Global.Settings.GetSection("Output")["ShowWatermark"] = true;
                    }
                }

                if (!Global.Settings.SectionExists("Execution"))
                {
                    RecreateSettings();
                }
                else
                {
                    if (!Global.Settings.GetSection("Execution").ValueExists("FirstRun"))
                    {
                        Global.Settings.GetSection("Execution")["FirstRun"] = false;
                    }

                    if (!Global.Settings.GetSection("Execution").ValueExists("LoadPlugins"))
                    {
                        Global.Settings.GetSection("Execution")["LoadPlugins"] = true;
                    }

                    if (!Global.Settings.GetSection("Execution").ValueExists("Enabled"))
                    {
                        Global.Settings.GetSection("Execution")["Enabled"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MANAGER: Couldn't load settings. Defaults loaded. Exception below.\n{ex}");
            }
        }

        private void RecreateSettings()
        {
            Global.Settings.Values.Clear();
            Global.Settings.Sections.Clear();

            Global.Settings.AddSection("Output")["LogToConsole"] = true;
            Global.Settings.GetSection("Output")["ShowWatermark"] = true;

            Global.Settings.AddSection("Execution")["FirstRun"] = false;
            Global.Settings.GetSection("Execution")["LoadPlugins"] = true;
            Global.Settings.GetSection("Execution")["Enabled"] = true;

            Global.Settings.Save(true);
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
