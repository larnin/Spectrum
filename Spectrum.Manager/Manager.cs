using System;
using System.IO;
using Spectrum.API;
using Spectrum.API.Configuration;
using Spectrum.API.Game;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Input;
using Spectrum.Manager.Lua;
using Spectrum.Manager.Managed;

namespace Spectrum.Manager
{
    public class Manager : IManager
    {
        public ILoader LuaLoader { get; set; }
        public IExecutor LuaExecutor { get; set; }
        public IHotkeyManager Hotkeys { get; set; }

        private PluginContainer ManagedPluginContainer { get; set; }
        private PluginLoader ManagedPluginLoader { get; set; }
        private ExternalDependencyResolver ManagedDependencyResolver { get; set; }

        private Settings ScriptHotkeySettings { get; set; }

        private string ScriptDirectory { get; }
        private string OnDemandScriptDirectory { get; }
        private string PluginDirectory { get; }

        public bool CanLoadScripts => Directory.Exists(ScriptDirectory);
        public bool CanLoadPlugins => Directory.Exists(PluginDirectory);

        public Manager()
        {
            CheckPaths();
            InitializeSettings();

            if (!Global.Settings.GetValue<bool>("Enabled"))
            {
                Console.WriteLine("Manager: Spectrum is disabled. Set 'Enabled' entry to 'true' in settings to restore extension framework functionality.");
                return;
            }
            ManagedDependencyResolver = new ExternalDependencyResolver();

            Hotkeys = new HotkeyManager(this);
            InitializeScriptHotkeys();

            ScriptDirectory = Defaults.ScriptDirectory;
            PluginDirectory = Defaults.PluginDirectory;
            OnDemandScriptDirectory = Defaults.OnDemandScriptDirectory;

            Scene.Loaded += (sender, args) =>
            {
                Game.ShowWatermark = Global.Settings.GetValue<bool>("ShowWatermark");
                Game.WatermarkText = $"Distance {API.Version.DistanceBuild} ([00AADD]Spectrum[-] {API.Version.APILevel.ToString()})";
            };

            if (Global.Settings.GetValue<bool>("LoadScripts"))
            {
                TryInitializeLua();
                StartLua();
            }

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

            if (!Directory.Exists(Defaults.ScriptDirectory))
            {
                Console.WriteLine("Lua autoload script directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.ScriptDirectory);
            }

            if (!Directory.Exists(Defaults.OnDemandScriptDirectory))
            {
                Console.WriteLine("Lua on-demand script directory does not exist. Creating...");
                Directory.CreateDirectory(Defaults.OnDemandScriptDirectory);
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

        private void InitializeScriptHotkeys()
        {
            try
            {
                ScriptHotkeySettings = new Settings(typeof(Manager), "Hotkeys");

                foreach (var s in ScriptHotkeySettings)
                {
                    if (!string.IsNullOrEmpty(s.Key) && !string.IsNullOrEmpty(s.Value))
                    {
                        Hotkeys.Bind(s.Key, s.Value);
                    }
                }
            }
            catch
            {
                Console.WriteLine("MANAGER: Couldn't load script hotkeys.");
            }
        }

        private void TryInitializeLua()
        {
            if (CanLoadScripts)
            {
                LuaLoader = new Loader(ScriptDirectory, OnDemandScriptDirectory);
                LuaLoader.LoadAll();
            }
            else
            {
                Console.WriteLine($"Can't load or execute scripts. Directory '{ScriptDirectory}' does not exist.");
            }
        }

        private void StartLua()
        {
            if (CanLoadScripts)
            {
                LuaExecutor = new Executor(LuaLoader);
                LuaExecutor.ExecuteAll();
            }
        }

        private void LoadExtensions()
        {
            ManagedPluginContainer = new PluginContainer();
            ManagedPluginLoader = new PluginLoader(PluginDirectory, ManagedPluginContainer);
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
