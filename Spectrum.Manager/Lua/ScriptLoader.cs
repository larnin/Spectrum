using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectrum.API;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    public class ScriptLoader
    {
        private SubsystemLog Log { get; }
        private FileSystemWatcher ScriptChangeWatcher { get; }

        public string ScriptFolder { get; }
        public string OnDemandScriptFolder { get; }

        public List<string> ScriptPaths { get; private set; }
        public List<string> OnDemandScriptPaths { get; private set; }

        public event EventHandler OnDemandScriptsLoaded;
        public event EventHandler StartupScriptsLoaded;

        public ScriptLoader(string scriptFolder, string onDemandScriptFolder)
        {
            ScriptChangeWatcher = new FileSystemWatcher(scriptFolder, "*.lua")
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            ScriptChangeWatcher.Changed += ScriptChangeWatcher_Changed;
            ScriptChangeWatcher.Created += ScriptChangeWatcher_Created;
            ScriptChangeWatcher.Renamed += ScriptChangeWatcher_Renamed;
            ScriptChangeWatcher.Deleted += ScriptChangeWatcher_Deleted;
            ScriptChangeWatcher.EnableRaisingEvents = true;

            ScriptFolder = scriptFolder;
            OnDemandScriptFolder = onDemandScriptFolder;

            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaLoaderLogFileName));
            Log.Info("Lua loader starting up...");
        }

        private void ScriptChangeWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            LoadStartupScripts();
        }

        private void ScriptChangeWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            LoadStartupScripts();
        }

        private void ScriptChangeWatcher_Created(object sender, FileSystemEventArgs e)
        {
            LoadStartupScripts();
        }

        private void ScriptChangeWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadStartupScripts();
        }

        private void LoadStartupScripts()
        {
            try
            {
                ScriptPaths = Directory.GetFiles(ScriptFolder, "*.lua").ToList();
                Log.Info($"Loaded {ScriptPaths.Count} startup scripts.");

                StartupScriptsLoaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.Error("Error occured while reloading Lua startup scripts.");
                Log.Exception(ex);
            }
        }

        private void LoadOnDemandScripts()
        {
            try
            {
                OnDemandScriptPaths = Directory.GetFiles(OnDemandScriptFolder, "*.lua").ToList();
                Log.Info($"Loaded {OnDemandScriptPaths.Count} on-demand scripts.");

                OnDemandScriptsLoaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.Error("Error occured while reloading Lua on-demand scripts.");
                Log.Exception(ex);
            }
        }

        public void LoadAll()
        {
            try
            {
                LoadStartupScripts();
                LoadOnDemandScripts();
            }
            catch(Exception ex)
            {
                Log.Error("Error occured while loading all Lua scripts.");
                Log.Exception(ex);
            }
        }
    }
}
