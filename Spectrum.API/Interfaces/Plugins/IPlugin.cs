using Spectrum.API.Interfaces.Systems;

namespace Spectrum.API.Interfaces.Plugins
{
    public interface IPlugin
    {
        string FriendlyName { get; }
        string Author { get; }
        string Contact { get; }
        APILevel CompatibleAPILevel { get; }

        void Initialize(IManager manager);
        void Shutdown();
    }
}
