using System;
using Spectrum.API.Game.EventArgs.Scene;

namespace Spectrum.API.Game
{
    public class Scene
    {
        public static event EventHandler<SceneLoadedEventArgs> Loaded;

        static Scene()
        {
            Events.Scene.LoadFinish.Subscribe(data =>
            {
                var eventArgs = new SceneLoadedEventArgs(data.sceneName);
                Loaded?.Invoke(null, eventArgs);
            });
        }
    }
}
