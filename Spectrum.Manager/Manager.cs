using Spectrum.Manager.Lua;
using Spectrum.LuaHelper;

namespace Spectrum.Manager
{
    public class Manager
    {
        private Loader LuaLoader { get; }
        private Executor LuaExecutor { get; }

        private Entry LuaHelperEntry { get; }

        public Manager()
        {
            LuaLoader = new Loader("Distance_Data/Spectrum/Scripts");
            LuaLoader.LoadScripts();

            LuaExecutor = new Executor(LuaLoader);
            LuaHelperEntry = new Entry(LuaExecutor.Lua);

            LuaExecutor.ExecuteAllScripts();
        }

        public void UpdateExtensions()
        {
            
        }
    }
}
