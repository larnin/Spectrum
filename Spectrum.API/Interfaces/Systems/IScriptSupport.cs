namespace Spectrum.API.Interfaces.Systems
{
    public interface IScriptSupport
    {
        void AddCLRPackage(string assemblyName, string wantedNamespace);
        void AddGlobalAlias(object obj, string name);

        void ExecuteScript(string fileName);
    }
}
