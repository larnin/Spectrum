namespace Spectrum.API.PluginInterfaces
{
    public interface IPlugin
    {
        string FriendlyName { get; }
        string Author { get; }
        string Contact { get; }
        int CompatibleAPILevel { get; }

        void Initialize(params object[] args);
        void Shutdown(params object[] args);
    }
}
