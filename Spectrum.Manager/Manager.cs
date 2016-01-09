using Spectrum.Manager.Lua;
using Spectrum.Manager.Resources;

namespace Spectrum.Manager
{
    public class Manager
    {
        private Loader LuaLoader { get; }
        private Executor LuaExecutor { get; }

        public Manager()
        {
            LuaLoader = new Loader(DefaultValues.ScriptDirectory);
            LuaLoader.LoadScripts();

            LuaExecutor = new Executor(LuaLoader);

            LuaExecutor.ExecuteAllScripts();
        }

        public void UpdateExtensions()
        {
            
        }
    }
}
