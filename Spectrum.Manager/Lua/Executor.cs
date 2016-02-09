using System;
using System.IO;
using Spectrum.Manager.Logging;
using Spectrum.Manager.Resources;

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
            Log = new SubsystemLog(Path.Combine(DefaultValues.LogDirectory, DefaultValues.LuaExecutorLogFileName), true);

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
                    Log.Exception(ex);
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
                Log.Exception(ex);
            }
        }
    }
}
