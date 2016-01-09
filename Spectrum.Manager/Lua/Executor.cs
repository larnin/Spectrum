using System;

namespace Spectrum.Manager.Lua
{
    class Executor
    {
        private Loader LuaLoader { get; }
        public NLua.Lua Lua { get; set; }

        public Executor(Loader luaLoader)
        {
            LuaLoader = luaLoader;
            InitializeLua();
        }

        public void ExecuteAllScripts()
        {
            Console.WriteLine("Lua execution started.");

            foreach (var path in LuaLoader.ScriptPaths)
            {
                try
                {
                    Console.Write($"Executing {path}... ");
                    Lua.DoFile(path);
                    Console.WriteLine("OK.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failure:\n{ex.Message}\nInner: {ex.InnerException?.Message}");
                }
            }
        }

        private void InitializeLua()
        {
            Console.Write("Initializing Lua subsystem... ");
            Lua = new NLua.Lua();
            Lua.LoadCLRPackage();

            Lua.DoString("print(_VERSION)");
            Console.WriteLine();
        }
    }
}
