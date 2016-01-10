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
            ScriptPaths = Directory.GetFiles(ScriptFolder, "*.lua").ToList();
        }
    }
}
