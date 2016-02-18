using System;
using System.IO;
using Spectrum.API;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    class Executor : IExecutor
    {
        private Loader LuaLoader { get; }
        private SubsystemLog Log { get; }
        public NLua.Lua Lua { get; set; }

        public Executor(ILoader luaLoader)
        {
            LuaLoader = (Loader)luaLoader;
            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaExecutorLogFileName));

            InitializeLua();
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
                        Lua.DoFile(path);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failure:\n{ex.Message}\n    Inner: {ex.InnerException?.Message}\n    File: {path}");
                    }
                }
            }
        }

        public void ExecuteAll()
        {
            foreach (var path in LuaLoader.ScriptPaths)
            {
                try
                {
                    Lua.DoFile(path);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failure:\n{ex.Message}\n    Inner: {ex.InnerException?.Message}\n    File: {path}");
                }
            }
        }

        private void InitializeLua()
        {
            try
            {
                Log.Info("Initializing Lua... ", true);
                Lua = new NLua.Lua();
                Lua.LoadCLRPackage();

                var version = (string)Lua.DoString("return _VERSION")[0];
                Log.WriteLine(version);
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while initializing Lua. Check the log for details.");
                Log.Exception(ex);
            }
        }
    }
}
