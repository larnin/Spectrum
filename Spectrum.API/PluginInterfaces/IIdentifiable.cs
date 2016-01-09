namespace Spectrum.API.PluginInterfaces
{
    public interface IIdentifiable
    {
        string Name { get; }
        string Author { get; }
        string Contact { get; }

        int CompatibleAPILevel { get; }
    }
}
