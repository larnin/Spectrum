using System;
using System.IO;
using Spectrum.API;
using Spectrum.API.Logging;

namespace Spectrum.Manager.Lua
{
    internal class ScriptExecutor
    {
        private NLua.Lua LuaState { get; }
        private ScriptLoader LuaLoader { get; }

        private Logger Log { get; }

        public ScriptExecutor(NLua.Lua luaState, ScriptLoader luaLoader)
        {
            LuaState = luaState;
            LuaLoader = luaLoader;
            LuaLoader.StartupScriptsLoaded += LuaLoader_StartupScriptsLoaded;

            Log = new Logger(Defaults.LuaExecutorLogFileName)
            {
                WriteToConsole = Global.Settings.GetValue<bool>("LogToConsole")
            };
        }

        private void LuaLoader_StartupScriptsLoaded(object sender, EventArgs e)
        {
            Log.Info("Reloading changed files...");
            ExecuteAllStartupScripts();
        }

        public void Execute(string name)
        {
            foreach (var path in LuaLoader.OnDemandScriptPaths)
            {
                var fileName = Path.GetFileName(path);
                if (fileName == name)
                {
                    try
                    {
                        LuaState.DoFile(path);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failure:\n{ex.Message}\n    Inner: {ex.InnerException?.Message}\n    File: {path}");
                    }
                }
            }
        }

        public void ExecuteAllStartupScripts()
        {
            foreach (var path in LuaLoader.ScriptPaths)
            {
                try
                {
                    LuaState.DoFile(path);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failure:\n{ex.Message}\n    Inner: {ex.InnerException?.Message}\n    File: {path}");
                }
            }
        }
    }
}
