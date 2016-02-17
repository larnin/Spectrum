using System;
using UnityEngine;

namespace Spectrum.API.Input
{
    public class Keyboard
    {
        public static bool IsKeyPressed(string key)
        {
            return UnityEngine.Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), key));
        }

        public static bool IsKeyDown(string key)
        {
            return UnityEngine.Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), key));
        }

        public static bool IsKeyUp(string key)
        {
            return UnityEngine.Input.GetKeyUp((KeyCode)Enum.Parse(typeof(KeyCode), key));
        }
    }
}
