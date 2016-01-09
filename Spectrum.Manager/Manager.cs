using System;
using Spectrum.Manager.Lua;
using Spectrum.Manager.Resources;

namespace Spectrum.Manager
{
    public class Manager
    {
        private Loader LuaLoader { get; set; }
        private Executor LuaExecutor { get; set; }

        public Manager()
        {
            InitializeLua();
            StartLua();

        }

        public void UpdateExtensions()
        {
            Console.WriteLine("update");
        }

        private void InitializeLua()
        {
            LuaLoader = new Loader(DefaultValues.ScriptDirectory);
            LuaLoader.LoadScripts();
        }

        private void StartLua()
        {
            LuaExecutor = new Executor(LuaLoader);
            LuaExecutor.ExecuteAllScripts();
        }
    }
}
