using Spectrum.API;
using Spectrum.API.Game;
using Spectrum.API.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;

namespace Spectrum.Plugins.TutorialPlugin
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Tutorial Plugin";
        public string Author => "AnAwesomeDeveloper";
        public string Contact => "anawesomedev@example.com";
        public APILevel CompatibleAPILevel => APILevel.InfraRed;

        public void Initialize(IManager manager)
        {
            Race.Started += (sender, args) => { LocalVehicle.HUD.SetHUDText("Hello, world!"); };
        }

        public void Shutdown()
        {
            
        }
    }
}
