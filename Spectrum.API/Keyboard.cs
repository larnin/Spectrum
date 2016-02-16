namespace Spectrum.API
{
    public class Keyboard
    {
        public static bool IsKeyPressed(string key)
        {
            return UnityEngine.Input.GetKey(key);
        }

        public static bool IsKeyDown(string key)
        {
            return UnityEngine.Input.GetKeyDown(key);
        }

        public static bool IsKeyUp(string key)
        {
            return UnityEngine.Input.GetKeyUp(key);
        }
    }
}
