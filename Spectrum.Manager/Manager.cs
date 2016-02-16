using System;
using System.IO;
using Spectrum.API;
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

        public Manager()
        {
            ScriptDirectory = Defaults.ScriptDirectory;
            PluginDirectory = Defaults.PluginDirectory;

            TryInitializeLua();
            StartLua();

            LoadExtensions();
            StartExtensions();
        }

        public void UpdateExtensions()
        {
            foreach (var pluginInfo in ManagedPluginContainer)
            {
                if (pluginInfo.Enabled && pluginInfo.IsUpdatable)
                {
                    ((IUpdatable)pluginInfo.Plugin).Update();
                }
            }
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
