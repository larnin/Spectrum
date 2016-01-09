using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spectrum.Manager.Lua
{
    public class Loader
    {
        public string ScriptFolder { get; }
        public List<string> ScriptPaths { get; private set; }

        public Loader(string scriptFolder)
        {
            ScriptFolder = scriptFolder;
            ScriptPaths = new List<string>();
        }

        public void LoadScripts()
        {
            if (!Directory.Exists(ScriptFolder))
            {
                Console.WriteLine("Specified Lua scripts directory does not exist.");
                return;
            }

            ScriptPaths = Directory.GetFiles(ScriptFolder, "*.lua").ToList();
        }
    }
}
