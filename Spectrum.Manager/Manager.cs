using System;
using System.IO;
using Spectrum.API;
using Spectrum.API.Configuration;
using Spectrum.API.PluginInterfaces;
using Spectrum.Manager.Lua;
using Spectrum.Manager.Managed;

namespace Spectrum.Manager
{
    public class Manager
    {
        private Loader LuaLoader { get; set; }
        private Executor LuaExecutor { get; set; }

        private PluginContainer ManagedPluginContainer { get; set; }
        private PluginLoader ManagedPluginLoader { get; set; }

        private string ScriptDirectory { get; }
        private string PluginDirectory { get; }

        public bool CanLoadScripts => Directory.Exists(ScriptDirectory);
        public bool CanLoadPlugins => Directory.Exists(PluginDirectory);
        public Settings Settings { get; private set; }


        public Manager()
        {
            InitializeSettings();

            ScriptDirectory = Defaults.ScriptDirectory;
            PluginDirectory = Defaults.PluginDirectory;

            if (Settings.GetValue<bool>("LoadScripts"))
            {
                TryInitializeLua();
                StartLua();
            }

            if (Settings.GetValue<bool>("LoadPlugins"))
            {
                LoadExtensions();
                StartExtensions();
            }
        }

        public void UpdateExtensions()
        {
            if (ManagedPluginContainer != null)
            {
                foreach (var pluginInfo in ManagedPluginContainer)
                {
                    if (pluginInfo.Enabled && pluginInfo.IsUpdatable)
                    {
                        ((IUpdatable)pluginInfo.Plugin).Update();
                    }
                }
            }
        }

        private void InitializeSettings()
        {
            try
            {
                Settings = new Settings(typeof(Manager));
                if (Settings["FirstRun"] == string.Empty || Settings.GetValue<bool>("FirstRun"))
                {
                    RecreateSettings();
                }
            }
            catch
            {
                Console.WriteLine("MANAGER: Couldn't load settings. Defaults loaded.");
            }
        }

        private void RecreateSettings()
        {
            Settings["FirstRun"] = "false";
            Settings["LoadPlugins"] = "true";
            Settings["LoadScripts"] = "true";

            Settings.Save();
        }

        private void TryInitializeLua()
        {
            if (CanLoadScripts)
            {
                LuaLoader = new Loader(ScriptDirectory);
                LuaLoader.LoadScripts();
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
                LuaExecutor.ExecuteAllScripts();
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
