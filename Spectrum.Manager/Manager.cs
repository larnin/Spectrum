using System;
using System.IO;
using Spectrum.API.PluginInterfaces;
using Spectrum.Manager.Lua;
using Spectrum.Manager.Managed;
using Spectrum.Manager.Resources;

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
            ScriptDirectory = DefaultValues.ScriptDirectory;
            PluginDirectory = DefaultValues.PluginDirectory;

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
                Console.WriteLine("Initializing scripts...");

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
                Console.WriteLine("Executing all scripts...");
                LuaExecutor = new Executor(LuaLoader);
                LuaExecutor.ExecuteAllScripts();
            }
        }

        private void LoadExtensions()
        {
            Console.WriteLine("Initializing extensions...");

            ManagedPluginContainer = new PluginContainer();
            ManagedPluginLoader = new PluginLoader(PluginDirectory, ManagedPluginContainer);
            ManagedPluginLoader.LoadPlugins();
        }

        private void StartExtensions()
        {
            Console.WriteLine("Starting up all loaded extensions...");
            foreach (var pluginInfo in ManagedPluginContainer)
            {
                pluginInfo.Plugin.Initialize(this);
            }
        }
    }
}
