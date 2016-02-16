using System;
using System.IO;
using Spectrum.API;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    class Executor
    {
        private Loader LuaLoader { get; }
        private SubsystemLog Log { get; }
        public NLua.Lua Lua { get; set; }

        public Executor(Loader luaLoader)
        {
            LuaLoader = luaLoader;
            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaExecutorLogFileName), true);

            InitializeLua();
        }

        public void ExecuteAllScripts()
        {
            foreach (var path in LuaLoader.ScriptPaths)
            {
                try
                {
                    Lua.DoFile(path);
                }
                catch (Exception ex)
                {
                    Log.Error($"    Failure:\n{ex.Message}\n    Inner: {ex.InnerException?.Message}\n    File: {path}");
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
